#version 330

out vec4 outputColor;

void main()
{
    vec2 C = vec2(2.0 * (gl_PointCoord.x-0.5),2.0*(gl_PointCoord.y-0.5));
    float mag = dot(C,C);
    if (mag > 1.0) discard;   // kill pixels outside circle

    outputColor = vec4(1 ,1, 1.0, 0.2);
}