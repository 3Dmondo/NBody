#version 330 core

layout(location = 0) in vec3 aPosition;

uniform mat4 model_view_projection;

void main(void)
{
  gl_Position = vec4(aPosition, 1.0) * model_view_projection;
}

