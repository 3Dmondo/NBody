using System;

namespace NBody
{
  internal class Universe
  {
    private const double MassMultiplier = 1e-10;
    public Body[] Bodies { get; private set; }
    private OcTreeCache OcTreeCache = new OcTreeCache();

    public Universe(Body[] bodies)
    {
      Bodies = bodies;
      InitVelocities();
    }

    private void InitVelocities()
    {
      var random = new Random();
      InitLocations(random);
      var tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => SetBodyVelocity(b, random, tree));
    }

    private static void SetBodyVelocity(Body b, Random random, OcTree tree)
    {
      if (b.Location == Vector.Zero) return;
      var d = tree.CenterOfMass - b.Location;
      var distance = Math.Sqrt(d * d);
      var dir = Vector.Cross(b.Acceleration, new Vector(0, 0, 1));
      dir /= dir.Magnitude();
      var a = Math.Sqrt(b.Acceleration * b.Acceleration);
      b.Velocity = dir * Math.Pow(distance * a, 0.5);
      var Vz = (random.NextDouble() - 0.5) * b.Velocity.Magnitude() * 0.1f;
      b.Velocity += new Vector(0, 0, Vz);
    }
    private void InitLocations(Random random)
    {
      Bodies[0] = new Body { Mass = Bodies.Length / 10.0 * MassMultiplier };
      for (int i = 1; i < Bodies.Length; i++) {
        Bodies[i] = new Body {
          Location = RandomInDisk(random, 5),
          Mass = MassMultiplier + random.NextDouble() * MassMultiplier,
        };
      }
    }

    Vector RandomInDisk(Random random, double radius)
    {
      var phi = random.NextDouble() * 2.0 * Math.PI;
      var r = radius * Math.Pow(random.NextDouble(), 2.0);
      var cosTheta = 2.0 * random.NextDouble() - 1.0;
      return new Vector(
        r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Cos(phi),
        r * cosTheta,
        0.1 * r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Sin(phi)
      );
    }

    public Vector Simulate()
    {
      OcTree tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK1());
      tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK2());
      tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK3());
      tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK4());
      Parallel.ForEach(Bodies, b => b.Update());

      return tree.CenterOfMass;
    }

    private OcTree AccelerateBodies()
    {
      double halfWidth = GetHalfWidth();
      OcTree tree = BuildOcTree(halfWidth);
      Parallel.ForEach(Bodies, tree.Accelerate);
      return tree;
    }

    private OcTree BuildOcTree(double halfWidth)
    {
      OcTreeCache.Current = 0;
      var tree = OcTreeCache.GetNextOcTree(Vector.Zero, 2.1 * halfWidth);

      foreach (var body in Bodies) {
        tree.Add(body);
      }

      return tree;
    }

    private double GetHalfWidth()
    {
      return Bodies.
        Select(
          b => Math.Max(
            Math.Max(
              Math.Abs(b.Location.X),
              Math.Abs(b.Location.Y)),
            Math.Abs(b.Location.Z))).
        Max();
    }

  }
}
