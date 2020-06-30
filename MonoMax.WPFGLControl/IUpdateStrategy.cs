using System.Windows.Media;

namespace MonoMax.WPFGLControl
{
    public enum UpdateStrategy
    {
        WriteableBitmapImage,
        D3DImage
    }

    public interface IUpdateStrategy
    {
        bool IsCreated { get; }
        void Create();
        void Destroy();
        void PostRender();
        void Compute();
        ImageSource CreateImageSource();
        void Resize(int width, int height);
        void PreRender();
    }
}
