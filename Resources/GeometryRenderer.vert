#version 330 core

uniform mat4 projection;
uniform ivec2 mapSize;

layout (location = 0) in uint aTileId;

out VS_OUT {
    uint tileId;
} vs_out;

void main()
{
    int i = gl_VertexID;
    float x = float(i / mapSize.y); //float(i & 15);
    float y = float(i % mapSize.y); //float((i >> 4) & 15);
    gl_Position = vec4(x, y, 0, 1);
    
    vs_out.tileId = aTileId;
}