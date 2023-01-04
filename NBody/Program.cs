using NBody;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

var nativeWindowSettings = new NativeWindowSettings() {
  Size = new Vector2i(800, 600),
  Title = "NBody",
  Flags = ContextFlags.ForwardCompatible,
};

var bodies = new Body[10000];

var universe = new Universe(bodies);

using var window = new Window(
  GameWindowSettings.Default,
  nativeWindowSettings,
  universe);
window.Run();