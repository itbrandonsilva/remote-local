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
using CSCore.Codecs.MP3;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using CSCore.MediaFoundation;

/*
namespace DesktopDupBlagh
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
                var ms = new MemoryStream();
                var encoder = MediaFoundationEncoder.CreateMP3Encoder(capture.WaveFormat, ms);

                capture.DataAvailable += (s, e) =>
                {
                    lock (buffer)
                    {
                        //Buffer.BlockCopy(e.Data, e.Offset, buffer, bufferEnd, e.ByteCount);
                        //bufferEnd += e.ByteCount;
                        encoder.Write(e.Data, e.Offset, e.ByteCount);
                        var encoded = ms.ToArray();
                        Buffer.BlockCopy(encoded, 0, buffer, bufferEnd, encoded.Length);
                        bufferEnd += encoded.Length;
                        ms.Position = 0;
                        ms.SetLength(0);
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
            //var bf = new BinaryFormatter();
            //byte[] bytes = null;
            //using (var ms = new MemoryStream())
            //{
            //    bf.Serialize(ms, capture.WaveFormat.Clone());
            //    bytes = ms.ToArray();
            //}
            //
            //var writer = new BinaryWriter(new MemoryStream());
            //writer.Write(20);
            //writer.Write(bytes.Length);
            //writer.Write(bytes);
            //var reader = new BinaryReader(writer.BaseStream);
            //reader.BaseStream.Position = 0;
            //return reader.ReadBytes((sizeof(int) * 2) + bytes.Length);

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

        public static WaveFormat WaveFormatFromPacket(byte[] packet)
        {
            //using (var memStream = new MemoryStream())
            //{
            //    var binForm = new BinaryFormatter();
            //    var size = BitConverter.ToInt32(packet, 4);
            //    memStream.Write(packet, 8, size);
            //    memStream.Seek(0, SeekOrigin.Begin);
            //    return (WaveFormat)binForm.Deserialize(memStream);
            //}

            Console.WriteLine("SIZE: " + packet.Length);
            int sampleRate = BitConverter.ToInt32(packet, 8);
            int bits = BitConverter.ToInt32(packet, 12);
            int channels = BitConverter.ToInt32(packet, 16);
            AudioEncoding encoding = (AudioEncoding)BitConverter.ToInt32(packet, 20);
            int extraSize = BitConverter.ToInt32(packet, 24);

            Console.WriteLine("Decoded...");
            Console.WriteLine(sampleRate + " " + bits + " " + channels + " " + encoding + " " + extraSize);

            var waveFormat = new WaveFormat(sampleRate, bits, channels, AudioEncoding.IeeeFloat, extraSize);
            //var waveFormat = new WaveFormatExtensible(sampleRate, bits, channels, AudioSubTypes.IeeeFloat);
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

        MediaFoundationDecoder _decoder;
        MFByteStream _stream;

        //public RLocalAudioPlayback(WaveFormatExtensible waveFormat)
        public RLocalAudioPlayback(WaveFormat wf)
        {
            //
            //soundOut = new CSCore.SoundOut.WasapiOut(false, AudioClientShareMode.Exclusive, 33);

            ////wff = new WaveFormat();
            ////var wf = new WaveFormatExtensible(wff.SampleRate, wff.BitsPerSample, wff.Channels, wff.WaveFormatTag);
            ////.WriteLine(wf.SampleRate + " " + wf.BitsPerSample + " " + wf.Channels + " " + wf.WaveFormatTag + " " + wf.ExtraSize);
            //int bufferSize = wf.BytesPerSecond * 5;
            ////audioSource = new RLocalIWaveSource(wf, bufferSize, 20);
            //audioSource = new RLocalIWaveSource(wf, bufferSize);

            //soundOut.Initialize(audioSource);

            var ms = new MemoryStream();
            var _stream = new MFByteStream(ms);
            _decoder = new MediaFoundationDecoder(_stream);
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
            ////if (!isPlaying) return;
            //audioSource.Write(bytes, 8, bytes.Length - 8);
            //if (!firstWrite)
            //{
            //    soundOut.Play();
            //    Console.WriteLine("WE PLAYING");
            //    firstWrite = true;
            //}
            //isPlaying = true;

            //_stream.Write(bytes, 8, bytes.Length - 8);
            if (!firstWrite)
            {
                //soundOut.Play();
                firstWrite = true;
            }
        }
    }

    public class RLocalIWaveSource : IWaveSource
    {
        byte[] buffer;
        int endOfAccess;
        int endOfBytes;
        private readonly WaveFormat _waveFormat;
        public int latency = 0;
        Mp3MediafoundationDecoder _decoder;

        public RLocalIWaveSource(WaveFormat wf, int bufferSize)
        {
            //this._waveFormat = new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat);
            this._waveFormat = wf;
            buffer = new byte[bufferSize];
            Reset();
        }

        public byte[] DecodeMP3(byte[] bytes, int offset, int size)
        {
            var wav = new Byte[0];

            //var ms = new MemoryStream();
            //var decoder = new Mp3MediafoundationDecoder(ms);
            //ms.Write(bytes, offset, size);
            //decoder.Read();
            //decoder.SetPosition(0);

            return wav;
        }

        bool firstWrite = false;
        public int Write(byte[] bytes, int offset, int size)
        {
            lock (buffer)
            {
                if (firstWrite)
                {
                    AlignEndOfAccess();
                    firstWrite = false;
                }

                int remaining = buffer.Length - endOfBytes;

                if (size > remaining)
                {
                    Buffer.BlockCopy(bytes, offset, buffer, endOfBytes, remaining);
                    endOfBytes = 0;
                    Write(bytes, remaining, size - remaining);
                    return size;
                }

                Buffer.BlockCopy(bytes, offset, buffer, endOfBytes, size);

                endOfBytes += size; ;
                return size;
            }
        }

        public int firstReadOffset = 0;

        public int Read(byte[] bytes, int offset, int size)
        {
            return Read(bytes, offset, size, 0);
        }

        public int Read(byte[] bytes, int offset, int size, int readOffset)
        {
            lock (buffer)
            {
                Console.WriteLine("READ");
                //AlignEndOfAccess(size);
                int remainingBytes = buffer.Length - endOfAccess;

                if (remainingBytes < size)
                {
                    Buffer.BlockCopy(buffer, endOfAccess, bytes, offset, remainingBytes);
                    endOfAccess = 0;
                    Read(bytes, offset + remainingBytes, size - remainingBytes);
                    return size;
                }

                Buffer.BlockCopy(buffer, endOfAccess, bytes, offset, size);

                endOfAccess += size;
                return size;
            }
        }

        // AlignEndOfAccess() is a hack to prevent an issue where audio playback falls behind over time
        int hackedCount = 0;
        int hackedInterval = 60;
        public void ShouldWeAlignEndOfAccess(int interval)
        {
            if (hackedInterval == 0) return;
            hackedCount++;
            if (hackedCount > hackedInterval)
            {
                AlignEndOfAccess();

                hackedCount = 0;
                if (hackedInterval == 60) hackedInterval = 60;
            }
        }

        public void AlignEndOfAccess()
        {
            int byteLatency = (int)_waveFormat.MillisecondsToBytes(350);
            int index = endOfBytes - byteLatency;
            if (index < 0) index = buffer.Length - Math.Abs(index);
            if (index > buffer.Length) index = index - buffer.Length;
            endOfAccess = RoundDown(index, _waveFormat.BlockAlign);
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

        public bool CanSeek
        {
            get
            {
                return false;
            }
        }
    }
}
*/