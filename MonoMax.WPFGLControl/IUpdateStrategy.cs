using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MonoMax.WPFGLControl
{
    public interface IUpdateStrategy
    {
        void Create();
        void Destroy();
        ImageSource Draw(Action renderCallback);
        void Resize(int width, int height);
    }
}
