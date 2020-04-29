using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace MonoMax.WPFGLControl
{
    public interface IUpdateStrategy
    {
        bool IsCreated { get; }
        void Create();
        void Destroy();
        void Draw();
        void InvalidateImageSource();
        ImageSource CreateImageSource();
        void Resize(int width, int height);
    }
}
