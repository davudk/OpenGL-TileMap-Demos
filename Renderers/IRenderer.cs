using System;
using OpenToolkit.Mathematics;

namespace TileMapDemos.Renderers
{
    public interface IRenderer : IDisposable
    {
        public const int TileSize = 32;
        public const float TileTexSize = 1 / 16f;
        public const float TileTexPadding = 1 / 256f;
        Vector2 Center { get; set; }
        TileMap TileMap { get; }

        void Initialize(TileMap tileMap);
        
        void Render();

        void OnBackBufferResized(int width, int height);
    }
}