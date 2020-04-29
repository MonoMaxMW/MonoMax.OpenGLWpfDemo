namespace MonoMax.Core
{
    using System;
    using System.Drawing;
    using System.Windows;
    using OpenTK;
    using OpenTK.Graphics.OpenGL;
    using gl = OpenTK.Graphics.OpenGL.GL;

    /// <summary>
    /// The renderer.
    /// </summary>
    public sealed class LegacyRenderer : IRenderer
    {

        private float angle;
        private int displayList;
        public int Width { get; private set; }
        public int Height { get; private set; }


        private void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float halfWidth = (float)(Width / 2);
            float halfHeight = (float)(height / 2);
            GL.Ortho(-halfWidth, halfWidth, halfHeight, -halfHeight, 1000.0f, -1000.0f);
            GL.Viewport(0, 0, (int)width, (int)height);
        }

        public void Draw(int width, int height)
        {
            if (width != Width || height != Height)
                Resize(width, height);

            gl.ClearColor(Color.LightBlue);
            gl.Clear(ClearBufferMask.ColorBufferBit);
            //gl.Enable(EnableCap.DepthTest);


            //DrawSimpleTriangle();
            DrawDisplayList();
        }

        private void DrawSimpleTriangle()
        {
            gl.Begin(PrimitiveType.Triangles);
                gl.Vertex3(0.0f, 1.0f, 0.0f);
                gl.Vertex3(-1.0f, -1.0f, 0.0f);
                gl.Vertex3(1.0f, -1.0f, 0.0f);
            gl.End();
        }

        private void DrawDisplayList()
        {
            if (this.displayList <= 0)
            {
                this.displayList = GL.GenLists(1);
                gl.NewList(this.displayList, ListMode.Compile);
                gl.PointSize(1.5f);
                gl.Begin(PrimitiveType.Points);
                Random rnd = new Random();
                for (int i = 0; i < 1_000_000; i++)
                {
                    float factor = 0.2f;
                    Vector3 position = new Vector3(
                        rnd.Next(-1000, 1000) * factor,
                        rnd.Next(-1000, 1000) * factor,
                        rnd.Next(-1000, 1000) * factor);

                    var r = rnd.Next(0, 255);
                    var g = rnd.Next(0, 255);
                    var b = rnd.Next(0, 255);
                    gl.Color3(0.2f, 0.3f, 0.4f);
                    gl.Vertex3(position);
                }

                gl.End();
                gl.EndList();
            }



            gl.MatrixMode(MatrixMode.Modelview);
            gl.LoadIdentity();

            this.angle += 1f;
            gl.Rotate(this.angle, Vector3.UnitZ);
            gl.Rotate(this.angle, Vector3.UnitY);
            gl.Rotate(this.angle, Vector3.UnitX);
            gl.Translate(0.5f, 0, 0);

            gl.CallList(this.displayList);
        }
    }
}
