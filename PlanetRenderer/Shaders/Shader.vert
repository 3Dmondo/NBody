#version 330 core

layout(location = 0) in vec3 aPosition;


uniform mat4 model_view_projection;
uniform vec3 camera_pos;
uniform vec3 lightPos;

const float pointSize   = 1000.0;

out vec3 lightDir;

void main(void)
{
  gl_Position = vec4(aPosition, 1.0) * model_view_projection;

  float cameraDist = distance(aPosition.xyz, camera_pos);
  gl_PointSize = pointSize / cameraDist;

  vec4 lightPosition = vec4(lightPos, 1.0) * model_view_projection;

  lightDir = normalize((lightPosition - gl_Position).xyz);
}

