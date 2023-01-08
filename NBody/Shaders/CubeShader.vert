#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 iPosition;
layout(location = 2) in float iNumber;

uniform vec3 camera_pos;
uniform mat4 model_view_projection;

out vec3 ourColor;

void main(void)
{
  gl_Position = vec4(aPosition * iPosition.w  + iPosition.xyz, 1.0) * model_view_projection;

  vec3 d = iPosition.xyz - camera_pos;
  if (abs(d.x) > iPosition.w ||
      abs(d.y) > iPosition.w ||
      abs(d.z) > iPosition.w)
    ourColor = vec3(1.0 / (iNumber), 0.0, 0.0);
  else
    ourColor = vec3(0,0,0);
}

