using System.Runtime.Intrinsics;

namespace NBody;

internal class Universe
{
  public const double MassMultiplier = 1e-11;
  private const int trajectoryUpdateFrequency = 10;
  public Body[] Bodies { get; private set; }
  public OcTreeCache OcTreeCache { get; private set; } = new OcTreeCache();
  public OcTree Tree { get; private set; }
  private int frame = 0;

  private StreamWriter file;

  public Universe(Body[] bodies)
  {
    Bodies = bodies;
    SetInitialConditions();
    file = new StreamWriter("nbody.csv");
  }

  public Vector SimulateLeapFrog()
  {
    if (frame++ == trajectoryUpdateFrequency)
      frame = 0;
    Parallel.ForEach(Bodies, b => b.ComputePositionAtHalfTimeStep());
    Tree = AccelerateBodies();
    Parallel.ForEach(Bodies, b => b.ComputeVelocity(frame == 0));
    file.WriteLine($"{KineticEnergy():0.00000E-0};{PotentialEnergy():0.00000E-0};{TotalEnergy():0.00000E-0}");
    return Bodies[0].Position;
  }

  public Vector SimulateRungeKutta4()
  {
    if (frame++ == trajectoryUpdateFrequency)
      frame = 0;
    Tree = AccelerateBodies();
    Parallel.ForEach(Bodies, b => b.ComputeK1());
    Tree = AccelerateBodies();
    Parallel.ForEach(Bodies, b => b.ComputeK2());
    Tree = AccelerateBodies();
    Parallel.ForEach(Bodies, b => b.ComputeK3());
    Tree = AccelerateBodies();
    Parallel.ForEach(Bodies, b => b.ComputeK4());
    Parallel.ForEach(Bodies, b => b.UpdateRungeKutta4(frame == 0));
    file.WriteLine($"{KineticEnergy():0.00000E-0};{PotentialEnergy():0.00000E-0};{TotalEnergy():0.00000E-0}");
    return Bodies[0].Position;
  }

  public double KineticEnergy()
  {
    return Bodies.Select(b => b.KineticEnergy).Sum() / MassMultiplier;
  }

  public double PotentialEnergy()
  {
    return Bodies.Select(b => b.PotentialEnergy).Sum() / MassMultiplier;
  }

  public double TotalEnergy()
  {
    return Bodies.Select(b => b.KineticEnergy + b.PotentialEnergy).Sum() / MassMultiplier;
  }

  private void SetInitialConditions()
  {
    var random = new Random();
    InitLocations(random);
    foreach (var body in Bodies) {
      body.InitTrajectory();
    }
    var tree = AccelerateBodies();
    Parallel.ForEach(Bodies, b => SetBodyVelocity(b, random, tree));
  }

  private static void SetBodyVelocity(Body b, Random random, OcTree tree)
  {
    if (b.Position == Vector.Zero) return;
    var d = tree.CenterOfMass - b.Position;
    var distance = d.Magnitude();
    var dir = b.Acceleration.Cross(new Vector([0.0, 0.0, 1.0, 0.0]));
    dir /= dir.Magnitude();
    var a = b.Acceleration.Magnitude();
    b.Velocity = dir * Math.Pow(distance * a, 0.5);
    var Vz = (random.NextDouble() - 0.5) * b.Velocity.Magnitude() * 0.1f;
    b.Velocity += new Vector([0, 0, Vz, 0]);
  }

  private void InitLocations(Random random)
  {
    Bodies[0] = new Body { Mass = Bodies.Length / 20.0 * MassMultiplier };
    for (int i = 1; i < Bodies.Length; i++) {
      Bodies[i] = new Body {
        Position = RandomInDisk(random, 10),
        Mass = MassMultiplier// + random.NextDouble() * MassMultiplier,
      };
    }
  }

  private Vector RandomInDisk(Random random, double radius)
  {
    var phi = random.NextDouble() * 2.0 * Math.PI;
    var r = 0.1 + radius * Math.Pow(random.NextDouble(), 1.5);
    var cosTheta = 2.0 * random.NextDouble() - 1.0;
    return new Vector([
      r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Cos(phi),
      r * cosTheta,
      0.05 * r * Math.Sqrt(1.0 - cosTheta * cosTheta) * Math.Sin(phi),
      0]
    );
  }

  private OcTree AccelerateBodies()
  {
    double halfWidth = GetHalfWidth();
    OcTree tree = BuildOcTree(halfWidth);
    Parallel.ForEach(Bodies, b => {
      b.PotentialEnergy = 0;
      b.Acceleration = Vector.Zero;
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
      b => {
        var abs = Vector256.Abs(b.Position.AsVector256());
        var ml = Math.Max(abs[0], abs[1]);
        return Math.Max(abs[2], ml);
      }).Max();
  }
}
