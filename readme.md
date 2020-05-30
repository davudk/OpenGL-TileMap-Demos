![Simple screenshot](Screenshots/Simple.png?raw=true)

## OpenGL TileMap Demos

I hope to demonstrate in this repository different ways to render a tilemap onto the screen,
and explain why some methods are preferred over others.

These are the different methods of rendering I intend on covering:
1. [Immediate Rendering (yuck!)](#1-immediate-rendering)
2. Vertex-Buffered Rendering
3. Element-Buffered Rendering
4. Compute Shader Rendering

#### 1. Immediate Rendering
**This shouldn't be done in real production applications.** This is demonstrated to convey how OpenGL works under the hood. This method of rendering has been deprecated for years and it is a highly inefficient way of using the GPU.

The reason why it is inefficient is because it requires too many API calls to render even a simple primitive (e.g. a triangle) onto the screen. Each API call has overhead associated with it. This is because each API call is directed to your machine's GPU. So, instead of sending many (thousands or tens of thousands) relatively-small commands to the GPU (which probably complete in less time than the sum of their overhead), it makes more sense to send fewer, but larger commands.

Immediate rendering works by controlling the GPU (through OpenGL) in a very simple way. Using OpenTK, we can render a triangle onto the screen as such:
```C#
GL.Begin(PrimitiveType.Triangles);

GL.Color3(Color.Red); GL.Vertex2(0, -0.5);
GL.Color3(Color.Green); GL.Vertex2(-0.5, 0.5);
GL.Color3(Color.Blue); GL.Vertex2(0.5, 0.5);

GL.End();
```

Notice how many API calls it took to render a single triangle? Just imagine how many it would require to render a large 3D model or a game full of 2D sprites.

For more information, see the [ImmediateRenderer.cs](Renderers/ImmediateRenderer.cs) file.

#### 2. Vertex-Buffered Rendering
Coming soon.

#### 3. Element-Buffered Rendering
Coming soon.

#### 4. Compute Shader Rendering
Coming soon.

## Other notes on rendering tile maps

#### Fixing the lines-between-tiles bug
![Lines between tiles](Screenshots/LinesBetweenTiles.png?raw=true)

Notice how there are lines between the tiles in the image above? Sometimes these horizontal and vertical lines appear. Don't worry, it's fairly easy to fix.

**Why does it happen?** This happens because you're performing sub-pixel rendering without using texture borders. This causes OpenGL to read/take pixel from the texture that don't fall into the boundaries of the tex-coords for a given tile. In other words, when a sub-pixel is rendered on the boundary of the tile, OpenGL assumes that you'll want to retrieve a pixel on the boundary of your texcoords. In texture atlasses this can be a problem, since that one pixel can mess up your tile map -as seen above.

**How can I fix it?** In this repository the sample tile set texture is a grid of 16-by-16 tiles. This means that the top-left-most tile has the following source rect: `(x0=0, y0=0, x1=1/16f, y1=1/16f)`. In fact, for any given tile referenced by its integer coordinates from 1 to 16, `(xi, yi)`, we can find its source rect using the following equation: `(x0=xi/16f, y0=yi/16f, x1=(xi+1)/16f, y1=(yi+1)/16f)`.

This can be fixed by performing a correction on the texcoords by adding a small padding: e.g. `1 / 256f`. So, the `x0` and `y0` fields will be increased by that padding amount, whereas the `x1` and `y1` fields will be decreased by it. The equation then becomes: `(x0=xi/16f+1/256f, y0=yi/16f+1/256f, x1=(xi+1)/16f-1/256f, y1=(yi+1)/16f-1/256f)`

This fix has already been comitted into the repository, so you likely will not see this problem to begin with.