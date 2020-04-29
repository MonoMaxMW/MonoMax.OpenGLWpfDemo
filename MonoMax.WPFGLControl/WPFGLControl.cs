using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
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

        public WPFGLControl()
        {
            Loaded += (s, e) =>
            {
                Window.GetWindow(this)
                .Closing += (ss, ee) => { _cts?.Cancel(); };
            };
        }

        private CancellationTokenSource _cts;
        private bool _wasResized;
        private int _frames;
        private TextBlock _framesTextBlock;
        private DateTime _lastMeasured;
        private Image _wpfImage;
        private IntPtr _windowHandle;
        private HwndSource _hwnd;
        private IWindowInfo _windowInfo;
        private IUpdateStrategy _updateStrategy;
        private GraphicsContext _glContext;
        private Thread _renderThread;
        private DispatcherTimer _dt;

        public bool UseSeperateRenderThread { get; set; }
        public UpdateStrategy UpdateStrategy { get; set; } = UpdateStrategy.D3DSurface;
        public event EventHandler GLRender;

        public override void OnApplyTemplate()
        {
            var window = Window.GetWindow(this);
            _windowHandle = window is null ? IntPtr.Zero : new WindowInteropHelper(window).Handle;
            _hwnd = new HwndSource(0, 0, 0, 0, 0, "Offscreen Window", _windowHandle);
            _windowInfo = Utilities.CreateWindowsWindowInfo(_hwnd.Handle);

            _wpfImage = new Image()
            {
                //RenderTransformOrigin = new Point(0.5, 0.5),
                //RenderTransform = new ScaleTransform(1, -1)
            };

            var grid = new Grid();
            _framesTextBlock = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(Colors.Blue),
                FontWeight = FontWeights.Bold
            };
            grid.Children.Add(_framesTextBlock);
            grid.Children.Add(_wpfImage);
            AddChild(grid);

            switch (UpdateStrategy)
            {
                case UpdateStrategy.WriteableBitmapImage:
                    _updateStrategy = new UpdateStrategyWriteableBitmap();
                    break;
                case UpdateStrategy.D3DSurface:
                    _updateStrategy = new UpdateStrategyD3D();
                    break;
            }

            if (UseSeperateRenderThread)
            {
                _cts = new CancellationTokenSource();
                _renderThread = new Thread((object boxedToken) =>
                {
                    InitOpenGLContext();
                    while (!_cts.IsCancellationRequested)
                    {
                        UpdateFramerate();
                        if (_wasResized)
                        {
                            _wasResized = false;
                            _updateStrategy.Resize((int)Math.Round(ActualWidth), (int)Math.Round(ActualHeight));
                            Dispatcher.Invoke(() => _wpfImage.Source = _updateStrategy.CreateImageSource());
                        }

                        GLRender?.Invoke(this, EventArgs.Empty);
                        _updateStrategy?.Draw();
                        Dispatcher.Invoke(() => _updateStrategy.InvalidateImageSource());
                    }

                    _renderThread.Join();
                })
                { IsBackground = true, Priority = ThreadPriority.Highest };
                _renderThread.Start(_cts);
                _stopwatch.Start();
            }
            else
            {
                _dt = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(1) };
                InitOpenGLContext();
                _dt.Tick += (o, e) =>
                {
                    UpdateFramerate();
                    if (_wasResized)
                    {
                        _wasResized = false;
                        _updateStrategy.Resize((int)Math.Round(ActualWidth), (int)Math.Round(ActualHeight));
                        _wpfImage.Source = _updateStrategy.CreateImageSource();
                    }

                    GLRender?.Invoke(this, EventArgs.Empty);
                    _updateStrategy.Draw();
                    _updateStrategy.InvalidateImageSource();
                };
                _dt.Start();
                _stopwatch.Start();
            }

            base.OnApplyTemplate();
        }

        private Stopwatch _stopwatch = new Stopwatch();

        private void UpdateFramerate()
        {
            ++_frames;
            if (_stopwatch.ElapsedMilliseconds > 1000)
            {
                Dispatcher.Invoke(() => _framesTextBlock.Text = $"fps {_frames}");
                _stopwatch.Restart();
                _frames = 0;
            }
        }

        private void InitOpenGLContext()
        {
            var mode = new GraphicsMode(ColorFormat.Empty, 0, 0, 0, 0, 0, false);
            _glContext = new GraphicsContext(mode, _windowInfo, 3, 0, GraphicsContextFlags.Default);
            _glContext.LoadAll();
            _glContext.MakeCurrent(_windowInfo);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _wasResized = true;
            base.OnRenderSizeChanged(sizeInfo);
        }

    }
}
