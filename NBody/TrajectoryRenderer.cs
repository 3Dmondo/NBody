using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NBody
{
  internal class TrajectoryRenderer
  {
    private readonly Shader Shader;
    private readonly Universe Universe;
    private readonly float[] Vertices;
    private readonly int VertexBufferObject;
    private readonly int VertexArrayObject;

    public TrajectoryRenderer(Universe universe)
    {
      Universe = universe;
      VertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
      VertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(VertexArrayObject);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);
      Shader = new Shader("Shaders/trajectoryShader.vert", "Shaders/trajectoryShader.frag");
      Shader.Use();

      Vertices = new float[Universe.Bodies.Length * Body.TrajectoryLength * 3];
    }

    public void Render(Camera camera)
    {
      Shader.Use();
      Shader.SetMatrix4(
        "model_view_projection",
        Matrix4.Identity *
        camera.GetViewMatrix() *
        camera.GetProjectionMatrix());

      GL.BindVertexArray(VertexArrayObject);
      var offset = 0;
      for (int i = 0; i < Universe.Bodies.Length; i++) {
        GL.DrawArrays(PrimitiveType.LineStrip, offset, Body.TrajectoryLength);
        offset += Body.TrajectoryLength;
      }
    }

    public void Update()
    {
      int j = 0;
      foreach (var body in Universe.Bodies) {
        foreach (var location in body.Trajectory.GetItems()) {
          Vertices[j++] = (float)location.X;
          Vertices[j++] = (float)location.Y;
          Vertices[j++] = (float)location.Z;
        }
      }
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StreamDraw);
    }
  }
}
