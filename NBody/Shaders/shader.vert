#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 Vel;
layout(location = 2) in float mass;


uniform mat4 model_view_projection;
uniform vec3 camera_pos;

const float minPointScale = 1.0;
const float maxPointScale = 100.0;
const float maxDistance   = 100.0;

out vec3 ourColor; // output a color to the fragment shader

void main(void)
{
  gl_Position = vec4(aPosition, 1.0) * model_view_projection;
  float cameraDist = distance(aPosition.xyz, camera_pos);

  float pointScale = maxDistance / cameraDist ;
  pointScale = max(pointScale, minPointScale);
  pointScale = min(pointScale, maxPointScale);

  gl_PointSize = log(1e11 * mass) * pointScale;

  //float cx = 0.0;
  //float cy = 0.0;
  //float cz = 0.0;
  //if (Vel.x > 0.0) {
  //  cx += Vel.x * Vel.x;
  //} else {
  //  cy += Vel.x * Vel.x;
  //  cz += Vel.x * Vel.x;
  //}
  //
  //if (Vel.y > 0.0) {
  //  cy += Vel.y * Vel.y;
  //} else {
  //  cx += Vel.y * Vel.y;
  //  cz += Vel.y * Vel.y;
  //}
  //
  //if (Vel.z > 0.0) {
  //  cz += Vel.z * Vel.z;
  //} else {
  //  cy += Vel.z * Vel.z;
  //  cx += Vel.z * Vel.z;
  //}
  //ourColor = vec3(pow(cx,0.25), pow(cy,0.25), pow(cz,0.25));

 ourColor = vec3(0.8, 0.8, 1);
}

