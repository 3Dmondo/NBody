namespace NBody
{
  internal class Universe
  {
    public Body[] Bodies { get; private set; }

    public Universe(Body[] bodies)
    {
      Bodies = bodies;
      InitVelocities();
    }

    private void InitVelocities()
    {
      var random = new Random();
      var tree = AccelerateBodies();
      Parallel.ForEach(Bodies, b => SetBodyVelocity(b, random, tree));
    }

    private static void SetBodyVelocity(Body b, Random random, OcTree tree)
    {
      if (b.Location == Vector.Zero) return;
      var d = tree.CenterOfMass - b.Location;
      var distance = Math.Sqrt(d * d);
      var dir = Vector.Cross(b.Location, new Vector(0, 1, 0));
      dir /= dir.Magnitude();
      var a = Math.Sqrt(b.Acceleration * b.Acceleration);
      b.Velocity = dir * Math.Pow(distance * a, 0.5);
      var Vy = (random.NextDouble() - 0.5) * b.Velocity.Magnitude() * 0.05f;
      b.Velocity += new Vector(0, Vy, 0);
    }

    private OcTreeCache OcTreeCache = new OcTreeCache();

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
