using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PlanetRenderer
{
  internal class Window : GameWindow
  {
    private Camera Camera;
    private bool Button1Pressed;
    private Renderer Renderer;
    private bool pause = false;

    internal Window(
      GameWindowSettings gameWindowSettings,
      NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
    { }

    protected override void OnLoad()
    {
      base.OnLoad();
      GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

      Camera = new Camera(Vector3.UnitZ * 20.0f, Size.X / (float)Size.Y);
      Renderer = new Renderer();

    }

    // Now that initialization is done, let's create our render loop.
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit);

      Renderer.RenderFrame(Camera);

      SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);
      Renderer.UpdateFrame(pause);

    }



    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
      base.OnKeyDown(e);
      switch (e.Key) {
        case Keys.Escape:
          Close();
          break;
        case Keys.Space:
          pause = !pause;
          break;
      }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      base.OnMouseWheel(e);
      Camera.Position += (Camera.Position - Camera.Target) * 0.1f * e.OffsetY;
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
      base.OnMouseMove(e);
      if (Button1Pressed) {
        var sideRotation = Matrix4.CreateFromAxisAngle(Camera.Up, -e.DeltaX * 0.002f);
        var pitchRotation = Matrix4.CreateFromAxisAngle(Vector3.Cross(Camera.Position, Camera.Up), e.DeltaY * 0.002f);
        var transform = sideRotation * pitchRotation;

        var newPosition = new Vector4(Camera.Position, 1.0f) * transform;
        var newUp = new Vector4(Camera.Up, 1.0f) * transform;

        Camera.Position = newPosition.Xyz;
        Camera.Up = newUp.Xyz;
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
      Camera.AspectRatio = Size.X / (float)Size.Y;
      GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload()
    {
      base.OnUnload();
    }
  }
}
