using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;

namespace MonoMax.WPFGLControl
{
    internal sealed class UpdateStrategyWriteableBitmap : IUpdateStrategy
    {
        private int _fbo;
        private IntPtr _backbuffer;
        private WriteableBitmap _imgBmp;

        public void Create()
        {
            _fbo = gl.GenFramebuffer();
        }

        public void Destroy()
        {

        }

        public ImageSource Draw(Action renderCallback)
        {
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            renderCallback?.Invoke();
            gl.ReadPixels(
                0, 0,
                _imgBmp.PixelWidth, _imgBmp.PixelHeight,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                _backbuffer);

            _imgBmp.Lock();
            _imgBmp.AddDirtyRect(new Int32Rect(0, 0, _imgBmp.PixelWidth, _imgBmp.PixelHeight));
            _imgBmp.Unlock();
            return _imgBmp;
        }

        public void Resize(int width, int height)
        {
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            var rboColor = gl.GenRenderbuffer();

            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboColor);
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
            gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, rboColor);
            //gl.DeleteRenderbuffer(rboColor);

            _imgBmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            _backbuffer = _imgBmp.BackBuffer;
        }
    }
}
