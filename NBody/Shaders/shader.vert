#version 330 core

layout(location = 0) in vec3 aPosition;

uniform mat4 model_view_projection;
uniform vec3 camera_pos;

const float minPointScale = 1.0;
const float maxPointScale = 100.0;
const float maxDistance   = 1.0;

void main(void)
{
  gl_Position = vec4(aPosition, 1.0) * model_view_projection;
  float cameraDist = distance(aPosition.xyz, camera_pos);

  float pointScale = maxDistance / cameraDist ;
  pointScale = max(pointScale, minPointScale);
  pointScale = min(pointScale, maxPointScale);

  gl_PointSize = 2.0 * pointScale;
}
