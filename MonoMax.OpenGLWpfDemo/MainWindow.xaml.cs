using MonoMax.Core;
using System;
using System.Windows;

namespace MonoMax.WpfExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IRenderer _renderer = new ModernRenderer();


        public MainWindow()
        {
            InitializeComponent();
        }

        private void glControl_GLRender(object sender, EventArgs e)
        {
            var w = (int)glControl.ActualWidth;
            var h = (int)glControl.ActualHeight;
            _renderer.Draw(w, h);
        }

    }
}
