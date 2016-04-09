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


namespace DesktopDup
{
    //public partial class Window : Form
    public class RLocalDesktopCapture
    {
        public int DisplayWidth;
        public int DisplayHeight;

        private SharpDX.DXGI.OutputDuplication duplicatedOutput;

        SharpDX.Direct3D11.Device device;
        SharpDX.Direct3D11.Texture2D screenTexture;
        private bool continueCapturing = true;
        BackgroundWorker captureWorker;
        BackgroundWorker EncoderWorker;
        BackgroundWorker DecoderWorker;

        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.DeviceContext d3dDeviceContext;
        private SharpDX.DXGI.SwapChain swapChain;
        private SharpDX.Direct3D11.RenderTargetView renderTargetView;

        protected SharpDX.Direct2D1.Device d2dDevice;
        protected SharpDX.Direct2D1.DeviceContext d2dContext;
        private SharpDX.Direct2D1.Bitmap1 d2dTarget;
        SharpDX.Direct2D1.RenderTarget d2dRenderTarget;

        private System.Threading.Timer TimerItem;
        private System.Threading.Timer TimerItem2;

        public byte[] FrameBytes;
        IntPtr frameBytesPtr;

        public byte[] outBytes;
        IntPtr outBytesPtr;

        public TranscoderOptions transcoderOptions;
        IntPtr transcoderOptionsPtr;

        IntPtr transcoderContext;

        IntPtr aKit;
        int framesCapped = 0;
        int framesEncoded = 0;

        bool capturedFrame = false;
        bool waiting = false;

        public RLocalDesktopCapture(int outputDeviceIndex)
        {
            captureWorker = new BackgroundWorker();
            captureWorker.DoWork += new DoWorkEventHandler(CaptureFrame);
            captureWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(HandleCapturedFrame);

            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            int numOutput = outputDeviceIndex;

            // Create DXGI Factory1
            var factory = new SharpDX.DXGI.Factory1();
            var adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            var output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            DisplayWidth = ((SharpDX.Rectangle)output.Description.DesktopBounds).Width;
            DisplayHeight = ((SharpDX.Rectangle)output.Description.DesktopBounds).Height;

            int stride = 4 * DisplayWidth;
            int size = stride * DisplayHeight;
            FrameBytes = new byte[size];

            GCHandle pinned = GCHandle.Alloc(outBytes, GCHandleType.Pinned);
            outBytesPtr = pinned.AddrOfPinnedObject();
            //pinned.Free();

            GCHandle pinned2 = GCHandle.Alloc(transcoderOptions, GCHandleType.Pinned);
            transcoderOptionsPtr = pinned2.AddrOfPinnedObject();

            GCHandle pinned3 = GCHandle.Alloc(FrameBytes, GCHandleType.Pinned);
            frameBytesPtr = pinned3.AddrOfPinnedObject();

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = DisplayWidth,
                Height = DisplayHeight,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            duplicatedOutput = output1.DuplicateOutput(device);

            CaptureFrameAsync();
            //StartWorkTimer();
        }

        public void StartWorkTimer()
        {
            object obj = null;
            System.Threading.TimerCallback TimerDelegate2 = new System.Threading.TimerCallback(PrintCappedFrames);
            TimerItem2 = new System.Threading.Timer(TimerDelegate2, obj, 0, 1000);

            // Create a timer that calls a procedure every 2 seconds.
            // Note: There is no Start method; the timer starts running as soon as 
            // the instance is created.
            /*System.Threading.TimerCallback TimerDelegate = new System.Threading.TimerCallback(TimerTask);
            int ms = 1000 / fps;
            TimerItem = new System.Threading.Timer(TimerDelegate, obj, 0, ms);*/
        }

        private void PrintCappedFrames(object StateObj)
        {
            System.Console.WriteLine("Frames capped: " + framesCapped);
            System.Console.WriteLine("Frames encoded: " + framesEncoded);
            framesCapped = 0;
            framesEncoded = 0;
        }

        private void CaptureFrameAsync()
        {
            captureWorker.RunWorkerAsync();
        }

        private void HandleCapturedFrame(object sender, RunWorkerCompletedEventArgs e)
        {
            CaptureFrameAsync();
        }


        private void CaptureFrame(object sender, DoWorkEventArgs ea)
        {

            SharpDX.DXGI.Resource screenResource = null;

            try
            {
                OutputDuplicateFrameInformation duplicateFrameInformation;

                // Try to get duplicated frame within given time
                this.duplicatedOutput.AcquireNextFrame(1000, out duplicateFrameInformation, out screenResource);

                // copy resource into memory that can be accessed by the CPU
                using (Texture2D screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);


                DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                int size = RLocalUtils.GetSizeBGRA(DisplayWidth, DisplayHeight);
                Marshal.Copy(mapSource.DataPointer, FrameBytes, 0, size);

                duplicatedOutput.ReleaseFrame();
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    throw e;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            framesCapped++;
        }
    }
}
