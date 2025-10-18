using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PlanetRenderer;

var nativeWindowSettings = new NativeWindowSettings() {
  ClientSize = new Vector2i(800, 600),
  Title = "NBody",
  Flags = ContextFlags.ForwardCompatible,
};
using var window = new Window(
  new GameWindowSettings {
    UpdateFrequency = 0,
  },
  nativeWindowSettings); 
window.Run();