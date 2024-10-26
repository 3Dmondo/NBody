#version 330
#define PI 3.1415926538
#define Sqrt2PI 2.506628274631000502415765284811
#define Sigma 0.01
#define CloudFactor (1.0 + Sigma)

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform sampler2D texture4;

uniform vec3 rotation;

in vec3 lightDir;


out vec4 outputColor;

mat4 rotationMatrix(vec3 axis, float angle) {
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

vec3 rotate(vec3 v, vec3 axis, float angle) {
	mat4 m = rotationMatrix(axis, angle);
	return (m * vec4(v, 1.0)).xyz;
}

vec3 tangent(vec3 normal){
  float theta = acos(normal.z);
  float phi = atan(normal.y,normal.x);
  theta += PI * 0.5;
  return vec3(
    sin(theta) * cos(phi),
    sin(theta) * sin(phi),
    cos(theta)
    );
}

vec4 renderBase(vec2 C)
{
    C *= CloudFactor;
    float mag = dot(C,C);

    if (mag > 1.0) return vec4(0,0,0,0);

    vec3 N = vec3(C, sqrt(1 - mag));

    vec3 T = tangent(N);
    vec3 B = normalize(cross(T,N));
    T = normalize(cross(N,B));
    mat3 TBN = mat3(T, B, normalize(N));

    N = N * CloudFactor;
    N = rotate(N,vec3(1,0,0), -0.4101524);
    
    float lat = acos(N.y/length(N)) / PI ;
    float lon = rotation.x + 0.5 * sign(N.z) * acos(-N.x/length(N.xz)) / PI;

    vec3 normal = texture(texture2, vec2(lon,lat)).xyz;

    normal = vec3(normal.x,1-normal.y,normal.z);
    normal = normal * 2.0 - 1.0;   
    normal = normalize(TBN * normal); 

    float diffuse = max(0.0, dot(lightDir, normal));

      return 
        (texture(texture0, vec2(lon,lat)) * diffuse + 
        texture(texture1, vec2(lon,lat)) * (1.0 - pow(diffuse, 0.5)) + 
        texture(texture3, vec2(lon,lat)) * pow(diffuse,15));
}

void main()
{

    vec2 C = (gl_PointCoord - vec2(0.5, 0.5)) * 2;
    float mag = dot(C,C);
    if (mag > 1.0) discard;   // kill pixels outside circle

    vec4 base = renderBase(C);

    vec3 N = vec3(C, sqrt(1 - mag));
    N = rotate(N,vec3(1,0,0), -0.4101524);
    float lat = acos(N.y) / PI ;
    float lon = rotation.x + 0.5 * sign(N.z) * acos(-N.x/length(N.xz)) / PI;

    float diffuse = max(0.0, dot(lightDir, N));

    vec4 cloud = texture(texture4, vec2(lon - rotation.x * 0.1,lat));

    float borderSmoth = 0;
    if (base.w == 0) 
    //if (sqrt(mag) > 1 - Sigma * 1.1)
    {
      borderSmoth = (1 + cos((sqrt(mag)-1/CloudFactor) * PI / Sigma));
      outputColor = (cloud * diffuse + (1-cloud.x) * vec4(0.4,0.4,0.9,1) * pow(diffuse, 2)) * borderSmoth ;
    }
    //else
      
    outputColor += (base * (1-cloud.x) + cloud * diffuse);// * (1-borderSmoth);

}

