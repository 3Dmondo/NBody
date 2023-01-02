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

    private Vector acceleration;
    private Vector PrevAcceleration;

    /// <summary>
    /// The acceleration accumulated for the body during a single simulation 
    /// step. 
    /// </summary>
    public Vector Acceleration {
      get => acceleration;
      set {
        PrevAcceleration = Acceleration;
        acceleration = value;
      }
    }

    /// <summary>
    /// The mass of the body. 
    /// </summary>
    public double Mass;

    public void Update()
    {
      //https://en.wikipedia.org/wiki/Leapfrog_integration
      Location = Location + Velocity + 0.5 * PrevAcceleration;
      Velocity = Velocity + 0.5 * (PrevAcceleration + Acceleration);
    }
  }
}
