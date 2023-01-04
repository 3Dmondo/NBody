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
bodies[0] = new Body { Mass = 0.000000000001 };
for (int i = 1; i < bodies.Length; i++) {
  bodies[i] = new Body {
    Location = RandomInSpere(10),
    Mass = 0.0000000000001, // random.NextDouble() * 2.0
  };
  bodies[i].Velocity = 0.001 * Vector.Cross(bodies[i].Location, new Vector(0, 1, 0)).Unit() * Math.Pow(bodies[i].Location * bodies[i].Location, 0.5);
}

var universe = new Universe(bodies);

using var window = new Window(
  GameWindowSettings.Default,
  nativeWindowSettings,
  universe);
window.Run();



Vector RandomInSpere(double radius)
{
  var phi = random.NextDouble() * 2.0 * Math.PI;
  var r = radius * Math.Pow(random.NextDouble(), 1.0 / 3.0);
  var cosTheta = 2.0 * random.NextDouble() - 1.0;
  return new Vector(
    r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Cos(phi),
    0.1 * r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Sin(phi),
    r * cosTheta
  );
}

