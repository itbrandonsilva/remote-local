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

namespace DesktopDup
{
    public partial class RLocal : Form
    {
        RLocalRenderer renderer;
        RLocalTranscoder transcoder;
        RLocalConnection connection;
        RLocalDesktopCapture videoCapture;
        RLocalAudioCapture audioCapture;
        RLocalAudioPlayback audioPlayback;

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
        }

        private void Start(bool server)
        {
            if (!ValidateOptions(server)) return;

            connection = new RLocalConnection();
            transcoder = new RLocalTranscoder();

            if (server) StartServer();
            else StartClient();

            //RenderLoop();
        }

        private void StartClient()
        {
            connection.StartClient(options.hostAddress, options.port, new RunWorkerCompletedEventHandler(PacketHandler));
            AsyncProcessInputs();
            //RenderLoop();
        }

        private void StartServer()
        {
            videoCapture = new RLocalDesktopCapture(MonitorComboBox.SelectedIndex);
            AllocTranscoderContext();
            DecodedBytes = new byte[RLocalUtils.GetSizeBGRA(options.outWidth, options.outHeight)];
            transcoder.AllocEncoder();
            transcoder.AllocDecoder();
            renderer = new RLocalRenderer(options.outWidth, options.outHeight);

            if (options.enableSound)
            {
                audioCapture = new RLocalAudioCapture();
                audioCapture.StartCapture();
            }

            connection.StartServer(options.bindAddress, options.port, new RunWorkerCompletedEventHandler(NewClientHandler));
            PrepareVJoy();
            AsyncEncodeForever();
            renderer.Start();
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
            Client client = (Client)eargs.Result;

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

        private void AudioCaptureDataHandler(byte[] bytes, int offset, int size)
        {

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
            RLocalIncomingMessage message = (RLocalIncomingMessage)eargs.Result;

            //Console.WriteLine("GOT PACKET: " + message.type);
            //if (message.bytes == null) Console.WriteLine("BYTES NULL");
            switch (message.type)
            {
                case 3:
                    ///Console.WriteLine("DECODED!?!?!?");
                    if (renderer == null)
                    {
                        Console.WriteLine("RENDERER NULL");
                        break;
                    }
                    //Console.WriteLine("YEE");
                    DecodeFrame(message.bytes, renderer.RenderBuffer);
                    break;
                case 9:
                    RLocalButtonState buttonState = RLocalButtonState.FromPacket(message.bytes);
                    //buttonState.Print();
                    vjoy.SetButtonState(1, buttonState.button, buttonState.value);
                    break;
                case 20:
                    var format = RLocalAudioCapture.WaveFormatFromPacket(message.bytes);
                    audioCapture = new RLocalAudioCapture();
                    //audioPlayback = new RLocalAudioPlayback(audioCapture.capture.WaveFormat);
                    audioPlayback = new RLocalAudioPlayback(format);
                    audioPlayback.Play(0);
                    break;
                case 21:
                    if (audioPlayback == null)
                    {
                        Console.WriteLine("AUDIO NULL");
                        break;
                    }
                    audioPlayback.Write(message.bytes);
                    break;
                case 45:
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
            vjoy.AcquireDevice(1);
        }

        private async void AsyncProcessInputs()
        {
            await Task.Delay(100);

            RLocalGamepadInput input = new RLocalGamepadInput();
            var gamepads = input.GetAvailableGamepads();
            input.PrintGamepads(gamepads);
            input.AssignGamepad(gamepads[0]);

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs ea) =>
            {
                List<RLocalButtonState> buttonStates = input.PollInputs();
                foreach (var buttonState in buttonStates)
                {
                    byte[] packet = buttonState.ToPacket();
                    connection.WriteBytes(packet, packet.Length);
                    buttonState.Print();
                }
            });
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                worker.RunWorkerAsync();
            });
            worker.RunWorkerAsync();
        }

        private void AsyncEncodeForever()
        {
            Task.Delay(10).Wait();
            long processTime = 0;

            BackgroundWorker EncoderWorker = new BackgroundWorker();
            EncoderWorker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs ea) =>
            {
                processTime = DateTime.Now.Ticks;
                int size = transcoder.EncodeFrame(videoCapture.FrameBytes, connection.m_writeBuffer);
                DecodeFrame(connection.m_writeBuffer, renderer.RenderBuffer);

                if (options.enableSound)
                {
                    byte[] audioPacket = GetAudioBytes();
                    if (audioPacket.Length > 0)
                    {
                        Buffer.BlockCopy(audioPacket, 0, connection.m_writeBuffer, size, audioPacket.Length);
                        size += audioPacket.Length;
                    }
                }

                //Console.WriteLine("BROADCASTING SIZE: " + size);
                connection.BroadcastBytes(null, size);
            });
            EncoderWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) =>
            {
                long frameTime = 1000 / options.desiredFps;
                long timeSpent = (DateTime.Now.Ticks - processTime) / 10000;
                int TimeToWait = (int)Math.Min(frameTime, Math.Max(frameTime - timeSpent, 0));
                Task.Delay(TimeToWait).Wait();
                EncoderWorker.RunWorkerAsync();
            });
            EncoderWorker.RunWorkerAsync();
        }

        private byte[] GetAudioBytes()
        {
            byte[] bytes = audioCapture.FlushBuffer();
            int size = bytes.Length;
            if (size == 0) return new byte[0];

            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(21);
            writer.Write(size);
            writer.Write(bytes);

            BinaryReader reader = new BinaryReader(writer.BaseStream);
            reader.BaseStream.Position = 0;
            byte[] packet = reader.ReadBytes(sizeof(int) * 2 + size);
            return packet;
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
    }
}
