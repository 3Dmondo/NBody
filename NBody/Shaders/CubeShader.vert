#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 iPosition;

uniform mat4 model_view_projection;

void main(void)
{
  vec3 pos = vec3(
    aPosition.x * iPosition.w, 
    aPosition.y * iPosition.w,
    aPosition.z * iPosition.w);
  gl_Position = vec4(aPosition * iPosition.w  + iPosition.xyz, 1.0) * model_view_projection;
}

