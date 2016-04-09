using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Threading;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

[StructLayout(LayoutKind.Sequential), Serializable]
public struct TranscoderOptions
{
    public int InputWidth;
    public int InputHeight;
    public int OutputWidth;
    public int OutputHeight;
}

namespace DesktopDup
{
    //public partial class Window : Form
    public class RLocalTranscoder
    {
        IntPtr transcoderContext;
        public TranscoderOptions transcoderOptions;
        IntPtr transcoderOptionsPtr;
        public int InputWidth() { return transcoderOptions.InputWidth; }
        public int InputHeight() { return transcoderOptions.InputHeight; }
        public int OutputWidth() { return transcoderOptions.OutputWidth; }
        public int OutputHeight() { return transcoderOptions.OutputHeight; }

        bool EncoderAvailable = false;
        bool DecoderAvailable = false;

        BackgroundWorker EncoderWorker;

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr AllocContext(IntPtr transcoderOptions);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr FreeContext(IntPtr transcoderOptions);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr AllocEncoder(IntPtr transcoderContext);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr AllocDecoder(IntPtr transcoderContext);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int EncodeFrame(IntPtr transcoderContext, byte[] bgraInput, byte[] packetOutput);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr DecodeFrame(IntPtr transcoderContext, byte[] packetInput, byte[] bgraOutput);

        public RLocalTranscoder()
        {
            transcoderOptions = new TranscoderOptions();
        }

        ~RLocalTranscoder()
        {
            if (transcoderContext != null)
            {
                //FreeContext(transcoderContext);
            }
        }

        public void AllocContext(int iwidth, int iheight, int owidth, int oheight)
        {
            transcoderOptions.InputWidth = iwidth;
            transcoderOptions.InputHeight = iheight;
            transcoderOptions.OutputWidth = owidth;
            transcoderOptions.OutputHeight = oheight;

            GCHandle pinned = GCHandle.Alloc(transcoderOptions, GCHandleType.Pinned);
            transcoderOptionsPtr = pinned.AddrOfPinnedObject();

            transcoderContext = AllocContext(transcoderOptionsPtr);
        }

        public void AllocEncoder()
        {
            AllocEncoder(transcoderContext);
        }

        public void AllocDecoder()
        {
            AllocDecoder(transcoderContext);
            DecoderAvailable = true;
        }

        private int GetInputSize() { return GetNumBytes(InputWidth(), InputHeight()); }
        private int GetOutputSize() { return GetNumBytes(OutputWidth(), OutputHeight()); }
        private int GetNumBytes(int width, int height)
        {
            int stride = 4 * width;
            return stride * height;
        }

        public int EncodeFrame(byte[] InputBytes, byte[] OutputPacket)
        {
            return EncodeFrame(transcoderContext, InputBytes, OutputPacket);
        }

        public void DecodeFrame(byte[] InputPacket, byte[] OutputBytes)
        {
            if (!DecoderAvailable) return;
            lock (OutputBytes)
            {
                DecodeFrame(transcoderContext, InputPacket, OutputBytes);
            }
        }
    }
}
