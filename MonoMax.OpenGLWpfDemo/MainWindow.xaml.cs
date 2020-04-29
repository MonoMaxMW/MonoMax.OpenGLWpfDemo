using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonoMax.OpenGLWpfDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Renderer m_renderer = new Renderer();
        private DateTime lastMeasuredTime;
        private int frames;

        public MainWindow()
        {
            InitializeComponent();
            glControl.RenderCallback = () =>
            {
                if (DateTime.Now.Subtract(lastMeasuredTime) > TimeSpan.FromSeconds(1))
                {
                    Title = $"{frames}";
                    lastMeasuredTime = DateTime.Now;
                    frames = 0;
                }

                m_renderer.Render((int)Math.Round(glControl.ActualWidth), (int)Math.Round(glControl.ActualHeight));
                frames++;
            };


        }
    }
}
