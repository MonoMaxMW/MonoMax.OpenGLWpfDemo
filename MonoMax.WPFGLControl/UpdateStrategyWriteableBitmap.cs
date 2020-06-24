using OpenTK.Graphics.OpenGL;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using gl = OpenTK.Graphics.OpenGL.GL;

namespace MonoMax.WPFGLControl
{
    internal sealed class UpdateStrategyWriteableBitmap : IUpdateStrategy
    {
        private int mFbo, mRboColor, mRboDepth;
        private int mWidth, mHeight;
        private IntPtr mBackbuffer;
        private WriteableBitmap mImgBmp;

        public bool IsCreated { get; private set; }

        public void Create()
        {
        }

        public void Destroy()
        {

        }

        public ImageSource CreateImageSource()
        {
            mImgBmp = new WriteableBitmap(mWidth, mHeight, 96, 96, PixelFormats.Pbgra32, null);
            mBackbuffer = mImgBmp.BackBuffer;
            return mImgBmp;
        }

        public void Resize(int width, int height)
        {
            mWidth = width;
            mHeight = height;

            if (mFbo > -1) gl.DeleteFramebuffer(mFbo); mFbo = -1;
            if (mRboColor > -1) gl.DeleteRenderbuffer(mRboColor); mRboColor = -1;
            if (mRboDepth > -1) gl.DeleteRenderbuffer(mRboDepth); mRboDepth = -1;

            mFbo = gl.GenFramebuffer();
            mRboColor = gl.GenRenderbuffer();
            mRboDepth = gl.GenRenderbuffer();
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, mFbo);

            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, mRboColor);
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
            gl.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                RenderbufferTarget.Renderbuffer,
                mRboColor);

            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, mRboDepth);
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
            gl.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer,
                mRboDepth);
        }

        public void PreRender()
        {

        }

        public void Render()
        {
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, mFbo);
            gl.ReadPixels(
                0, 0,
                mWidth, mHeight,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                mBackbuffer);
        }

        public void PostRender()
        {
            mImgBmp?.Lock();
            mImgBmp?.AddDirtyRect(new Int32Rect(0, 0, mImgBmp.PixelWidth, mImgBmp.PixelHeight));
            mImgBmp?.Unlock();
        }

    }
}
