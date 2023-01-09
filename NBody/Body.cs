namespace NBody
{

  internal class Body
  {
    public const int TrajectoryLength = 100;

    private Vector PrevLocation;
    private Vector PrevVelocity;

    private Vector K1V;
    private Vector K2V;
    private Vector K3V;
    private Vector K4V;

    private Vector K1L;
    private Vector K2L;
    private Vector K3L;
    private Vector K4L;

    public Vector Location = Vector.Zero;

    public Vector Velocity = Vector.Zero;

    public Vector Acceleration;

    public double PotentialEnergy;

    public double Mass;

    public int Interactions;

    public CircularBuffer<Vector> Trajectory { get; private set; }

    public bool TooClose { get; internal set; }

    public void ComputeK1()
    {
      PrevLocation = Location;
      PrevVelocity = Velocity;
      K1V = Acceleration;
      K1L = Velocity;
      Velocity = PrevVelocity + Acceleration * 0.5;
      Location = PrevLocation + Velocity * 0.5;
      Acceleration = Vector.Zero;
      PotentialEnergy = 0;
    }

    public void ComputeK2()
    {
      K2V = Acceleration;
      K2L = Velocity;
      Velocity = PrevVelocity + Acceleration * 0.5;
      Location = PrevLocation + Velocity * 0.5;
      Acceleration = Vector.Zero;
      PotentialEnergy = 0;
    }

    public void ComputeK3()
    {
      K3V = Acceleration;
      K3L = Velocity;
      Velocity = PrevVelocity + Acceleration * 0.5;
      Location = PrevLocation + Velocity * 0.5;
      Acceleration = Vector.Zero;
      PotentialEnergy = 0;
    }

    public void ComputeK4()
    {
      K4V = Acceleration;
      K4L = Velocity;
      Velocity = PrevVelocity + Acceleration;
      Location = PrevLocation + Velocity;
      Acceleration = Vector.Zero;
    }

    public void Update(bool updateTrajectory)
    {
      Velocity = PrevVelocity + 1.0 / 6.0 * (K1V + 2.0 * K2V + 2.0 * K3V + K4V);
      Location = PrevLocation + 1.0 / 6.0 * (K1L + 2.0 * K2L + 2.0 * K3L + K4L);
      Acceleration = Vector.Zero;
      if (updateTrajectory)
        Trajectory.Add(Location);
    }

    internal void InitTrajectory()
    {
      Trajectory = new CircularBuffer<Vector>(TrajectoryLength, Location);
    }
  }

}
