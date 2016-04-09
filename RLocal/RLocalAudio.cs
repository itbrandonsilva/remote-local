using System;
using CSCore;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Codecs.WAV;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.Streams;
using CSCore.Win32;
using CSCore.Utils.Buffer;
using CSCore.DMO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DesktopDup
{
    public class RLocalAudioCapture
    {
        public WasapiCapture capture;
        public byte[] buffer;
        public int bufferEnd;

        public RLocalAudioCapture()
        {
            //new WaveFormat
            //RLocalIWaveSource soundSource = new RLocalIWaveSource(capture.WaveFormat, bufferSize);

            capture = new WasapiLoopbackCapture();
            capture.Initialize();

            int bufferSize = capture.WaveFormat.BytesPerSecond * 20;
            buffer = new byte[bufferSize];
            FlushBuffer();

            WaveFormat wf = capture.WaveFormat;
            Console.WriteLine(wf.SampleRate + " " + wf.BitsPerSample + " " + wf.Channels + " " + wf.WaveFormatTag + " " + wf.ExtraSize);
        }

        public void StartCapture()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((object sender) =>
            {
                capture.DataAvailable += (s, e) =>
                {
                    lock (buffer)
                    {
                        //captureHandler(e.Data, e.Offset, e.ByteCount);
                        Buffer.BlockCopy(e.Data, e.Offset, buffer, bufferEnd, e.ByteCount);
                        bufferEnd += e.ByteCount;
                    }
                };

                capture.Start();

                Console.ReadKey();
            }));
        }

        public byte[] FlushBuffer()
        {
            lock (buffer)
            {
                if (bufferEnd == 0) return new byte[0];
                byte[] bytes = buffer.Take(bufferEnd).ToArray<byte>();
                bytes = (byte[])bytes.Clone();
                Array.Clear(buffer, 0, bufferEnd);
                Buffer.BlockCopy(buffer, bufferEnd, buffer, 0, bufferEnd);
                bufferEnd = 0;
                return bytes;
            }
        }

        public void StopCapture()
        {
            capture.Stop();
        }

        public byte[] WaveFormatToPacket()
        {
            WaveFormat wf = capture.WaveFormat;

            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(20);
            writer.Write((sizeof(int) * 5));
            writer.Write(wf.SampleRate);
            writer.Write(wf.BitsPerSample);
            writer.Write(wf.Channels);
            writer.Write((int)wf.WaveFormatTag);
            writer.Write(wf.ExtraSize);

            BinaryReader reader = new BinaryReader(writer.BaseStream);
            reader.BaseStream.Position = 0;
            byte[] packet = reader.ReadBytes(sizeof(int) * 7);
            Console.WriteLine("SIZE: " + packet.Length);
            return packet;
        }

        public static WaveFormatExtensible WaveFormatFromPacket(byte[] packet)
        {
            Console.WriteLine("SIZE: " + packet.Length);
            int sampleRate = BitConverter.ToInt32(packet, 8);
            int bits = BitConverter.ToInt32(packet, 12);
            int channels = BitConverter.ToInt32(packet, 16);
            AudioEncoding encoding = (AudioEncoding)BitConverter.ToInt32(packet, 20);
            int extraSize = BitConverter.ToInt32(packet, 24);

            Console.WriteLine(sampleRate + " " + bits + " " + channels + " " + encoding + " " + extraSize);

            //public WaveFormat(int sampleRate, int bits, int channels, AudioEncoding encoding, int extraSize)
            WaveFormatExtensible waveFormat = new WaveFormatExtensible(sampleRate, bits, channels, AudioSubTypes.IeeeFloat);
            return waveFormat;
        }

        public static void PrintWaveFormat(WaveFormat wf)
        {
            Console.WriteLine(wf.SampleRate + " " + wf.BitsPerSample + " " + wf.Channels + " " + wf.WaveFormatTag + " " + wf.ExtraSize);
        }
    }

    public class RLocalAudioPlayback
    {
        RLocalIWaveSource audioSource;
        CSCore.SoundOut.WasapiOut soundOut;
        //CSCore.SoundOut.DirectSoundOut soundOut;
        bool isPlaying = false;

        public RLocalAudioPlayback(WaveFormatExtensible waveFormat)
        {
            soundOut = new CSCore.SoundOut.WasapiOut(true, AudioClientShareMode.Exclusive, 100);

            int bufferSize = waveFormat.BytesPerSecond * 5;
            audioSource = new RLocalIWaveSource(waveFormat, bufferSize, 20);

            soundOut.Initialize(audioSource);
        }

        public async void Play(int latency)
        {
            return;
            isPlaying = true;
            soundOut.Play();
        }

        public void Stop()
        {
            soundOut.Stop();
            isPlaying = false;
            audioSource.Reset();
        }

        bool firstWrite = false;
        public void Write(byte[] bytes)
        {
            //if (!isPlaying) return;
            audioSource.Write(bytes, 8, bytes.Length - 8);
            if (!firstWrite)
            {
                soundOut.Play();
                firstWrite = true;
            }
            //isPlaying = true;
        }
    }

    public class RLocalIWaveSource : IWaveSource
    {
        byte[] buffer;
        int endOfAccess;
        int endOfBytes;
        private readonly WaveFormat _waveFormat;
        public int latency = 0;

        public RLocalIWaveSource(WaveFormat waveFormat, int bufferSize, int latency)
        {
            this.latency = latency;
            this._waveFormat = waveFormat;
            buffer = new byte[bufferSize];
            Reset();
        }

        public int Write(byte[] bytes, int offset, int size)
        {
            lock (buffer)
            {
                int remaining = buffer.Length - endOfBytes;

                if (size > remaining)
                {
                    Buffer.BlockCopy(bytes, offset, buffer, endOfBytes, remaining);
                    endOfBytes = 0;
                    Write(bytes, remaining, size - remaining);
                    return size;
                }

                Buffer.BlockCopy(bytes, offset, buffer, endOfBytes, size);

                endOfBytes += size;;
                return size;
            }
        }

        public int firstReadOffset = 0;

        int iRead = 0;
        int iWritten = 0;

        public int Read(byte[] bytes, int offset, int size)
        {
            return Read(bytes, offset, size, 0);
        }

        public int Read(byte[] bytes, int offset, int size, int readOffset)
        {
            lock (buffer)
            {
                AlignEndOfAccess(size);
                int remainingBytes = buffer.Length - endOfAccess;

                if (remainingBytes < size)
                {
                    Buffer.BlockCopy(buffer, endOfAccess, bytes, offset, remainingBytes);
                    endOfAccess = 0;
                    Read(bytes, offset + remainingBytes, size - remainingBytes);
                    return size;
                }

                Buffer.BlockCopy(buffer, endOfAccess, bytes, offset, size); iRead += size;

                endOfAccess += size;
                return size;
            }
        }

        // AlignEndOfAccess() is a hack to prevent an issue where audio playback falls behind over time
        int hackedCount = 0;
        int hackedInterval = 100;
        public void AlignEndOfAccess(int interval)
        {
            hackedCount++;
            if (hackedCount > hackedInterval)
            {

                int byteLatency = (int)_waveFormat.MillisecondsToBytes(200);
                int index = endOfBytes - byteLatency;
                if (index < 0) index = buffer.Length - Math.Abs(index);
                if (index > buffer.Length) index = index - buffer.Length;
                endOfAccess = RoundDown(index, _waveFormat.BlockAlign);

                hackedCount = 0;
            }
        }

        int RoundDown(int toRound, int interval)
        {
            return toRound - toRound % interval;
        }

        public void Reset()
        {
            lock (buffer)
            {
                Array.Clear(buffer, 0, buffer.Length);
                endOfAccess = 0;
                endOfBytes = 0;
            }
        }

        public long Position
        {
            get
            {
                return 0;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public long Length
        {
            get { return buffer.Length; }
        }

        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        public void Dispose()
        {
        }
    }
}
