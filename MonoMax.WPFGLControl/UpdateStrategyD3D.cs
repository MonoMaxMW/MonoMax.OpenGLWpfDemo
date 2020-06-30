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
        private IntPtr[] mGlHandles;
        private IntPtr mGlHandle;
        private IntPtr mDxSharedHandle;
        private int mSharedTexture = -1;
        private int mFbo = -1;
        private int mRboDepth = -1;
        private Device mDevice;
        private Surface mSurface;
        private WGLInterop mWglInterop;
        private D3DImage mD3dImage;
        public bool IsCreated { get; private set; }

        public void Create()
        {

        }

        public void Destroy()
        {

        }

        public ImageSource CreateImageSource()
        {
            return mD3dImage = new D3DImage(96, 96);
        }

        public void Resize(int width, int height)
        {
            mGlHandle = IntPtr.Zero;
            mDxSharedHandle = IntPtr.Zero;

            if (mFbo > -1) gl.DeleteFramebuffer(mFbo); mFbo = -1;
            if (mRboDepth > -1) gl.DeleteRenderbuffer(mRboDepth); mRboDepth = -1;
            if (mSharedTexture > -1) gl.DeleteTexture(mSharedTexture); mSharedTexture = -1;
            if (mSurface != null) mSurface.Dispose(); mSurface = null;
            if (mDevice != null) mDevice.Dispose(); mDevice = null;

            mWglInterop = new WGLInterop();
            mDevice = new DeviceEx(
                new Direct3DEx(),
                0,
                DeviceType.Hardware,
                IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing |
                CreateFlags.Multithreaded |
                CreateFlags.PureDevice,
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

            mSurface = Surface.CreateRenderTarget(
                mDevice,
                width,
                height,
                Format.A8R8G8B8,
                MultisampleType.None,
                0,
                false,
                ref mDxSharedHandle);

            mFbo = gl.GenFramebuffer();
            mSharedTexture = gl.GenTexture();

            mGlHandle = mWglInterop.WglDXOpenDeviceNV(mDevice.NativePointer);
            mWglInterop.WglDXSetResourceShareHandleNV(mSurface.NativePointer, mDxSharedHandle);

            var genHandle = mWglInterop.WglDXRegisterObjectNV(
                mGlHandle,
                mSurface.NativePointer,
                (uint)mSharedTexture,
                (uint)TextureTarget.Texture2D,
                WGLInterop.WGL_ACCESS_READ_WRITE_NV);
            mGlHandles = new IntPtr[] { genHandle };

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, mFbo);
            gl.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                mSharedTexture, 0);

            mRboDepth = gl.GenRenderbuffer();
            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, mRboDepth);
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
            gl.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                 RenderbufferTarget.Renderbuffer,
                 mRboDepth);

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }


        public void PreRender()
        {
            mWglInterop.WglDXLockObjectsNV(mGlHandle, 1, mGlHandles);
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, mFbo);
        }

        public void Render()
        {
            gl.Finish();
            mWglInterop.WglDXUnlockObjectsNV(mGlHandle, 1, mGlHandles);
        }

        public void PostRender()
        {
            mD3dImage.Lock();
            mD3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, mSurface.NativePointer);
            mD3dImage.AddDirtyRect(new Int32Rect(0, 0, mD3dImage.PixelWidth, mD3dImage.PixelHeight));
            mD3dImage.Unlock();
        }
    }
}
