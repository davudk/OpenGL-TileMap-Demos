#version 330 core

uniform sampler2D texture0;
in vec2 texCoord;
out vec4 FragColor;

void main()
{
    FragColor = texture(texture0, texCoord);
}