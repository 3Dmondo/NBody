//copyright Opentk contributors
//license https://github.com/opentk/LearnOpenTK/blob/master/LICENSE

using OpenTK.Mathematics;

namespace NBody
{
  public class Camera
  {
    public Vector3 Up { get; set; }  = Vector3.UnitY;

    private float _fov = MathHelper.PiOver2 / 2.0f;

    public Camera(Vector3 position, float aspectRatio)
    {
      Position = position;
      AspectRatio = aspectRatio;
    }

    public Vector3 Position { get; set; }

    public Vector3 Target { get; set; }

    public float AspectRatio { private get; set; }

    public Matrix4 GetViewMatrix()
    {
      return Matrix4.LookAt(Position, Target, Up);
    }

    public Matrix4 GetProjectionMatrix()
    {
      return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.001f, 1000f);
    }

  }
}