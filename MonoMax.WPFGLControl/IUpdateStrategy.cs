using System.Windows.Media;

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
