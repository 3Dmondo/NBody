namespace NBody
{
  internal class Universe
  {
    public const double MassMultiplier = 1e-10;
    public Body[] Bodies { get; private set; }
    public OcTreeCache OcTreeCache { get; private set; } = new OcTreeCache();
    public OcTree Tree { get; private set; }

    public Universe(Body[] bodies)
    {
      Bodies = bodies;
      InitVelocities();
    }

    public Vector Simulate()
    {
      Tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK1());
      Tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK2());
      Tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK3());
      Tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => b.ComputeK4());
      Parallel.ForEach(Bodies, b => b.Update());
      return Bodies[0].Location;
    }

    public double KineticEnergy()
    {
      return Bodies.Select(b => b.Mass * (b.Velocity * b.Velocity)).Sum() / MassMultiplier;
    }

    public double PotentialEnergy()
    {
      return Bodies.Select(b => b.PotentialEnergy).Sum() / MassMultiplier;
    }

    public double TotalEnergy()
    {
      return Bodies.Select(b => b.Mass * (b.Velocity * b.Velocity) + b.PotentialEnergy).Sum() / MassMultiplier;
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
      Bodies[0] = new Body { Mass = Bodies.Length / 20.0 * MassMultiplier };
      for (int i = 1; i < Bodies.Length; i++) {
        Bodies[i] = new Body {
          Location = RandomInDisk(random, 10),
          Mass = MassMultiplier// + random.NextDouble() * MassMultiplier,
        };
      }
    }

    private Vector RandomInDisk(Random random, double radius)
    {
      var phi = random.NextDouble() * 2.0 * Math.PI;
      var r = 0.1 + radius * Math.Pow(random.NextDouble(), 1.5);
      var cosTheta = 2.0 * random.NextDouble() - 1.0;
      return new Vector(
        r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Cos(phi),
        r * cosTheta,
        0.05 * r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Sin(phi)
      );
    }

    private OcTree AccelerateBodies()
    {
      double halfWidth = GetHalfWidth();
      OcTree tree = BuildOcTree(halfWidth);
      Parallel.ForEach(Bodies,b => {
        b.Interactions = 0;
        b.TooClose = false;
        tree.Accelerate(b);
        });
      return tree;
    }

    private OcTree BuildOcTree(double halfWidth)
    {
      OcTreeCache.Count = 0;
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
