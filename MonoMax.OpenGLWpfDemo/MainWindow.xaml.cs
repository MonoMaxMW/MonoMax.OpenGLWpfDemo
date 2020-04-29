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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void glControl_GLRender(object sender, EventArgs e)
        {
            m_renderer.Render((int)Math.Round(glControl.ActualWidth), (int)Math.Round(glControl.ActualHeight));

        }
    }
}
