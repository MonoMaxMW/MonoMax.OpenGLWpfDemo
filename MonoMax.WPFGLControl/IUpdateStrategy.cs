using System.Windows.Media;

namespace MonoMax.WPFGLControl
{
    public enum UpdateStrategy
    {
        WriteableBitmapImage,
        D3DSurface
    }

    public interface IUpdateStrategy
    {
        bool IsCreated { get; }
        void Create();
        void Destroy();
        void Render();
        void PostRender();
        ImageSource CreateImageSource();
        void Resize(int width, int height);
        void PreRender();
    }
}
