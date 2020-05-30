using System;

namespace TileMapDemos
{
    public class TileMap
    {
        public readonly int Width, Height;
        public readonly byte[] Tiles;
        public int TileSetHandle { get; set; }

        /*
         * This multiplication-based index calculation can be avoided if the width of
         * the map is a power of 2. In that case you can use bit-shifting for efficiency.
         */
        public ref byte this[int x, int y] => ref Tiles[x + y * Width];

        public TileMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Must be positive.");

            Width = width;
            Height = height;
            Tiles = new byte[width * height];
        }
    }
}