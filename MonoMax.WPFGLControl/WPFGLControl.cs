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
    public class WPFGLControl : FrameworkElement
    {
        static WPFGLControl()
        {
            Toolkit.Init(new ToolkitOptions() { Backend = PlatformBackend.PreferNative });
        }
        private readonly Stopwatch mRenderwatch = new Stopwatch();
        private readonly CancellationTokenSource mCts = new CancellationTokenSource();

        private Rect mDirtyArea;
        private bool mWasResized;
        private HwndSource mHwnd;
        private IntPtr mWndHandle;
        private Thread mRenderTread;
        private IWindowInfo mWindowInfo;
        private ImageSource mRenderedImg;
        private TimeSpan mTargetFramerate;
        private GraphicsContext mGlContext;
        private IUpdateStrategy mUpdateStrategy;

        public UpdateStrategy UpdateStrategy { get; set; } = UpdateStrategy.D3DImage;
        public event EventHandler GLRender;
        public int MajorVersion { get; set; } = 3;
        public int MinorVersion { get; set; } = 3;
        public int FramerateLimit { get; set; }

        public WPFGLControl()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                Window.GetWindow(this).Closing += (ss, ee) => { mCts?.Cancel(); };

            var window = Window.GetWindow(this);
            mWndHandle = window is null ? IntPtr.Zero : new WindowInteropHelper(window).Handle;
            mHwnd = new HwndSource(0, 0, 0, 0, 0, "Offscreen Window", mWndHandle);
            mWindowInfo = Utilities.CreateWindowsWindowInfo(mHwnd.Handle);
            mTargetFramerate = FramerateLimit > 0 ? TimeSpan.FromMilliseconds(1000.0d / FramerateLimit) : TimeSpan.Zero;

            switch (UpdateStrategy)
            {
                case UpdateStrategy.WriteableBitmapImage:
                    mUpdateStrategy = new UpdateStrategyWriteableBitmap();
                    break;
                case UpdateStrategy.D3DImage:
                    mUpdateStrategy = new UpdateStrategyD3D();
                    break;
            }

            mRenderTread = new Thread((object boxedToken) =>
            {
                InitOpenGLContext();
                while (!mCts.IsCancellationRequested)
                {
                    if (mWasResized)
                    {
                        mWasResized = false;
                        mUpdateStrategy.Resize((int)Math.Round(ActualWidth), (int)Math.Round(ActualHeight));
                        Dispatcher.Invoke(() => mRenderedImg = mUpdateStrategy.CreateImageSource());
                    }

                    var sleep = mTargetFramerate - Render();
                    if (FramerateLimit > 0)
                        Thread.Sleep(sleep > TimeSpan.Zero ? sleep : TimeSpan.Zero); 
                    
                    Dispatcher.Invoke(() => InvalidateVisual());
                }

                mRenderTread.Join();
            })
            { IsBackground = true };
            mRenderTread.Start(mCts);
        }

        private TimeSpan Render()
        {
            mRenderwatch.Restart();
            mUpdateStrategy.PreRender();
            GLRender?.Invoke(this, EventArgs.Empty);
            mUpdateStrategy?.PostRender();
            Dispatcher.Invoke(() => mUpdateStrategy.Compute());
            return mRenderwatch.Elapsed;
        }

        private void InitOpenGLContext()
        {
            var mode = new GraphicsMode(ColorFormat.Empty, 0, 0, 0, 0, 0, false);
            mGlContext = new GraphicsContext(mode, mWindowInfo, MajorVersion, MinorVersion, GraphicsContextFlags.Default);
            mGlContext.MakeCurrent(mWindowInfo);
            mGlContext.LoadAll();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawImage(mRenderedImg, mDirtyArea);
            base.OnRender(drawingContext);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            mWasResized = true;
            mDirtyArea = new Rect(0, 0, sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
            base.OnRenderSizeChanged(sizeInfo);
        }

    }
}
