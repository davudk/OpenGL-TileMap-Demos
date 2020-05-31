#version 330 core

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
uniform mat4 projection;

out vec2 texCoord;

void main()
{
    gl_Position = projection * vec4(aPosition, 0.0, 1.0);
    texCoord = aTexCoord;
}