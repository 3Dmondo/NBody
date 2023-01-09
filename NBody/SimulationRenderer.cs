using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NBody
{
  internal class SimulationRenderer
  {
    private readonly Universe Universe;
    private Shader StarShader;
    private readonly float[] _vertices;
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private const int SimulationStepsPerFrame = 1;

    public SimulationRenderer(Universe universe)
    {
      _vertices = new float[universe.Bodies.Length * 7];
      Universe = universe;
      _vertexBufferObject = GL.GenBuffer();
      UpdateVertices();
      _vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(_vertexArrayObject);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);
      GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
      GL.EnableVertexAttribArray(1);
      GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 7 * sizeof(float), 6 * sizeof(float));
      GL.EnableVertexAttribArray(2);
      StarShader = new Shader("Shaders/starShader.vert", "Shaders/starShader.frag");
    }

    public void RenderSimulation(
      Camera camera,
      int colourVelocity,
      int blurry)
    {
      GL.Enable(EnableCap.PointSprite);
      GL.Enable(EnableCap.VertexProgramPointSize);

      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

      StarShader.Use();
      StarShader.SetMatrix4(
        "model_view_projection",
        Matrix4.Identity *
        camera.GetViewMatrix() *
        camera.GetProjectionMatrix());
      StarShader.SetVector3("camera_pos", camera.Position);
      StarShader.SetInt("colourVelocity", colourVelocity);
      StarShader.SetInt("blurry", blurry);
      StarShader.SetFloat("MassMultiplier", 10f / (float)Universe.MassMultiplier);

      GL.BindVertexArray(_vertexArrayObject);
      GL.DrawArrays(PrimitiveType.Points, 0, _vertices.Length / 7);
      GL.Disable(EnableCap.PointSprite);
      GL.Disable(EnableCap.VertexProgramPointSize);
    }

    public void UpdateFrame(Camera camera)
    {
      Vector target = new Vector();
      for (int i = 0; i < SimulationStepsPerFrame; i++)
        target = Universe.Simulate();
      camera.Target = new Vector3(
        (float)target.X,
        (float)target.Y,
        (float)target.Z);
      UpdateVertices();
    }

    private void UpdateVertices()
    {
      var j = 0;
      for (int i = 0; i < Universe.Bodies.Length; i++) {
        var bodyLocation = Universe.Bodies[i].Location;
        var bodyVelocity = Universe.Bodies[i].Velocity.Unit();
        _vertices[j++] = (float)bodyLocation.X;
        _vertices[j++] = (float)bodyLocation.Y;
        _vertices[j++] = (float)bodyLocation.Z;
        _vertices[j++] = (float)bodyVelocity.X;
        _vertices[j++] = (float)bodyVelocity.Y;
        _vertices[j++] = (float)bodyVelocity.Z;
        _vertices[j++] = (float)Universe.Bodies[i].Mass;
      }
      GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);
    }
  }
}
