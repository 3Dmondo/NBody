using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace NBody
{
  internal class Window : GameWindow
  {
    private readonly float[] _vertices;
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private Shader _shader;
    private Camera _camera;

    private Universe Universe;

    internal Window(
      GameWindowSettings gameWindowSettings,
      NativeWindowSettings nativeWindowSettings,
      Universe universe)
            : base(gameWindowSettings, nativeWindowSettings)
    {
      _vertices = new float[universe.Bodies.Length * 3];
      Universe = universe;
    }

    protected override void OnLoad()
    {
      base.OnLoad();
      GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
      _vertexBufferObject = GL.GenBuffer();
      UpdateVertices();
      _vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(_vertexArrayObject);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);
      _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
      _shader.Use();
      _camera = new Camera((Vector3.UnitZ * 10.0f) + (Vector3.UnitY * 10.0f), Size.X / (float)Size.Y);
    }

    // Now that initialization is done, let's create our render loop.
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit);
      GL.Enable(EnableCap.PointSprite);
      GL.Enable(EnableCap.VertexProgramPointSize);
      GL.Enable(EnableCap.Blend);
      _shader.Use();
      GL.BindVertexArray(_vertexArrayObject);
      var model = Matrix4.Identity;
      _shader.SetMatrix4(
        "model_view_projection",
        model *
        _camera.GetViewMatrix() *
        _camera.GetProjectionMatrix());
      _shader.SetVector3("camera_pos", _camera.Position);
      GL.DrawArrays(PrimitiveType.Points, 0, _vertices.Length / 3);
      GL.Disable(EnableCap.PointSprite);
      GL.Disable(EnableCap.VertexProgramPointSize);
      SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      var input = KeyboardState;

      if (input.IsKeyDown(Keys.Escape)) {
        Close();
      }

      var centerOfMass = Universe.Simulate();
      UpdateVertices();
    }

    private void UpdateVertices()
    {
      var j = 0;
      for (int i = 0; i < Universe.Bodies.Length; i++) {
        var bodyLocation = Universe.Bodies[i].Location;
        _vertices[j++] = (float)bodyLocation.X;
        _vertices[j++] = (float)bodyLocation.Y;
        _vertices[j++] = (float)bodyLocation.Z;
      }
      GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);
    }

    // In the mouse wheel function, we manage all the zooming of the camera.
    // This is simply done by changing the FOV of the camera.
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      base.OnMouseWheel(e);
      _camera.Position *= 1.0f + 0.1f * e.OffsetY;
      //_camera.Fov -= e.OffsetY;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
      base.OnResize(e);
      _camera.AspectRatio = Size.X / (float)Size.Y;
      GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload()
    {
      // Unbind all the resources by binding the targets to 0/null.
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindVertexArray(0);
      GL.UseProgram(0);

      // Delete all the resources.
      GL.DeleteBuffer(_vertexBufferObject);
      GL.DeleteVertexArray(_vertexArrayObject);

      GL.DeleteProgram(_shader.Handle);

      base.OnUnload();
    }
  }
}
