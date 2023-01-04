using NBody;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

var nativeWindowSettings = new NativeWindowSettings() {
  Size = new Vector2i(800, 600),
  Title = "NBody",
  Flags = ContextFlags.ForwardCompatible,
};

var random = new Random();

var bodies = new Body[10000];
for (int i = 0; i < bodies.Length; i++) {
  bodies[i] = new Body {
    Location = RandomInDisk(10),
    Mass = random.NextDouble() * 0.000000001, 
  };
}

var universe = new Universe(bodies);

using var window = new Window(
  GameWindowSettings.Default,
  nativeWindowSettings,
  universe);
window.Run();


Vector RandomInDisk(double radius)
{
  var phi = random.NextDouble() * 2.0 * Math.PI;
  var r = radius * Math.Pow(random.NextDouble(), 1.0 / 3.0);
  var cosTheta = 2.0 * random.NextDouble() - 1.0;
  return new Vector(
    r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Cos(phi),
    0.0,
    r * cosTheta
  );
}

