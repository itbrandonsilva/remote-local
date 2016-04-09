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


/*[StructLayout(LayoutKind.Sequential), Serializable]
public struct TranscoderOptions
{
    public int InputWidth;
    public int InputHeight;
    public int OutputWidth;
    public int OutputHeight;
}*/

namespace DesktopDup
{
    //public partial class Window : Form
    public partial class Window : SharpDX.Windows.RenderForm
    {
        Connection conn;

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

        public byte[] frameBytes;
        IntPtr frameBytesPtr;

        public byte[] outBytes;
        IntPtr outBytesPtr;

        public TranscoderOptions transcoderOptions;
        IntPtr transcoderOptionsPtr;

        IntPtr transcoderContext;

        public int DisplayWidth() { return transcoderOptions.InputWidth; }
        public int DisplayHeight() { return transcoderOptions.InputHeight; }
        public int RenderWidth() { return transcoderOptions.OutputWidth; }
        public int RenderHeight() { return transcoderOptions.OutputHeight; }

        IntPtr aKit;
        int framesCapped = 0;
        int framesEncoded = 0;

        //[DllImport("FFmpegCompressor.dll")]
        //public static extern void doNothing();

        bool capturedFrame = false;
        bool waiting = false;

        /*[DllImport("FFmpegCompressor.dll", CallingConvention=CallingConvention.Cdecl)]
        static extern void TranscoderAlloc(int width, int height);

        [DllImport("FFmpegCompressor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void TranscoderFree();

        [DllImport("FFmpegCompressor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void EncodeFrame(byte[] bgraBytes);*/

        //[DllImport("FFmpegCompressor.dll", CallingConvention = CallingConvention.Cdecl)]
        //static extern void DoWork(int w, int h, int fps, byte[] bgraBytes, IntPtr outBytes, IntPtr workerKit);

        //[DllImport("FFmpegCompressor.dll", CallingConvention = CallingConvention.Cdecl)]
        //static extern void TheInit(int w, int h, int fps);

        //[DllImport("FFmpegCompressor.dll", CallingConvention = CallingConvention.Cdecl)]
        //static extern IntPtr BuildKit(int w, int h, int fps);

        //[DllImport("FFmpegCompressor.dll", CallingConvention = CallingConvention.Cdecl)]
        //static extern IntPtr FreeKit(IntPtr kit);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr AllocContext(IntPtr transcoderOptions);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr AllocEncoder(IntPtr transcoderContext);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr AllocDecoder(IntPtr transcoderContext);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int EncodeFrame(IntPtr transcoderContext, byte[] bgraInput, byte[] packetOutput);

