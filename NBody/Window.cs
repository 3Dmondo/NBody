using System.Runtime.InteropServices;
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
    private Camera _camera;

    private TextRenderer TextRenderer;
    private SimulationRenderer SimulationRenderer;
    
    private Universe Universe;

    private bool Button1Pressed;
    private int colourVelocity = 0;
    private int fixedSize = 0;
    private bool Pause = false;
    private bool renderText;
    private bool renderHelp;

    internal Window(
      GameWindowSettings gameWindowSettings,
      NativeWindowSettings nativeWindowSettings,
      Universe universe)
            : base(gameWindowSettings, nativeWindowSettings)
    {
      Universe = universe;
    }

    protected override void OnLoad()
    {
      base.OnLoad();
      GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

      _camera = new Camera(Vector3.UnitZ * 20.0f, Size.X / (float)Size.Y);

      SimulationRenderer = new SimulationRenderer(Universe);

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        TextRenderer = new TextRenderer();
    }

    // Now that initialization is done, let's create our render loop.
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit);

      var ypos = 0.0f;
      
      if (renderHelp &&
          RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        ypos = TextRenderer.RenderText(
          HelpString,
          0, ypos, 1, new Vector2(1.0f, 0),
          Size, new Vector3(1f, 1f, 1f));
      } 
      if (renderText &&
          RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        ypos = TextRenderer.RenderText(
          $"Fps: {1.0 / e.Time:F1}\n" +
          $"Number of stars: {Universe.Bodies.Length}\n" +
          $"Kinetic energy: {Universe.KineticEnergy():0.0E-0}\n" + 
          $"Potential energy: {Universe.PotentialEnergy():0.0E-0}\n" + 
          $"Total energy: {Universe.TotalEnergy():0.0E-0}",
          0, ypos, 1, new Vector2(1.0f, 0),
          Size, new Vector3(1f, 1f, 1f));
      }

      SimulationRenderer.RenderSimulation(
        _camera,
        colourVelocity,
        fixedSize);

      SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      if (!Pause) {
        SimulationRenderer.UpdateFrame(_camera);
      }
    }

    private const string HelpString =
 @"ESC: Close
F1: (show|hide) help
p: Pause simulation
t: Show text info
f: (point sized|blurry) stars
c: Star velocities as colours
Mouse left button + mouse move: move camera";

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
        case Keys.T:
          renderText = !renderText;
          break;
        case Keys.F11:
          if (WindowState == WindowState.Fullscreen)
            WindowState = WindowState.Normal;
          else WindowState = WindowState.Fullscreen;
          break;
        case Keys.F1:
          renderHelp= !renderHelp;
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
      base.OnUnload();
    }
  }
}
