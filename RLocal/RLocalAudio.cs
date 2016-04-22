using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using NAudio.Wave;
using System.Windows.Forms;

namespace RLocal
{
    public class RLocalAudioCapture
    {
        public WaveIn waveIn;
        public WaveFormat waveFormat;

        public RLocalAudioCapture(Action<byte[],int> dataAvailable, int rate=44100, int bits=16, int channels=2)
        {
            //waveIn = new WasapiLoopbackCapture();
            //var wf = WaveFormat.CreateIeeeFloatWaveFormat(codec.RecordFormat.SampleRate, codec.RecordFormat.Channels);

            waveFormat = new WaveFormat(rate, bits, channels);
            waveIn = new WaveIn();
            waveIn.BufferMilliseconds = 50;
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = waveFormat;
            waveIn.DataAvailable += (sender, e) =>
            {
                dataAvailable(e.Buffer, e.BytesRecorded);
            };
            waveIn.StartRecording();
        }

        /*private List<WaveInCapabilities> GetAudioSources()
        {
            var capabilities = new List<WaveInCapabilities>();
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var c = WaveIn.GetCapabilities(n);
                capabilities.Add(c);
                //MessageBox.Show(c.ProductName + " " + c.ProductGuid);
            }
            return capabilities;
        }*/

        public void StopCapture()
        {
            waveIn.StopRecording();
        }

        public byte[] WaveFormatToPacket()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(20);
            writer.Write((sizeof(int) * 3));
            writer.Write(waveFormat.SampleRate);
            writer.Write(waveFormat.BitsPerSample);
            writer.Write(waveFormat.Channels);

            Console.WriteLine("FORMAT: " + waveFormat.SampleRate + " " + waveFormat.BitsPerSample + " " + waveFormat.Channels);

            BinaryReader reader = new BinaryReader(writer.BaseStream);
            reader.BaseStream.Position = 0;
            byte[] packet = reader.ReadBytes(sizeof(int) * 5);
            return packet;
        }

        public static WaveFormat WaveFormatFromPacket(byte[] packet)
        {
            int sampleRate =    BitConverter.ToInt32(packet, 8);
            int bitsPerSample = BitConverter.ToInt32(packet, 12);
            int channels =      BitConverter.ToInt32(packet, 16);

            Console.WriteLine("FORMAT: " + sampleRate + " " + bitsPerSample + " " + channels);

            return new WaveFormat(sampleRate, bitsPerSample, channels);
        }
    }

    public class RLocalAudioPlayback
    {
        WaveOut waveOut;
        BufferedWaveProvider waveProvider;

        private readonly Object _lock = new Object();

        //public RLocalAudioPlayback(WaveFormatExtensible waveFormat)
        public RLocalAudioPlayback(WaveFormat waveFormat)
        {
            waveOut = new WaveOut();
            waveProvider = new BufferedWaveProvider(waveFormat);
            waveProvider.BufferDuration = TimeSpan.FromMilliseconds(2000);
            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        double dropPacketsUntil = 0;
        //int align = 0;
        public void Write(byte[] bytes, int offset, int size)
        {
            lock(_lock)
            {
                var seconds = waveProvider.BufferedDuration.TotalSeconds;
                // Drop bytes if we are beginning to desync
                if (seconds > 0.2 || dropPacketsUntil > 0)
                {
                    dropPacketsUntil = 0.2;
                    if (seconds >= dropPacketsUntil) return;
                    dropPacketsUntil = 0;
                }

                waveProvider.AddSamples(bytes, offset, size);
            }
        }

        public void Stop()
        {
            waveOut.Stop();
        }
    }
}