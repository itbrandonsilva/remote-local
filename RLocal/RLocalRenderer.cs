﻿using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace DesktopDup
{
    public partial class RLocalRenderer : IDisposable
    {
        public RenderForm renderForm;
        int RenderWidth;
        int RenderHeight;
        public byte[] RenderBuffer;

        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.DeviceContext d3dDeviceContext;
        private SharpDX.DXGI.SwapChain swapChain;
        private SharpDX.Direct3D11.RenderTargetView renderTargetView;

        protected SharpDX.Direct2D1.Device d2dDevice;
        protected SharpDX.Direct2D1.DeviceContext d2dContext;
        private SharpDX.Direct2D1.Bitmap1 d2dTarget;
        SharpDX.Direct2D1.RenderTarget d2dRenderTarget;

        Func<byte[]> RequestFrame;

        public RLocalRenderer(int width, int height)
        {
            /*RenderWidth = width;
            RenderHeight = height;

            renderForm = new RenderForm("RLocal Renderer");
            renderForm.ClientSize = new Size(RenderWidth, RenderHeight);
            renderForm.AllowUserResizing = false;

            ModeDescription backBufferDesc = new ModeDescription(RenderWidth, RenderHeight, new Rational(60, 1), Format.B8G8R8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
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

            System.Drawing.Graphics g = renderForm.CreateGraphics();

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

            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);*/

            /*
            this.Size = new System.Drawing.Size(1920, 1080);
            Format current = Manager.Adapters[0].CurrentDisplayMode.F­ormat;
            presentParams = new PresentParameters();
            presentParams.SwapEffect = SwapEffect.Discard;
            presentParams.Windowed = false;
            presentParams.BackBufferFormat = current;
            presentParams.BackBufferCount = 1;
            presentParams.BackBufferWidth = 1920;
            presentParams.BackBufferHeight = 1080;
            device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            */

            RenderWidth = width;
            RenderHeight = height;

            renderForm = new RenderForm("RLocal Renderer");
            renderForm.ClientSize = new Size(RenderWidth, RenderHeight);
            renderForm.AllowUserResizing = false;
            //renderForm.IsFullscreen = true;

            ModeDescription backBufferDesc = new ModeDescription(RenderWidth, RenderHeight, new Rational(60, 1), Format.B8G8R8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };

            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.VideoSupport | SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport; // | DeviceCreationFlags.Debug;
            //SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, creationFlags, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>();

            //swapChain.SetFullscreenState(true, null);

            using (SharpDX.Direct3D11.Texture2D backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            {
                renderTargetView = new SharpDX.Direct3D11.RenderTargetView(d3dDevice, backBuffer);
            }

            System.Drawing.Graphics g = renderForm.CreateGraphics();

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

            RenderBuffer = new byte[RLocalUtils.GetSizeBGRA(RenderWidth, RenderHeight)];
        }

        public int GetBufferSize()
        {
            return RLocalUtils.GetSizeBGRA(RenderWidth, RenderHeight);
        }

        public void Start()
        {
            //renderForm.Show();
            //renderLoop = new RenderLoop(renderForm);
            renderForm.DoubleClick += new EventHandler((o, ea) =>
            {
                renderForm.IsFullscreen = !renderForm.IsFullscreen;
            });
            
            RenderLoop.Run(renderForm, RenderCallback);
        }

        public void RenderCallback()
        {
            //RenderBuffer = RequestFrame();
            DrawBytes(RenderBuffer);
            swapChain.Present(1, PresentFlags.None);
        }

        public void DrawBytes(byte[] bytes)
        {
            d2dContext.Target = d2dTarget;
            d2dContext.BeginDraw();

            SharpDX.Rectangle rect = new SharpDX.Rectangle(0, 0, RenderWidth, RenderHeight);
            d2dTarget.CopyFromMemory(bytes, RenderWidth * 4, rect);

            d2dContext.EndDraw();
        }

        public void Dispose()
        {
            renderForm.Dispose();
        }
    }
}
