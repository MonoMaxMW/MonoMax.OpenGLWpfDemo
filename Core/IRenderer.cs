using System.Drawing;
using System.Windows;

namespace MonoMax.Core
{
    public interface IRenderer
    {
        int Width { get; }
        int Height { get; }
        void Draw(int width, int height);
    }
}
