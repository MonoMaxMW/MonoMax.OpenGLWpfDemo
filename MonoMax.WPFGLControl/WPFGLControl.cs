using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace MonoMax.WPFGLControl
{
    public enum UpdateStrategy
    {
        WriteableBitmapImage,
        D3DSurface
    }

    public class WPFGLControl : UserControl
    {
        static WPFGLControl()
        {
            Toolkit.Init(new ToolkitOptions() { Backend = PlatformBackend.PreferNative });
        }

        private Image _wpfImage;
        private IntPtr _windowHandle;
        private HwndSource _hwnd;
        private IWindowInfo _windowInfo;
        private IUpdateStrategy _updateStrategy;
        private GraphicsContext _glContext;
        private DispatcherTimer _dt = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(1) };

        public UpdateStrategy UpdateStrategy { get; set; } = UpdateStrategy.D3DSurface;
        public Action RenderCallback { get; set; }


        public WPFGLControl()
        {
            _wpfImage = new Image()
            {
                //RenderTransformOrigin = new Point(0.5, 0.5),
                //RenderTransform = new ScaleTransform(1, -1)
            };
            AddChild(_wpfImage);
            _dt.Tick += (o, e) => Draw();
            _dt.Start();

            //CompositionTarget.Rendering += (o, args) =>
            //{
            //    //Draw();
            //};
        }

        public override void OnApplyTemplate()
        {
            switch (UpdateStrategy)
            {
                case UpdateStrategy.WriteableBitmapImage:
                    _updateStrategy = new UpdateStrategyWriteableBitmap();
                    break;
                case UpdateStrategy.D3DSurface:
                    _updateStrategy = new UpdateStrategyD3D();
                    break;
            }


            var window = Window.GetWindow(this);
            _windowHandle = window is null ? IntPtr.Zero : new WindowInteropHelper(window).Handle;
            _hwnd = new HwndSource(0, 0, 0, 0, 0, "Offscreen Window", _windowHandle);
            _windowInfo = Utilities.CreateWindowsWindowInfo(_hwnd.Handle);

            var mode = new GraphicsMode(ColorFormat.Empty, 0, 0, 0, 0, 0, false);
            _glContext = new GraphicsContext(mode, _windowInfo, 3, 0, GraphicsContextFlags.Default);
            _glContext.MakeCurrent(_windowInfo);
            _glContext.LoadAll();
            _updateStrategy.Create();
            
            base.OnApplyTemplate();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _updateStrategy.Resize((int)Math.Round(ActualWidth),(int)Math.Round(ActualHeight));
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void Draw()
        {
            _wpfImage.Source = _updateStrategy?.Draw(RenderCallback);
        }
    }
}
