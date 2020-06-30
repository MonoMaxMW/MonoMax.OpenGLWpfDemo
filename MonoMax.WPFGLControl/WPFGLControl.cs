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
    public class WPFGLControl : UserControl
    {
        static WPFGLControl()
        {
            Toolkit.Init(new ToolkitOptions() { Backend = PlatformBackend.PreferNative });
        }

        private CancellationTokenSource mCts;
        private bool mWasResized;
        private int mFrames;
        private TextBlock mTextblockFrames;
        private Image mWpfImage;
        private IntPtr mWndHandle;
        private HwndSource mHwnd;
        private IWindowInfo mWindowInfo;
        private IUpdateStrategy mUpdateStrategy;
        private GraphicsContext mGlContext;
        private Thread mRenderTread;
        private DispatcherTimer mDt;
        private Stopwatch mStopwatch = new Stopwatch();


        public bool ShowFramerate { get; set; }
        public bool UseSeperateRenderThread { get; set; }
        public UpdateStrategy UpdateStrategy { get; set; } = UpdateStrategy.D3DSurface;
        public event EventHandler GLRender;
        public int MajorVersion { get; set; } = 3;
        public int MinorVersion { get; set; } = 3;

        private void InitOpenGLContext()
        {
            var mode = new GraphicsMode(ColorFormat.Empty, 0, 0, 0, 0, 0, false);
            mGlContext = new GraphicsContext(mode, mWindowInfo, MajorVersion, MinorVersion, GraphicsContextFlags.Default);
            mGlContext.MakeCurrent(mWindowInfo);
            mGlContext.LoadAll();
        }

        public override void OnApplyTemplate()
        {
            if(!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                Window.GetWindow(this).Closing += (ss, ee) => { mCts?.Cancel(); };

            var window = Window.GetWindow(this);
            mWndHandle = window is null ? IntPtr.Zero : new WindowInteropHelper(window).Handle;
            mHwnd = new HwndSource(0, 0, 0, 0, 0, "Offscreen Window", mWndHandle);
            mWindowInfo = Utilities.CreateWindowsWindowInfo(mHwnd.Handle);

            mWpfImage = new Image()
            {
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, -1)
            };

            var grid = new Grid();
            mTextblockFrames = new TextBlock()
            {
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5),
                Foreground = new SolidColorBrush(Colors.Blue),
                FontWeight = FontWeights.Bold
            };
            Panel.SetZIndex(mTextblockFrames, 1);

            grid.Children.Add(mTextblockFrames);
            grid.Children.Add(mWpfImage);
            AddChild(grid);

            switch (UpdateStrategy)
            {
                case UpdateStrategy.WriteableBitmapImage:
                    mUpdateStrategy = new UpdateStrategyWriteableBitmap();
                    break;
                case UpdateStrategy.D3DSurface:
                    mUpdateStrategy = new UpdateStrategyD3D();
                    break;
            }

            if (UseSeperateRenderThread)
            {
                mCts = new CancellationTokenSource();
                mRenderTread = new Thread((object boxedToken) =>
                {
                    InitOpenGLContext();
                    while (!mCts.IsCancellationRequested)
                    {
                        UpdateFramerate();
                        if (mWasResized)
                        {
                            mWasResized = false;
                            mUpdateStrategy.Resize((int)Math.Round(ActualWidth), (int)Math.Round(ActualHeight));
                            Dispatcher.Invoke(() => mWpfImage.Source = mUpdateStrategy.CreateImageSource());
                        }

                        mUpdateStrategy.PreRender();
                        GLRender?.Invoke(this, EventArgs.Empty);
                        mUpdateStrategy?.Render();
                        Dispatcher.Invoke(() => mUpdateStrategy.PostRender());
                    }

                    mRenderTread.Join();
                })
                { IsBackground = true, Priority = ThreadPriority.Highest };
                mRenderTread.Start(mCts);
                mStopwatch.Start();
            }
            else
            {
                InitOpenGLContext();

                CompositionTarget.Rendering += (o, e) =>
                {
                    UpdateFramerate();
                    if (mWasResized)
                    {
                        mWasResized = false;
                        mUpdateStrategy.Resize((int)Math.Round(ActualWidth), (int)Math.Round(ActualHeight));
                        mWpfImage.Source = mUpdateStrategy.CreateImageSource();
                    }

                    mUpdateStrategy.PreRender();
                    GLRender?.Invoke(this, EventArgs.Empty);
                    mUpdateStrategy.Render();
                    mUpdateStrategy.PostRender();
                };


                //_dt = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(1) };
                //_dt.Tick += (o, e) =>
                //{

                //};
                //_dt.Start();
                //_stopwatch.Start();
            }

            base.OnApplyTemplate();
        }


        private void UpdateFramerate()
        {
            if (!ShowFramerate)
                return;

            ++mFrames;
            if (mStopwatch.ElapsedMilliseconds > 1000)
            {
                Dispatcher.Invoke(() => mTextblockFrames.Text = $"fps {mFrames}");
                mStopwatch.Restart();
                mFrames = 0;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            mWasResized = true;
            base.OnRenderSizeChanged(sizeInfo);
        }

    }
}
