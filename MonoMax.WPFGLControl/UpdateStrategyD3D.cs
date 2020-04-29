using OpenTK.Graphics.OpenGL;
using SharpDX.Direct3D9;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using gl = OpenTK.Graphics.OpenGL.GL;

namespace MonoMax.WPFGLControl
{
    internal sealed class UpdateStrategyD3D : IUpdateStrategy
    {
        private IntPtr[] _glHandles;
        private IntPtr _glHandle;
        private IntPtr _dxSharedHandle;
        private int _glTexture = -1;
        private int _fbo = -1;
        private Device _device;
        private Surface _surface;
        private WGLInterop _wglInterop;
        private D3DImage _d3dImage;
        public bool IsCreated { get; private set; }

        public void Create()
        {

        }

        public void Destroy()
        {

        }

        private void ReleaseResources()
        {
            _glHandle = IntPtr.Zero;
            _dxSharedHandle = IntPtr.Zero;

            if (_fbo > -1) gl.DeleteFramebuffer(_fbo); _fbo = -1;
            if (_glTexture > -1) gl.DeleteTexture(_glTexture); _glTexture = -1;
            if (_surface != null) _surface = null;
            if (_device != null) _device = null;
        }

        public ImageSource CreateImageSource()
        {
            return _d3dImage = new D3DImage(96, 96);
        }

        public void Resize(int width, int height)
        {
            _wglInterop = new WGLInterop();
            ReleaseResources();

            _device = new DeviceEx(
                new Direct3DEx(),
                0,
                DeviceType.Hardware,
                IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing |
                CreateFlags.Multithreaded |
                CreateFlags.FpuPreserve,
                new PresentParameters()
                {
                    Windowed = true,
                    SwapEffect = SwapEffect.Discard,
                    DeviceWindowHandle = IntPtr.Zero,
                    PresentationInterval = PresentInterval.Default,
                    BackBufferFormat = Format.Unknown,
                    BackBufferWidth = width,
                    BackBufferHeight = height
                });

            _surface = Surface.CreateRenderTarget(
                _device,
                width,
                height,
                Format.A8R8G8B8,
                MultisampleType.None,
                0,
                false,
                ref _dxSharedHandle);

            _fbo = gl.GenFramebuffer();
            _glTexture = gl.GenTexture();

            _glHandle = _wglInterop.WglDXOpenDeviceNV(_device.NativePointer);
            _wglInterop.WglDXSetResourceShareHandleNV(_surface.NativePointer, _dxSharedHandle);

            var genHandle = _wglInterop.WglDXRegisterObjectNV(
                _glHandle,
                _surface.NativePointer,
                (uint)_glTexture,
                (uint)TextureTarget.Texture2D,
                WGLInterop.WGL_ACCESS_READ_WRITE_NV);
            _glHandles = new IntPtr[] { genHandle };

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            gl.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                _glTexture,
                0);



            gl.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0);
        }

        public void Draw()
        {
            _wglInterop.WglDXLockObjectsNV(_glHandle, 1, _glHandles);
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            gl.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0);
            _wglInterop.WglDXUnlockObjectsNV(_glHandle, 1, _glHandles);

        }

        public void InvalidateImageSource()
        {
            _d3dImage.Lock();
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _surface.NativePointer);
            _d3dImage.AddDirtyRect(new Int32Rect(0, 0, _d3dImage.PixelWidth, _d3dImage.PixelHeight));
            _d3dImage.Unlock();
        }
    }
}
