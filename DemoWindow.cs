using System;
using System.Reflection;
using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using SixLabors.ImageSharp.PixelFormats;
using TileMapDemos.Renderers;
using Image = SixLabors.ImageSharp.Image;

namespace TileMapDemos
{
    public class DemoWindow : GameWindow
    {
        private static readonly NativeWindowSettings Settings = new NativeWindowSettings()
        {
            Size = new Vector2i(1366, 768),
            Profile = ContextProfile.Compatability
        };

        public IRenderer Renderer { get; private set; } = new ImmediateRenderer();
        public TileMap TileMap { get; set; }

        public DemoWindow() : base(GameWindowSettings.Default, Settings)
        {
            VSync = VSyncMode.Adaptive;
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);

            Random rnd = new Random();
            TileMap = new TileMap(256, 256);
            for (int i = 0; i < TileMap.Tiles.Length; i++)
            {
                TileMap.Tiles[i] = (byte) rnd.Next(4);
            }

            TileMap.TileSetHandle = LoadTileSetTexture();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            Renderer?.Initialize(TileMap);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            Renderer?.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            Title = "OpenGL TileMap Demos - " + Renderer.GetType().Name;
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            Renderer?.Render();

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Renderer?.OnBackBufferResized(e.Width, e.Height);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            if (MouseState.IsAnyButtonDown)
            {
                Renderer.Center += e.Delta * 1.25f;
            }
        }

        private int LoadTileSetTexture()
        {
            GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TileMap.TileSetHandle);
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("opengl_tilemap_demos.Resources.TileSet.png");
            using var image = Image.Load<Rgba32>(stream);

            byte[] data = new byte[image.Width * image.Height * 4];
            var i = 0;
            for (var y = 0; y < image.Height; y++)
                foreach (var p in image.GetPixelRowSpan(y))
                {
                    data[i++] = p.R;
                    data[i++] = p.G;
                    data[i++] = p.B;
                    data[i++] = p.A;
                }

            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);

            return handle;
        }

        private static void Main(string[] args)
        {
            using var window = new DemoWindow();
            window.Run();
        }
    }
}