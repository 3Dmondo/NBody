using NBody;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

var nativeWindowSettings = new NativeWindowSettings() {
  Size = new Vector2i(800, 600),
  Title = "NBody",
  Flags = ContextFlags.ForwardCompatible,
};

#if DEBUG
var bodies = new Body[100];
#else
var bodies = new Body[10000];
#endif

var universe = new Universe(bodies);

using var window = new Window(
  new GameWindowSettings {
    IsMultiThreaded = false,
    UpdateFrequency = 0,
    RenderFrequency = 0
  },
  nativeWindowSettings,
  universe); ;
window.Run();