        [DllImport("Transcoder.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr DecodeFrame(IntPtr transcoderContext, byte[] packetInput, byte[] bgraOutput);

        public Window()
        {
            transcoderOptions = new TranscoderOptions();
            transcoderOptions.OutputWidth = 1600;
            transcoderOptions.OutputHeight = 900;

            InitializeComponent();
            conn = new Connection();
            this.FormClosed += EventFormClosed;
            Init();
            frameBytes = new byte[GetNumBytesDisplay()];
            //TheInit(dWidth, dHeight, 60);

            //aKit = BuildKit(DesktopWidth, DesktopHeight, fps);
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
            System.Console.WriteLine("Packets emitted: " + conn.packetsSent);
            framesCapped = 0;
            framesEncoded = 0;
            conn.packetsSent = 0;
        }

        private void TimerTask(object StateObj)
        {
            //IntPtr ptr = BuildKit(dWidth, dHeight, fps);
            //DoWork(dWidth, dHeight, fps, frameBytes, outBytesPtr, aKit);
            //FreeKit(ptr);
            EncodeFrameAsync();
        }

        private void EncodeFrameAsync()
        {
            try
            {
                EncoderWorker.RunWorkerAsync();
            } catch (Exception e)
            {

            }
        }

        private void DecodeFrameAsync()
        {
            try
            {
                DecoderWorker.RunWorkerAsync();
            }
            catch (Exception e)
            {

            }
        }

        private void EncodeFrameTask(object sender, DoWorkEventArgs ea)
        {
            EncodeFrame();
        }

        private void DecodeFrameTask(object sender, DoWorkEventArgs ea)
        {
            DecodeFrame();
        }

        private void EncodeFrame()
        {
            //DoWork(dWidth, dHeight, 60, frameBytes, outBytesPtr, aKit);
            //EncodeFrame(transcoderContext, frameBytes, outBytesPtr);
            int size = EncodeFrame(transcoderContext, frameBytes, conn.m_writeBuffer);
            //System.Console.WriteLine("SIZE: " + size);
            conn.BroadcastBytes(size);
            //if (framesEncoded > 5)
            //{
            //DecodeFrame(transcoderContext, conn.m_writeBuffer, outBytes);
            //}
            framesEncoded++;
        }

        private void DecodeFrame()
        {
            conn.ReadBytes();
            //System.Console.WriteLine("BYTES READ: " + transcoderContext);
            DecodeFrame(transcoderContext, conn.m_readBuffer, outBytes);
            //System.Console.WriteLine("BYTES DECODED");
        }

        private void HandleEncodedFrame(object sender, RunWorkerCompletedEventArgs e)
        {
            EncodeFrameAsync();
            //if (waiting) { EncodeFrameAsync(); waiting = false; return; }
        }

        private void HandleDecodedFrame(object sender, RunWorkerCompletedEventArgs e)
        {
            DecodeFrameAsync();
            //if (waiting) { EncodeFrameAsync(); waiting = false; return; }
        }

        public void StartRenderLoop()
        {
            RenderLoop.Run(this, RenderCallback);
        }

        public void StartServer()
        {
            transcoderContext = AllocContext(transcoderOptionsPtr);
            AllocEncoder(transcoderContext);
            //AllocDecoder(transcoderContext);
            CaptureFrameAsync();
            StartWorkTimer();
            EncodeFrameAsync();

            conn.StartServer(47812, "192.168.1.5");
        }

        public void StartClient()
        {
            transcoderContext = AllocContext(transcoderOptionsPtr);
            //AllocEncoder(transcoderContext);
            AllocDecoder(transcoderContext);
            //CaptureFrameAsync();
            //EncodeFrameAsync();
            //StartWorkTimer();
            conn.StartClient(47812, "98.116.235.176");
            DecodeFrameAsync();
            //conn.BytesReceived += ClientHandleReceive;
        }

        //public bool decoding = false;
        public void ClientHandleReceive(object sender, EventArgs e)
        {
            //DecodeFrame(transcoderContext, conn.m_readBuffer, outBytes);
            //conn.ReadBytes();
            //framesCapped++;
        }
        
        private void RenderCallback()
        {
            Draw();
        }

        /*new private void Dispose()
        {
            Parent.Dispose();  
        }*/

        private void EventFormClosed(Object sender, FormClosedEventArgs e)
        {
            continueCapturing = false;
        }

        /*private void CaptureFrame(object sender, DoWorkEventArgs e)
        {
            //Bitmap bmp = GetFrame();
            //e.Result = bmp;
        }*/

        private void Init()
        {
            captureWorker = new BackgroundWorker();
            captureWorker.DoWork += new DoWorkEventHandler(CaptureFrame);
            captureWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(HandleCapturedFrame);

            EncoderWorker = new BackgroundWorker();
            EncoderWorker.DoWork += new DoWorkEventHandler(EncodeFrameTask);
            EncoderWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(HandleEncodedFrame);

            DecoderWorker = new BackgroundWorker();
            DecoderWorker.DoWork += new DoWorkEventHandler(DecodeFrameTask);
            DecoderWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(HandleDecodedFrame);

            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            const int numOutput = 0;

            // Create DXGI Factory1
            var factory = new SharpDX.DXGI.Factory1();
            var adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            var output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            transcoderOptions.InputWidth = ((SharpDX.Rectangle)output.Description.DesktopBounds).Width;
            transcoderOptions.InputHeight = ((SharpDX.Rectangle)output.Description.DesktopBounds).Height;

            outBytes = new byte[GetNumBytesRender()];
            GCHandle pinned = GCHandle.Alloc(outBytes, GCHandleType.Pinned);
            outBytesPtr = pinned.AddrOfPinnedObject();
            //pinned.Free();

            GCHandle pinned2 = GCHandle.Alloc(transcoderOptions, GCHandleType.Pinned);
            transcoderOptionsPtr = pinned2.AddrOfPinnedObject();

            GCHandle pinned3 = GCHandle.Alloc(frameBytes, GCHandleType.Pinned);
            frameBytesPtr = pinned3.AddrOfPinnedObject();

            this.ClientSize = new Size(1600, 900);

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = DisplayWidth(),
                Height = DisplayHeight(),
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            duplicatedOutput = output1.DuplicateOutput(device);

            InitializeDeviceResources();
        }

        public void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription(1600, 900, new Rational(60, 1), Format.B8G8R8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = this.Handle,
                IsWindowed = true
            };

            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.VideoSupport | SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;
            //SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, creationFlags, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>();

            using (SharpDX.Direct3D11.Texture2D backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            {
                renderTargetView = new SharpDX.Direct3D11.RenderTargetView(d3dDevice, backBuffer);
            }

            System.Drawing.Graphics g = this.CreateGraphics();

            SharpDX.Direct2D1.Factory d2dFactory = new SharpDX.Direct2D1.Factory(SharpDX.Direct2D1.FactoryType.SingleThreaded, SharpDX.Direct2D1.DebugLevel.None);

            // Create Direct2D device
            var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
            //d2dDevice = new SharpDX.Direct2D1.Device(d2dFactory, dxgiDevice);
            d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice);
            d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
            //d2dContext.PrimitiveBlend = PrimitiveBlend.SourceOver;

            BitmapProperties1 properties = new BitmapProperties1(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore),
                g.DpiX, g.DpiY, BitmapOptions.Target | BitmapOptions.CannotDraw);

            Surface backBuffer2D = swapChain.GetBackBuffer<Surface>(0);
            //new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)
            SharpDX.Direct2D1.RenderTargetProperties rtp = new SharpDX.Direct2D1.RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore));
            d2dRenderTarget = new SharpDX.Direct2D1.RenderTarget(d2dFactory, backBuffer2D, rtp);
            d2dTarget = new Bitmap1(d2dContext, backBuffer2D, properties);

            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
        }

        private void Window_Load(object sender, EventArgs e) { }

        public void DrawByte(byte[] bytes)
        {
            //System.Drawing.Graphics graphics = this.CreateGraphics();
            //graphics.DrawImage(bmp, 10, 10);


            /*d2dContext.Target = d2dTarget;
            d2dContext.BeginDraw();
            SolidColorBrush solidBrush = new SolidColorBrush(d2dContext, SharpDX.Color.Coral);
            d2dContext.FillRectangle(new SharpDX.RectangleF(50, 50, 200, 200), solidBrush);*/

            //d2dTarget.CopyFromBitmap();
            //d2dContex

            d2dContext.Target = d2dTarget;
            d2dContext.BeginDraw();
            //d2dContext.
            //SolidColorBrush solidBrush = new SolidColorBrush(d2dContext, SharpDX.Color.Coral);
            //d2dContext.FillRectangle(new SharpDX.RectangleF(50, 50, 200, 200), solidBrush);

            //SharpDX.Rectangle rect = new SharpDX.Rectangle(0, 0, dWidth, dHeight);
            //d2dTarget.CopyFromMemory(bytes, dWidth * 4, rect);
            SharpDX.Rectangle rect = new SharpDX.Rectangle(0, 0, RenderWidth(), RenderHeight());
            d2dTarget.CopyFromMemory(bytes, RenderWidth() * 4, rect);

            //BitmapProperties bp = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));
            //int stride = bmp.Width * sizeof(int);
            //SharpDX.Direct2D1.Bitmap newBMP = Texture2D.FromStream<Texture2D>()
            /*using (SharpDX.DataStream tempStream = new SharpDX.DataStream(bmp.Height * stride, false, true))
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        int a = c.A;
                        int r = (c.R * a) / 255;
                        int g = (c.G * a) / 255;
                        int b = (c.B * a) / 255;
                        int bgra = b | (g << 8) | (r << 16) | (a << 24);
                        tempStream.Write(bgra);
                    }
                }

                //d2dRenderTarget.BeginDraw();
                //newBMP = new SharpDX.Direct2D1.Bitmap(d2dRenderTarget, new Size2(bmp.Width, bmp.Height), tempStream, stride, bp);
                //d2dRenderTarget.EndDraw();
            }*/


            //rect, 1.0f, SharpDX.Direct2D1.InterpolationMode.Linear
            //SharpDX.Mathematics.Interop.RawRectangleF rect = new SharpDX.Mathematics.Interop.RawRectangleF(50, 50, dWidth + 50, dHeight + 50);
            //d2dContext.DrawBitmap(bmp, rect, 1.0f, SharpDX.Direct2D1.InterpolationMode.Linear);
            //d2dContext.DrawBitmap(bmp, 1.0f, SharpDX.Direct2D1.InterpolationMode.Linear);
            d2dContext.EndDraw();

            //newBMP.Dispose();
        }

        private void Draw()
        {
            //d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));
            //renderTargetView.

            //byte[] bytes = CaptureFrame();

            if ( ! capturedFrame)
            {
                //capturedFrame = true;


                //GCHandle pinned = GCHandle.Alloc(outBytes, GCHandleType.Pinned);
                //IntPtr ptr = pinned.AddrOfPinnedObject();

                //pinned.Free();

                //int ProcessorCount = Environment.ProcessorCount;

                DrawByte(outBytes);
                //DrawByte(frameBytes);

                swapChain.Present(1, PresentFlags.None);
            }

            //DrawByte(frameBytes);
            //apChain.Present(1, PresentFlags.None);
        }

        public static void DoTheWork(object state)
        {
            object[] p = state as object[];
            //DoWork(dWidth, dHeight, 60, frameBytes, outBytesPtr);
            //DoWork((int)p[0], (int)p[1], (int)p[2], (byte[])p[3], (IntPtr)p[4]);
        }

        private int GetNumBytesDisplay()
        {
            return GetNumBytes(DisplayWidth(), DisplayHeight());
        }

        private int GetNumBytesRender()
        {
            return GetNumBytes(RenderWidth(), RenderHeight());
        }

        private int GetNumBytes(int width, int height)
        {
            int stride = 4 * width;
            return stride * height;
        }

        private void CaptureFrameAsync()
        {
            captureWorker.RunWorkerAsync();
            //ThreadPool.QueueUserWorkItem(this.CaptureFrame);
        }

        private void HandleCapturedFrame(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (conn.isServer) conn.BroadcastBytes(frameBytes, GetNumBytes());

            //byte[] s = System.Text.Encoding.UTF8.GetBytes("hello-from-desktop-dup");
            //conn.BroadcastBytes(s, s.Length);
            //if (!continueCapturing) return;
            CaptureFrameAsync();

            //BackgroundWorker compressionWorker = new BackgroundWorker();
            //byte[] duplicate = (byte[])frameBytes.Clone();

            //compressionWorker.DoWork += compress;
            //compressionWorker.RunWorkerAsync();
        }

        /*private void compress(object sender, DoWorkEventArgs ea) {
            int numBytes = GetNumBytes();
            byte[] duplicate = (byte[])frameBytes.Clone();
            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionMode.Compress), numBytes))
                {
                    gzs.Write(duplicate, 0, numBytes);
                }
                byte[] compressed = compressIntoMs.ToArray();
                Debug.WriteLine(frameBytes.Length);
                Debug.WriteLine(compressed.Length);
            }
        }*/

        //private void HandleCapturedFrame(object sender, RunWorkerCompletedEventArgs e)
        private void CaptureFrame(object sender, DoWorkEventArgs ea)
        //private void CaptureFrame(object state)
        {
            //CaptureFrameAsync();
            //Stopwatch timer = new Stopwatch();
            //timer.Start();

            //System.Drawing.Bitmap bitmap = null;

            SharpDX.DXGI.Resource screenResource = null;

            //bool captureDone = false;
            //for (int i = 0; !captureDone; i++)
            //{
                try
                {
                    OutputDuplicateFrameInformation duplicateFrameInformation;

                    // Try to get duplicated frame within given time
                    this.duplicatedOutput.AcquireNextFrame(1000, out duplicateFrameInformation, out screenResource);

                    //if (i > 0)
                    //{
                        // copy resource into memory that can be accessed by the CPU
                        using (Texture2D screenTexture2D = screenResource.QueryInterface<Texture2D>())
                            device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);


                        DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                /*int loopCap = dWidth * dHeight;
                unsafe
                {
                    uint* destPtr2 = (uint*)mapDest2.Scan0;
                    //uint* sourcePtr2 = (uint*)sourcePtr;
                    for (int x = 0; x < loopCap; ++x)
                    {
                        destPtr2[x] = (destPtr2[x] & 0x000000ff) << 16 | (destPtr2[x] & 0x0000FF00) | (destPtr2[x] & 0x00FF0000) >> 16 | (destPtr2[x] & 0xFF000000);
                        //sourcePtr2[x] = (sourcePtr2[x] & 0x000000ff) << 16 | (sourcePtr2[x] & 0x0000FF00) | (sourcePtr2[x] & 0x00FF0000) >> 16 | (sourcePtr2[x] & 0xFF000000);
                    }
                }*/

                byte[] byteData = new byte[GetNumBytesDisplay()];
                Marshal.Copy(mapSource.DataPointer, byteData, 0, GetNumBytesDisplay());
                frameBytes = byteData;
                        //captureDone = true;
                    //}

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
            //}

            //timer.Stop();
            //Debug.WriteLine(timer.ElapsedMilliseconds);
            //if (!continueCapturing) return;
            //CaptureFrameAsync();

            //frameBytes = byteData;

            //screenResource.Dispose();

            //return bitmap;
            //ea.Result = bytedata;
            //ThreadPool.QueueUserWorkItem(DoTheWork, new object[] { dWidth, dHeight, 60, frameBytes, outBytesPtr });

            //Stopwatch timer = new Stopwatch();
            //timer.Start();

            //DoWork(dWidth, dHeight, 60, frameBytes, outBytesPtr);

            //timer.Stop();
            //System.Console.WriteLine("Elapsed: " + timer.ElapsedMilliseconds);

            framesCapped++;
            //EncodeFrameAsync();
        }
    }
}
