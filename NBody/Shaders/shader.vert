#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 Vel;
layout(location = 2) in float mass;


uniform mat4 model_view_projection;
uniform vec3 camera_pos;
uniform int colourVelocity;
uniform int fixedSize;

const float minPointScale = 1.0;
const float maxPointScale = 100.0;
const float maxDistance   = 100.0;

out vec3 ourColor; // output a color to the fragment shader

void main(void)
{
  gl_Position = vec4(aPosition, 1.0) * model_view_projection;

  if (fixedSize > 0){
    gl_PointSize = 2;
  }
  else{
    float cameraDist = distance(aPosition.xyz, camera_pos);
    float pointScale = maxDistance / cameraDist ;
    pointScale = max(pointScale, minPointScale);
    pointScale = min(pointScale, maxPointScale);
    gl_PointSize = log(1e12 * mass) * pointScale;
  }

  if (colourVelocity > 0)
  {
   float cx = 0.0;
   float cy = 0.0;
   float cz = 0.0;

   float velXSq = Vel.x * Vel.x;
   if (Vel.x > 0.0) {
     cx += velXSq;
   } else {
     cy += velXSq;
     cz += velXSq;
   }

   float velySq = Vel.y * Vel.y;
   if (Vel.y > 0.0) {
     cy += velySq;
   } else {
     cx += velySq;
     cz += velySq;
   }
   
   float velxSq = Vel.z * Vel.z;
   if (Vel.z > 0.0) {
     cz += velxSq;
   } else {
     cy += velxSq;
     cx += velxSq;
   }
   ourColor = vec3(pow(cx,0.5), pow(cy,0.5), pow(cz,0.5));
  }
  else
    ourColor = vec3(0.8, 0.8, 1);
}

