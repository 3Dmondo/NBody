using OpenTK.Mathematics;

namespace NBody
{
  // This is the camera class as it could be set up after the tutorials on the website.
  // It is important to note there are a few ways you could have set up this camera.
  // For example, you could have also managed the player input inside the camera class,
  // and a lot of the properties could have been made into functions.

  // TL;DR: This is just one of many ways in which we could have set up the camera.
  // Check out the web version if you don't know why we are doing a specific thing or want to know more about the code.
  public class Camera
  {
    public Vector3 Up { get; set; }  = Vector3.UnitY;

    // The field of view of the camera (radians)
    private float _fov = MathHelper.PiOver2 / 2.0f;

    public Camera(Vector3 position, float aspectRatio)
    {
      Position = position;
      AspectRatio = aspectRatio;
    }

    // The position of the camera
    public Vector3 Position { get; set; }

    public Vector3 Target { get; set; }

    // This is simply the aspect ratio of the viewport, used for the projection matrix.
    public float AspectRatio { private get; set; }

    // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
    public Matrix4 GetViewMatrix()
    {
      return Matrix4.LookAt(Position, Target, Up);
    }

    // Get the projection matrix using the same method we have used up until this point
    public Matrix4 GetProjectionMatrix()
    {
      return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.001f, 1000f);
    }

  }
}