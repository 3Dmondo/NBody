namespace NBody
{

  internal class Body
  {
    /// <summary>
    /// The spatial location of the body. 
    /// </summary>
    public Vector Location = Vector.Zero;

    /// <summary>
    /// The velocity of the body. 
    /// </summary>
    public Vector Velocity = Vector.Zero;

    /// <summary>
    /// The acceleration accumulated for the body during a single simulation 
    /// step. 
    /// </summary>
    public Vector Acceleration;

    public double PotentialEnergy;

    /// <summary>
    /// The mass of the body. 
    /// </summary>
    public double Mass;

    public int Interactions;

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

    public void Update()
    {
      Velocity = PrevVelocity + 1.0 / 6.0 * (K1V + 2.0 * K2V + 2.0 * K3V + K4V);
      Location = PrevLocation + 1.0 / 6.0 * (K1L + 2.0 * K2L + 2.0 * K3L + K4L);
      Acceleration = Vector.Zero;
    }

  }

  internal class Body1
  {
    /// <summary>
    /// The spatial location of the body. 
    /// </summary>
    public Vector Location = Vector.Zero;

    /// <summary>
    /// The velocity of the body. 
    /// </summary>
    public Vector Velocity = Vector.Zero;

    private Vector acceleration;
    private Vector PrevAcceleration;

    /// <summary>
    /// The acceleration accumulated for the body during a single simulation 
    /// step. 
    /// </summary>
    public Vector Acceleration { get; set; }

    /// <summary>
    /// The mass of the body. 
    /// </summary>
    public double Mass;

    public void Update()
    {
      //https://en.wikipedia.org/wiki/Leapfrog_integration
      Location = Location + Velocity + 0.5 * PrevAcceleration;
      Velocity = Velocity + 0.5 * (PrevAcceleration + Acceleration);
      PrevAcceleration= Acceleration;
      Acceleration = Vector.Zero;
    }
  }
}
