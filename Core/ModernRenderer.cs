using MonoMax.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using gl = OpenTK.Graphics.OpenGL.GL;
using mat4 = OpenTK.Matrix4;

namespace MonoMax.OpenGLWpfDemo.Renderer
{
    public sealed class ModernRenderer : IRenderer
    {
        private int _vao = -1;
        private int _vbo = -1;
        private int _prg = -1;
        private mat4 _projMat;
        private mat4 _viewMat;
        private float[] _vertices;
        private float angle = 0.0f;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private static string GetShaderCode(string file)
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(fi.DirectoryName, "Assets", file);

            if (!File.Exists(path))
                throw new Exception();

            return File.ReadAllText(path);
        }

        public void Draw(int width, int height)
        {
            if (width != Width || height != Height)
                Resize(width, height);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gl.ClearColor(Color.LightBlue);
            gl.Enable(EnableCap.DepthTest);


            if (_vao == -1)
            {
                _vertices = FillData();

                _vao = gl.GenVertexArray();
                _vbo = gl.GenBuffer();

                _prg = gl.CreateProgram();
                var vsId = gl.CreateShader(ShaderType.VertexShader);
                gl.ShaderSource(vsId, GetShaderCode("example_shader_vs.glsl")); LogError();
                gl.CompileShader(vsId); LogError();

                var fsId = gl.CreateShader(ShaderType.FragmentShader);
                gl.ShaderSource(fsId, GetShaderCode("example_shader_fs.glsl")); LogError();
                gl.CompileShader(fsId); LogError();

                gl.AttachShader(_prg, vsId); LogError();
                gl.AttachShader(_prg, fsId); LogError();
                gl.LinkProgram(_prg); LogError();
                gl.DeleteShader(vsId); LogError();
                gl.DeleteShader(fsId); LogError();

                gl.UseProgram(_prg);
                gl.BindVertexArray(_vao);
                gl.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                gl.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertices.Length, _vertices, BufferUsageHint.DynamicDraw);
                gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                gl.EnableVertexAttribArray(0);
            }

            gl.PointSize(1.5f);
            gl.BindVertexArray(_vao);

            _viewMat =
                mat4.CreateRotationY(angle) *
                mat4.CreateTranslation(0.0f, 0.0f, -1000.0f);


            gl.UniformMatrix4(0, false, ref _projMat);
            gl.UniformMatrix4(1, false, ref _viewMat);
            gl.DrawArrays(PrimitiveType.Points, 0, _drawCount);



            angle += 0.01f;
        }

        private int _drawCount;

        private float[] FillData()
        {
            var count = 1_000_000;
            var factor = 0.2f;
            var rnd = new Random();
            var array = new float[count * 3];

            //return new[]
            //{
            //    0.0f, 1.0f, 0.0f,
            //    -1.0f, -1.0f, 0.0f,
            //    1.0f, -1.0f, 0.0f
            //};



            for (int i = 0; i < array.Length / 3; i += 3)
            {
                array[i + 0] = rnd.Next(-1000, 1000) * factor;
                array[i + 1] = rnd.Next(-1000, 1000) * factor;
                array[i + 2] = rnd.Next(-1000, 1000) * factor;
            }
            _drawCount = array.Length / 3;
            return array;
        }

        private void LogError() => Console.WriteLine($"Error:{gl.GetError()}");

        private void Resize(int width, int height)
        {
            Height = height;
            Width = width;

            float halfWidth = Width * 0.00125f;
            float halfHeight = Height * 0.00125f;

            var aspect = (float)width / (float)height;

            _projMat = mat4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60), aspect,
                0.01f,
                2000.0f);

            gl.Viewport(0, 0, width, height);
        }

    }
}
