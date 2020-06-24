using MonoMax.Core;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Diagnostics;

namespace MonoMax.GameWindowExample
{
    public class Game : GameWindow
    {
        private readonly IRenderer _renderer = new ModernRenderer();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private int _frames;

        public Game()
            :base(
                 640, 480, 
                 GraphicsMode.Default,
                 "Console", 
                 GameWindowFlags.Default, 
                 DisplayDevice.Default, 3, 0, 
                 GraphicsContextFlags.Default)
        {

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard.GetState()[Key.Escape])
                Exit();

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            ++_frames;

            if(_stopwatch.ElapsedMilliseconds > 1000)
            {
                Title = "fps " + _frames;
                _stopwatch.Restart();
                _frames = 0;
            }


            _renderer.Draw(this.Width, this.Height);
            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }
    }
}
