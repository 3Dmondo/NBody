using OpenTK.Graphics.OpenGL;

namespace NBody;


internal class Body
{
  public const int TrajectoryLength = 100;

  public Vector Position = Vector.Zero;

  public Vector Velocity = Vector.Zero;

  public Vector Acceleration;

  public double PotentialEnergy;

  public double Mass;

  public int Interactions;

  public double KineticEnergy => 0.5 * (Mass * Velocity).MagnitudeSquared() / Mass;

  public CircularBuffer<Vector> Trajectory { get; private set; }

  public bool TooClose { get; internal set; }

  #region "LeapFrog"

  public void ComputePositionAtHalfTimeStep()
  {
    Position += Velocity * 0.5;
  }

  public void ComputeVelocity(bool updateTrajectory)
  {
    Velocity += Acceleration;
    Position += Velocity * 0.5;
    if (updateTrajectory)
      Trajectory.Add(Position);
  }

  #endregion

  #region "runge kutta"

  private Vector PrevPosition;
  private Vector PrevVelocity;

  private Vector K1V;
  private Vector K2V;
  private Vector K3V;
  private Vector K4V;

  private Vector K1L;
  private Vector K2L;
  private Vector K3L;
  private Vector K4L;

  public void ComputeK1()
  {
    PrevPosition = Position;
    PrevVelocity = Velocity;
    K1V = Acceleration;
    K1L = Velocity;
    Velocity = PrevVelocity + K1V * 0.5;
    Position = PrevPosition + K1L * 0.5;
  }

  public void ComputeK2()
  {
    K2V = Acceleration;
    K2L = Velocity;
    Velocity = PrevVelocity + K2V * 0.5;
    Position = PrevPosition + K2L * 0.5;
  }

  public void ComputeK3()
  {
    K3V = Acceleration;
    K3L = Velocity;
    Velocity = PrevVelocity + K3V;
    Position = PrevPosition + K3L;
  }

  public void ComputeK4()
  {
    K4V = Acceleration;
    K4L = Velocity;
  }

  public void UpdateRungeKutta4(bool updateTrajectory)
  {
    Velocity = PrevVelocity + 1.0 / 6.0 * (K1V + 2.0 * K2V + 2.0 * K3V + K4V);
    Position = PrevPosition + 1.0 / 6.0 * (K1L + 2.0 * K2L + 2.0 * K3L + K4L);
    if (updateTrajectory)
      Trajectory.Add(Position);
  }
  #endregion

  internal void InitTrajectory()
  {
    Trajectory = new CircularBuffer<Vector>(TrajectoryLength, Position);
  }
}
