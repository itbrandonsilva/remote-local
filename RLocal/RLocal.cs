using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using SharpDX.Windows;
using System.IO;
using System.Diagnostics;

public struct RLocalOptions
{
    public IPAddress bindAddress;
    public IPAddress hostAddress;
    public int port;
    public bool enableSound;
    public int desiredFps;
    public int outWidth;
    public int outHeight;
}

namespace RLocal
{
    public partial class RLocal : Form
    {
        RLocalRenderer renderer;
        RLocalTranscoder transcoder;
        RLocalConnection connection;
        RLocalDesktopCapture videoCapture;
        RLocalAudioCapture audioCapture;
        RLocalAudioPlayback audioPlayback;
        RLocalInputManager inputManager;
        RLocalConsoleManager consoleManager;

        RLocalOptions options;

        RLocalVJoy vjoy;

        byte[] DecodedBytes;

        public RLocal()
        {
            options = new RLocalOptions();
            InitializeComponent();
            int monitorCount = Screen.AllScreens.Length;
            int i = 0;
            while (i < monitorCount)
            {
                MonitorComboBox.Items.Add(i);
                ++i;
            }

            MonitorComboBox.SelectedIndex = 0;


            inputManager = new RLocalInputManager();
            inputManager.DiscoverInputDevices().ForEach(input =>
            {
                InputSourceComboBox.Items.Add(input.name);
                Console.WriteLine(input.name);
                Debug.WriteLine(input.name);
            });
            InputSourceComboBox.SelectedIndex = 0;
        }

        private void Start(bool server)
        {
            if (!ValidateOptions(server)) return;

            connection = new RLocalConnection();
            transcoder = new RLocalTranscoder();

            if (server) StartServer();
            else StartClient();
        }

        private void StartClient()
        {
            inputManager.AssignDevice(InputSourceComboBox.SelectedIndex);
            connection.StartClient(options.hostAddress, options.port, new RunWorkerCompletedEventHandler(PacketHandler));
            AsyncProcessInputs();
        }

        private async void EncodeAndBroadcastFrame()
        {
            await Task.Run(() =>
            {
                var locked = !Monitor.TryEnter(transcoder);
                if (!locked)
                {
                    int s = transcoder.EncodeFrame(videoCapture.FrameBytes, connection.m_writeBuffer);
                    connection.BroadcastBytes(null, s);
                    Monitor.Exit(transcoder);
                }
            });
        }

        private void StartServer()
        {
            connection.StartServer(options.bindAddress, options.port, new RunWorkerCompletedEventHandler(NewClientHandler));

            videoCapture = new RLocalDesktopCapture(MonitorComboBox.SelectedIndex);
            AllocTranscoderContext();
            transcoder.AllocEncoder();
            transcoder.AllocDecoder();

            videoCapture.Capture((sender, e) =>
            {
                EncodeAndBroadcastFrame();
            });

            //DecodedBytes = new byte[RLocalUtils.GetSizeBGRA(options.outWidth, options.outHeight)];

            //renderer = new RLocalRenderer(options.outWidth, options.outHeight);

            PrepareVJoy();

            if (options.enableSound)
            {
                audioCapture = new RLocalAudioCapture((bytes, size) =>
                {
                    BinaryWriter writer = new BinaryWriter(new MemoryStream());
                    writer.Write(21);
                    writer.Write(size);
                    writer.Write(bytes);

                    BinaryReader reader = new BinaryReader(writer.BaseStream);
                    reader.BaseStream.Position = 0;
                    int packetSize = sizeof(int) * 2 + size;
                    byte[] packet = reader.ReadBytes(packetSize);
                    connection.BroadcastBytes(packet, packetSize);
                });
            }

            //renderer.Start();
        }

        private void AllocTranscoderContext()
        {
            if (videoCapture != null)
            {
                transcoder.AllocContext(videoCapture.DisplayWidth, videoCapture.DisplayHeight, options.outWidth, options.outHeight);
            }
            else {
                transcoder.AllocContext(0, 0, options.outWidth, options.outHeight);
            }
        }

        private void NewClientHandler(object sender, RunWorkerCompletedEventArgs eargs)
        {
            var client = (RLocalClient)eargs.Result;

            vjoy.AcquireDevice((uint)client.playerId);

            byte[] optionsPacket = BuildOptionsPacket();
            client.stream.Write(optionsPacket, 0, optionsPacket.Length);

            if (options.enableSound)
            {
                byte[] audioPacket = audioCapture.WaveFormatToPacket();
                client.stream.Write(audioPacket, 0, audioPacket.Length);
            }

            connection.StartReaderWorker(client, new RunWorkerCompletedEventHandler(PacketHandler));
        }

        private byte[] BuildOptionsPacket()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(45);
            writer.Write(8);
            writer.Write(options.outWidth);
            writer.Write(options.outHeight);

            BinaryReader reader = new BinaryReader(writer.BaseStream);
            reader.BaseStream.Position = 0;
            byte[] packet = reader.ReadBytes(sizeof(int) * 4);
            return packet;
        }

