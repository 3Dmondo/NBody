using System.IO;
using NBody.Text;
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
    private Shader StarShader;
    private Shader TextShader;
    private Camera _camera;
    private MyTextRenderer TextRenderer;
    private Universe Universe;
    private bool Button1Pressed;
    private int colourVelocity = 0;
    private int fixedSize = 0;
    private bool Pause = false;

    internal Window(
      GameWindowSettings gameWindowSettings,
      NativeWindowSettings nativeWindowSettings,
      Universe universe)
            : base(gameWindowSettings, nativeWindowSettings)
    {
      _vertices = new float[universe.Bodies.Length * 7];
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
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);
      GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
      GL.EnableVertexAttribArray(1);
      GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 7 * sizeof(float), 6 * sizeof(float));
      GL.EnableVertexAttribArray(2);
      StarShader = new Shader("Shaders/starShader.vert", "Shaders/starShader.frag");
      TextShader = new Shader("Shaders/textShader.vert", "Shaders/textShader.frag");

      //_shader.Use();
      _camera = new Camera(Vector3.UnitZ * 20.0f, Size.X / (float)Size.Y);

      TextRenderer = new MyTextRenderer();

    }

    // Now that initialization is done, let's create our render loop.
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit);
      Matrix4 projectionM = Matrix4.CreateScale(new Vector3(1f / this.Size.X, 1f / this.Size.Y, 1.0f));
      projectionM = Matrix4.CreateOrthographicOffCenter(0.0f, this.Size.X, this.Size.Y, 0.0f, -1.0f, 1.0f);
      TextShader.Use();
      GL.UniformMatrix4(1, false, ref projectionM);

      GL.Uniform3(2, new Vector3(0.5f, 0.8f, 0.2f));
      TextRenderer.RenderText("Hello world!", 0, 0, 1, new Vector2(1.0f, 0.0f));

      GL.Enable(EnableCap.PointSprite);
      GL.Enable(EnableCap.VertexProgramPointSize);
      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

      StarShader.Use();
      StarShader.SetMatrix4(
        "model_view_projection",
        Matrix4.Identity *
        _camera.GetViewMatrix() *
        _camera.GetProjectionMatrix());
      StarShader.SetVector3("camera_pos", _camera.Position);
      StarShader.SetInt("colourVelocity", colourVelocity);
      StarShader.SetInt("fixedSize", fixedSize);

      GL.BindVertexArray(_vertexArrayObject);
      GL.DrawArrays(PrimitiveType.Points, 0, _vertices.Length / 3);
      GL.Disable(EnableCap.PointSprite);
      GL.Disable(EnableCap.VertexProgramPointSize);

      SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      if (!Pause) {
        var target = Universe.Simulate();
        _camera.Target = new Vector3(
          (float)target.X,
          (float)target.Y,
          (float)target.Z);
        UpdateVertices();
      }
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

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
      base.OnKeyDown(e);
      switch (e.Key) {
        case Keys.Escape:
          Close();
          break;
        case Keys.C:
          colourVelocity = colourVelocity > 0 ? 0 : 1;
          break;
        case Keys.P:
          Pause = !Pause;
          break;
        case Keys.F:
          fixedSize = fixedSize > 0 ? 0 : 1;
          break;
      }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      base.OnMouseWheel(e);
      _camera.Position *= 1.0f + 0.1f * e.OffsetY;
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
      base.OnMouseMove(e);
      if (Button1Pressed) {
        var sideRotation = Matrix4.CreateFromAxisAngle(_camera.Up, -e.DeltaX * 0.001f);
        var pitchRotation = Matrix4.CreateFromAxisAngle(Vector3.Cross(_camera.Position, _camera.Up), e.DeltaY * 0.001f);
        var transform = sideRotation * pitchRotation;

        var newPosition = new Vector4(_camera.Position, 1.0f) * transform;
        var newUp = new Vector4(_camera.Up, 1.0f) * transform;


        _camera.Position = newPosition.Xyz;
        _camera.Up = newUp.Xyz;
      }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      base.OnMouseDown(e);
      if (e.Button == MouseButton.Button1) {
        Button1Pressed = e.IsPressed;
      }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
      base.OnMouseUp(e);
      if (e.Button == MouseButton.Button1) {
        Button1Pressed = e.IsPressed;
      }
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

      GL.DeleteProgram(StarShader.Handle);

      base.OnUnload();
    }
  }
}
