using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;

namespace MonoMax.WPFGLControl
{
    internal sealed class UpdateStrategyWriteableBitmap : IUpdateStrategy
    {
        private int _fbo;
        private int _width, _height;
        private IntPtr _backbuffer;
        private WriteableBitmap _imgBmp;

        public bool IsCreated { get; private set; }

        public void Create()
        {
        }

        public void Destroy()
        {

        }


        public ImageSource CreateImageSource()
        {
            _imgBmp = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Pbgra32, null);
            _backbuffer = _imgBmp.BackBuffer;
            return _imgBmp;
        }

        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;

            _fbo = gl.GenFramebuffer();
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            var rboColor = gl.GenRenderbuffer();

            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboColor);
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
            gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, rboColor);
        }

        public void Draw()
        {
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            gl.ReadPixels(
                0, 0,
                _width, _height,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                _backbuffer);
        }

        public void InvalidateImageSource()
        {
            _imgBmp?.Lock();
            _imgBmp?.AddDirtyRect(new Int32Rect(0, 0, _imgBmp.PixelWidth, _imgBmp.PixelHeight));
            _imgBmp?.Unlock();
        }

    }
}