        private void DecodeFrame(byte[] input, byte[] output)
        {
            transcoder.DecodeFrame(input, output);
        }

        private void RenderLoop()
        {
            while (true)
            {
                if (renderer != null) break;
            }
            renderer.Start();
        }

        private void PacketHandler(object sender, RunWorkerCompletedEventArgs eargs)
        {
            if (eargs.Result == null) return;
            RLocalIncomingMessage message = (RLocalIncomingMessage)eargs.Result;
            switch (message.type)
            {
                case 3: // Frame
                    if (renderer == null) break;
                    DecodeFrame(message.bytes, renderer.RenderBuffer);
                    break;
                case 9: // Button
                    RLocalButtonState buttonState = RLocalButtonState.FromPacket(message.bytes);
                    vjoy.SetButtonState((uint)message.playerId, RLocalInput.MapIdToButton[buttonState.button], buttonState.value);
                    break;
                case 20: // Audio Format
                    var waveFormat = RLocalAudioCapture.WaveFormatFromPacket(message.bytes);
                    audioPlayback = new RLocalAudioPlayback(waveFormat);
                    break;
                case 21: // Audio Data
                    if (audioPlayback == null) break;
                    audioPlayback.Write(message.bytes, 8, message.bytes.Length - 8);
                    break;
                case 45: // Video Format
                    options.outWidth = BitConverter.ToInt32(message.bytes, 8);
                    options.outHeight = BitConverter.ToInt32(message.bytes, 12);

                    renderer = new RLocalRenderer(options.outWidth, options.outHeight);
                    ThreadPool.QueueUserWorkItem((a) =>
                    {
                        StartRender();
                    });
                    break;
            };
        }

        delegate void StartRenderCallback();
        private void StartRender()
        {
            if (renderer.renderForm.InvokeRequired)
            {
                var cb = new StartRenderCallback(StartRender);
                this.Invoke(cb, new object[] { });
            }
            AllocTranscoderContext();
            transcoder.AllocDecoder();
            DecodedBytes = new byte[RLocalUtils.GetSizeBGRA(options.outWidth, options.outHeight)];
            renderer.Start();
        }

        private void PrepareVJoy()
        {
            vjoy = new RLocalVJoy();
        }

        private void AsyncProcessInputs()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs ea) =>
            {
                List<RLocalButtonState> buttonStates = inputManager.PollInputs();
                foreach (var buttonState in buttonStates)
                {
                    byte[] packet = buttonState.ToPacket();
                    connection.WriteBytes(packet, packet.Length);
                    //buttonState.Print();
                }
            });
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                worker.RunWorkerAsync();
            });
            worker.RunWorkerAsync();
        }

        private void ClientButton_Click(object sender, EventArgs e)
        {
            Start(false);
        }

        private void ServerButton_Click(object sender, EventArgs e)
        {
            Start(true);
        }

        public bool ValidateOptions(bool isServer)
        {
            IPAddress hostAddress = null;
            IPAddress bindAddress = null;
            int port = 0;
            int monitor = 0;
            int desiredFps = 0;
            int outWidth = 0;
            int outHeight = 0;

            if (!Int32.TryParse(PortTextBox.Text, out port))
            {
                MessageBox.Show("Port is not a valid integer.", "Error");
                return false;
            }

            if (isServer)
            {
                if (!IPAddress.TryParse(BindAddressTextBox.Text, out bindAddress))
                {
                    MessageBox.Show("Bind Address specified is invalid.", "Error");
                    return false;
                }

                if (!Int32.TryParse(MonitorComboBox.Text, out monitor))
                {
                    MessageBox.Show("Monitor specified is invalid.", "Error");
                    return false;
                }

                if (!Int32.TryParse(FramerateTextBox.Text, out desiredFps))
                {
                    MessageBox.Show("Framerate specified is invalid.", "Error");
                    return false;
                }

                if (!Int32.TryParse(OutWidthTextBox.Text, out outWidth))
                {
                    MessageBox.Show("Out Width specified is invalid.", "Error");
                    return false;
                }

                if (!Int32.TryParse(OutHeightTextBox.Text, out outHeight))
                {
                    MessageBox.Show("Out Height specified is invalid.", "Error");
                    return false;
                }
            }
            else
            {
                if (!IPAddress.TryParse(HostAddressTextBox.Text, out hostAddress))
                {
                    MessageBox.Show("Host Address specified is invalid.", "Error");
                    return false;
                }
            }

            options.hostAddress = hostAddress;
            options.bindAddress = bindAddress;
            options.port = port;
            options.enableSound = SoundCheckBox.Checked;
            options.desiredFps = desiredFps;
            options.outWidth = outWidth;
            options.outHeight = outHeight;
            return true;
        }

        private void ToggleDebugConsoleButton_Click(object sender, EventArgs e)
        {
            ToggleConsole();
        }

        public void ToggleConsole()
        {
            if (consoleManager == null)
            {
                consoleManager = new RLocalConsoleManager();
            }
            consoleManager.ToggleConsoleWindow();
        }
    }
}
