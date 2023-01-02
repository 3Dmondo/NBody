namespace NBody
{
  internal class Universe
  {
    private Body[] Bodies;

    public Universe(Body[] bodies)
    {
      Bodies = bodies;
    }

    private OcTreeCache OcTreeCache = new OcTreeCache();

    public void Simulate()
    {
      foreach (var body in Bodies) {
        body.Update();
      }

      var halfWidth = Bodies.
        Select(
          b => Math.Max(
            Math.Max(
              Math.Abs(b.Location.X), 
              Math.Abs(b.Location.Y)),
            Math.Abs(b.Location.Z))).
        Max();

      OcTreeCache.Current = 0;
      var tree = OcTreeCache.GetNextOcTree(Vector.Zero, 2.1 * halfWidth);

      foreach (var body in Bodies) {
        tree.Add(body);
      }

      foreach (var body in Bodies) {
        tree.Accelerate(body);
      }
    }
  }
}
