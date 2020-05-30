using System;
using System.Drawing;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;

namespace TileMapDemos.Renderers
{
    public class ImmediateRenderer : IRenderer
    {
        public Vector2 Center { get; set; }
        public TileMap TileMap { get; private set; }
        private int backBufferWidth, backBufferHeight;

        public void Initialize(TileMap tileMap)
        {
            GL.ClearColor(Color.DimGray);
            GL.ClipControl(ClipOrigin.UpperLeft, ClipDepthMode.NegativeOneToOne);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            TileMap = tileMap;
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.LoadIdentity();
            GL.Ortho(0f, backBufferWidth, 0, backBufferHeight, 0f, 1f);
            GL.Translate(backBufferWidth / 2, backBufferHeight / 2, 0);
            GL.Translate(-Center.X, -Center.Y, 0);

            GL.BindTexture(TextureTarget.Texture2D, TileMap.TileSetHandle);
            GL.Begin(PrimitiveType.Triangles);
            for (int x = 0; x < TileMap.Width; x++)
            {
                for (int y = 0; y < TileMap.Height; y++)
                {
                    byte tile = TileMap[x, y];
                    float tileTexCoordX0 = (tile & 15) * IRenderer.TileTexSize + IRenderer.TileTexPadding;
                    float tileTexCoordY0 = (tile >> 4) * IRenderer.TileTexSize + IRenderer.TileTexPadding;
                    float tileTexCoordX1 = tileTexCoordX0 + IRenderer.TileTexSize - IRenderer.TileTexPadding * 2;
                    float tileTexCoordY1 = tileTexCoordY0 + IRenderer.TileTexSize - IRenderer.TileTexPadding * 2;

                    float tileX0 = x * IRenderer.TileSize;
                    float tileX1 = tileX0 + IRenderer.TileSize;
                    float tileY0 = y * IRenderer.TileSize;
                    float tileY1 = tileY0 + IRenderer.TileSize;

                    GL.TexCoord2(tileTexCoordX0, tileTexCoordY0);
                    GL.Vertex2(tileX0, tileY0);

                    GL.TexCoord2(tileTexCoordX1, tileTexCoordY0);
                    GL.Vertex2(tileX1, tileY0);

                    GL.TexCoord2(tileTexCoordX0, tileTexCoordY1);
                    GL.Vertex2(tileX0, tileY1);


                    GL.TexCoord2(tileTexCoordX1, tileTexCoordY0);
                    GL.Vertex2(tileX1, tileY0);

                    GL.TexCoord2(tileTexCoordX0, tileTexCoordY1);
                    GL.Vertex2(tileX0, tileY1);

                    GL.TexCoord2(tileTexCoordX1, tileTexCoordY1);
                    GL.Vertex2(tileX1, tileY1);
                }
            }

            GL.End();
        }

        public void Dispose() { }

        public void OnBackBufferResized(int width, int height)
        {
            backBufferWidth = width;
            backBufferHeight = height;
            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
        }
    }
}