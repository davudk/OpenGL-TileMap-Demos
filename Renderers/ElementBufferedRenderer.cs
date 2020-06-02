using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace TileMapDemos.Renderers
{
    public class ElementBufferedRenderer : IRenderer
    {
        public Vector2 Center { get; set; }
        public TileMap TileMap { get; private set; }
        private int shaderHandle, vboHandle, eboHandle, vaoHandle;
        private int backBufferWidth, backBufferHeight;

        public void Initialize(TileMap tileMap)
        {
            GL.ClearColor(Color.DimGray);
            GL.ClipControl(ClipOrigin.UpperLeft, ClipDepthMode.NegativeOneToOne);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            TileMap = tileMap;

            CreateShader();
            GenerateVertexBufferObject();
            GenerateElementBufferObject();
            GenerateVertexArrayObject();
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindTexture(TextureTarget.Texture2D, TileMap.TileSetHandle);
            GL.BindVertexArray(vaoHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);

            Matrix4 projection = Matrix4.CreateTranslation(-Center.X, -Center.Y, 0) *
                                 Matrix4.CreateScale(IRenderer.TileSize, IRenderer.TileSize, 1) *
                                 Matrix4.CreateScale(2f / backBufferWidth, 2f / backBufferHeight, 1);

            GL.UniformMatrix4(GL.GetUniformLocation(shaderHandle, "projection"), false, ref projection);

            GL.UseProgram(shaderHandle);
            // GL.DrawArrays(PrimitiveType.Triangles, 0, TileMap.Tiles.Length * 6);
            GL.DrawElements(PrimitiveType.Triangles, TileMap.Tiles.Length * 6, DrawElementsType.UnsignedInt, 0);
        }

        private void CreateShader()
        {
            var assembly = Assembly.GetExecutingAssembly();

            const string vertFilePath = "OpenGLTileMapDemos.Resources.BufferedRenderer.vert";
            string vertSource = new StreamReader(assembly.GetManifestResourceStream(vertFilePath)).ReadToEnd();

            const string fragFilePath = "OpenGLTileMapDemos.Resources.BufferedRenderer.frag";
            string fragSource = new StreamReader(assembly.GetManifestResourceStream(fragFilePath)).ReadToEnd();

            int vertHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertHandle, vertSource);
            GL.CompileShader(vertHandle);
            Console.WriteLine("Compiling BufferedRenderer.vert " + GL.GetShaderInfoLog(vertHandle));

            int fragHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragHandle, fragSource);
            GL.CompileShader(fragHandle);
            Console.WriteLine("Compiling BufferedRenderer.frag " + GL.GetShaderInfoLog(fragHandle));

            shaderHandle = GL.CreateProgram();
            GL.AttachShader(shaderHandle, vertHandle);
            GL.AttachShader(shaderHandle, fragHandle);
            GL.LinkProgram(shaderHandle);

            GL.DetachShader(shaderHandle, vertHandle);
            GL.DeleteShader(vertHandle);

            GL.DetachShader(shaderHandle, fragHandle);
            GL.DeleteShader(fragHandle);
        }

        private void GenerateVertexBufferObject()
        {
            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);

            int floatCount = TileMap.Tiles.Length // for each tile
                             * 4 // there are 4 vertices in each tile (since it's a square)
                             * 2 // each vertex has two components: Position and Texcoord
                             * 2; // each component has two fields: x and y
            float[] vertexData = new float[floatCount];
            int i = 0;
            for (int x = 0; x < TileMap.Width; x++)
            {
                for (int y = 0; y < TileMap.Height; y++)
                {
                    byte tile = TileMap[x, y];
                    float tx0 = (tile & 15) * IRenderer.TileTexSize + IRenderer.TileTexPadding;
                    float ty0 = (tile >> 4) * IRenderer.TileTexSize + IRenderer.TileTexPadding;
                    float tySize = IRenderer.TileTexSize - IRenderer.TileTexPadding * 2;

                    // vertex 0 (top left)
                    vertexData[i + 0] = x; // position x
                    vertexData[i + 1] = y; // position y
                    vertexData[i + 2] = tx0; // texcoord x
                    vertexData[i + 3] = ty0; // texcoord y
                    i += 4;

                    // vertex 1 (top right)
                    vertexData[i + 0] = x + 1; // position x
                    vertexData[i + 1] = y; // position y
                    vertexData[i + 2] = tx0 + tySize; // texcoord x
                    vertexData[i + 3] = ty0; // texcoord y
                    i += 4;

                    // vertex 2 (bottom left)
                    vertexData[i + 0] = x; // position x
                    vertexData[i + 1] = y + 1; // position y
                    vertexData[i + 2] = tx0; // texcoord x
                    vertexData[i + 3] = ty0 + tySize; // texcoord y
                    i += 4;

                    // vertex 3 (bottom right)
                    vertexData[i + 0] = x + 1; // position x
                    vertexData[i + 1] = y + 1; // position y
                    vertexData[i + 2] = tx0 + tySize; // texcoord x
                    vertexData[i + 3] = ty0 + tySize; // texcoord y
                    i += 4;
                }
            }

            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData,
                BufferUsageHint.StaticDraw);
        }

        private void GenerateElementBufferObject()
        {
            eboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);

            int indexCount = TileMap.Tiles.Length // for each tile
                             * 6; // there are 6 vertices (two triangles, each with 3 vertices)
            uint[] indices = new uint[indexCount];
            uint i = 0, j = 0;
            for (int x = 0; x < TileMap.Width; x++)
            {
                for (int y = 0; y < TileMap.Height; y++)
                {
                    indices[i + 0] = j;
                    indices[i + 1] = j + 1;
                    indices[i + 2] = j + 2;
                    indices[i + 3] = j + 1;
                    indices[i + 4] = j + 2;
                    indices[i + 5] = j + 3;
                    i += 6;
                    j += 4;
                }
            }

            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices,
                BufferUsageHint.StaticDraw);
        }

        private void GenerateVertexArrayObject()
        {
            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);

            GL.EnableVertexAttribArray(0); // position x and y
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);

            GL.EnableVertexAttribArray(1); // texcoord x and y
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, sizeof(float) * 2);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vaoHandle);
            vaoHandle = 0;

            GL.DeleteBuffer(eboHandle);
            eboHandle = 0;

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