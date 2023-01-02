using NBody;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

var nativeWindowSettings = new NativeWindowSettings() {
  Size = new Vector2i(800, 600),
  Title = "NBody",
  Flags = ContextFlags.ForwardCompatible,
};

using var window = new Window(GameWindowSettings.Default, nativeWindowSettings);
window.Run();

