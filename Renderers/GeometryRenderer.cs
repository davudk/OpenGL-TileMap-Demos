using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace TileMapDemos.Renderers
{
    public class GeometryRenderer : IRenderer
    {
        public Vector2 Center { get; set; }
        public TileMap TileMap { get; private set; }
        private int shaderHandle, vboHandle, vaoHandle;
        private int backBufferWidth, backBufferHeight;

        public void Initialize(TileMap tileMap)
        {
            GL.ClearColor(Color.DimGray);
            GL.ClipControl(ClipOrigin.UpperLeft, ClipDepthMode.NegativeOneToOne);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            TileMap = tileMap;

            CreateShader();
            GenerateVertexBufferObject();
            GenerateVertexArrayObject();
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindTexture(TextureTarget.Texture2D, TileMap.TileSetHandle);
            GL.BindVertexArray(vaoHandle);

            Matrix4 projection = Matrix4.CreateTranslation(-Center.X, -Center.Y, 0) *
                                 Matrix4.CreateScale(IRenderer.TileSize, IRenderer.TileSize, 1) *
                                 Matrix4.CreateScale(2f / backBufferWidth, 2f / backBufferHeight, 1);

            GL.UniformMatrix4(GL.GetUniformLocation(shaderHandle, "projection"), false, ref projection);
            GL.Uniform2(GL.GetUniformLocation(shaderHandle, "mapSize"), TileMap.Width, TileMap.Height);

            GL.UseProgram(shaderHandle);
            GL.DrawArrays(PrimitiveType.Points, 0, TileMap.Tiles.Length);
        }

        private void CreateShader()
        {
            var assembly = Assembly.GetExecutingAssembly();

            const string vertFilePath = "OpenGLTileMapDemos.Resources.GeometryRenderer.vert";
            string vertSource = new StreamReader(assembly.GetManifestResourceStream(vertFilePath)).ReadToEnd();

            const string geomFilePath = "OpenGLTileMapDemos.Resources.GeometryRenderer.geom";
            string geomSource = new StreamReader(assembly.GetManifestResourceStream(geomFilePath)).ReadToEnd();

            const string fragFilePath = "OpenGLTileMapDemos.Resources.GeometryRenderer.frag";
            string fragSource = new StreamReader(assembly.GetManifestResourceStream(fragFilePath)).ReadToEnd();

            int vertHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertHandle, vertSource);
            GL.CompileShader(vertHandle);
            Console.WriteLine("Compiling GeometryRenderer.vert " + GL.GetShaderInfoLog(vertHandle));

            int geomHandle = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geomHandle, geomSource);
            GL.CompileShader(geomHandle);
            Console.WriteLine("Compiling GeometryRenderer.geom " + GL.GetShaderInfoLog(geomHandle));

            int fragHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragHandle, fragSource);
            GL.CompileShader(fragHandle);
            Console.WriteLine("Compiling GeometryRenderer.frag " + GL.GetShaderInfoLog(fragHandle));

            shaderHandle = GL.CreateProgram();
            GL.AttachShader(shaderHandle, vertHandle);
            GL.AttachShader(shaderHandle, geomHandle);
            GL.AttachShader(shaderHandle, fragHandle);
            GL.LinkProgram(shaderHandle);

            GL.DetachShader(shaderHandle, vertHandle);
            GL.DeleteShader(vertHandle);

            GL.DetachShader(shaderHandle, geomHandle);
            GL.DeleteShader(geomHandle);

            GL.DetachShader(shaderHandle, fragHandle);
            GL.DeleteShader(fragHandle);
        }

        private void GenerateVertexBufferObject()
        {
            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, TileMap.Tiles.Length * sizeof(byte), TileMap.Tiles,
                BufferUsageHint.StaticDraw);
        }

        private void GenerateVertexArrayObject()
        {
            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);

            GL.EnableVertexAttribArray(0); // tile id
            GL.VertexAttribIPointer(0, 1, VertexAttribIntegerType.UnsignedByte, sizeof(byte), IntPtr.Zero);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vaoHandle);
            vaoHandle = 0;

            GL.DeleteBuffer(vboHandle);
            vboHandle = 0;

            GL.DeleteProgram(shaderHandle);
        }

        public void OnBackBufferResized(int width, int height)
        {
            backBufferWidth = width;
            backBufferHeight = height;
            GL.Viewport(0, 0, width, height);
        }
    }
}