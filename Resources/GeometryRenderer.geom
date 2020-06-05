#version 330 core

uniform mat4 projection;

in VS_OUT {
    uint tileId;
} gs_in[];

out vec2 texCoord;

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

void main() {
    uint tileId = gs_in[0].tileId & 255u;
    float tileX = float(tileId & 15u) / 16.0;
    float tileY = float((tileId >> 4u) & 15u) / 16.0;

    const float B = 1 / 256.0;
    const float S = 1 / 16.0;

    gl_Position = projection * gl_in[0].gl_Position;
    texCoord = vec2(tileX + B, tileY + B);
    EmitVertex();

    gl_Position = projection * (gl_in[0].gl_Position + vec4(1.0, 0.0, 0.0, 0.0));
    texCoord = vec2(tileX + S - B, tileY + B);
    EmitVertex();

    gl_Position = projection * (gl_in[0].gl_Position + vec4(0.0, 1.0, 0.0, 0.0));
    texCoord = vec2(tileX + B, tileY + S - B);
    EmitVertex();

    gl_Position = projection * (gl_in[0].gl_Position + vec4(1.0, 1.0, 0.0, 0.0));
    texCoord = vec2(tileX + S - B, tileY + S - B);
    EmitVertex();

    EndPrimitive();
}  