namespace NBody
{
  internal class OcTree
  {
    private readonly OcTreeCache ocTreeCache;
    private readonly OcTree[] subTrees = new OcTree[8];

    /// <summary>
    /// The tolerance of the mass grouping approximation in the simulation. A 
    /// body is only accelerated when the ratio of the tree's width to the 
    /// distance (from the tree's center of mass to the body) is less than this.
    /// </summary>
    private const double Tolerance = 0.5;
    private const double ToleranceSquare = Tolerance * Tolerance;

    /// <summary>
    /// The softening factor for the acceleration equation. This dampens the 
    /// the slingshot effect during close encounters of bodies. 
    /// </summary>
    private const double Epsilon = 0.005; //700;

    /// <summary>
    /// The minimum width of a tree. Subtrees are not created when if their width 
    /// would be smaller than this value. 
    /// </summary>
    private const double MinimumWidth = 0;

    /// <summary>
    /// The number of bodies in the tree. 
    /// </summary>
    public int BodyCount;

    /// <summary>
    /// The total mass of the bodies contained in the tree. 
    /// </summary>
    public double Mass;

    /// <summary>
    /// The location of the center of the tree's bounds. 
    /// </summary>
    public Vector Location { get; private set; }

    /// <summary>
    /// The width of the tree's bounds. 
    /// </summary>
    private double Width;

    public double HalfWidth { get; private set; }
    private double QuarterWidth;
    private double WidthSquare;

    /// <summary>
    /// The location of the center of mass of the bodies contained in the tree. 
    /// </summary>
    public Vector CenterOfMass { get; private set; }

    private Body FirstBody { get; set; }

    public OcTree(OcTreeCache ocTreeCache)
    {
      this.ocTreeCache = ocTreeCache;
    }

    public void Reset(Vector location, double width)
    {
      BodyCount = 0;
      Mass = 0;
      CenterOfMass = Vector.Zero;
      Location = location;
      Width = width;
      HalfWidth = Width / 2.0;
      QuarterWidth = HalfWidth / 2.0;
      WidthSquare = Width * Width;
      for (int i = 0; i < 8; i++) {
        subTrees[i] = null;
      }
    }

    /// <summary>
    /// Adds a body to the tree and subtrees if appropriate. 
    /// </summary>
    /// <param name="body">The body to add to the tree.</param>
    public void Add(Body body)
    {
      CenterOfMass = (Mass * CenterOfMass + body.Mass * body.Location) / (Mass + body.Mass);
      Mass += body.Mass;
      BodyCount++;
      if (BodyCount == 1)
        FirstBody = body;
      else {
        AddToSubtree(body);
        if (BodyCount == 2)
          AddToSubtree(FirstBody);
      }
    }

    private void AddToSubtree(Body body)
    {
      double subtreeWidth = HalfWidth;

      int ii = 1, jj = 2, kk = 4;
      double i = 1.0, j = 1.0, k = 1.0;

      if (body.Location.X < Location.X) {
        ii = 0;
        i = -1.0;
      }

      if (body.Location.Y < Location.Y) {
        jj = 0;
        j = -1.0;
      }

      if (body.Location.Z < Location.Z) {
        kk = 0;
        k = -1.0;
      }

      int subtreeIndex = ii + jj + kk;
      var subtreeLocation = Location + QuarterWidth * new Vector(i, j, k);
      if (subTrees[subtreeIndex] == null)
        subTrees[subtreeIndex] = ocTreeCache.GetNextOcTree(subtreeLocation, subtreeWidth);
      subTrees[subtreeIndex].Add(body);
    }

    public void Accelerate(Body body)
    {
      var d = CenterOfMass - body.Location;
      var dSquare = d * d;

      if ((BodyCount == 1 && body != FirstBody) ||
          (WidthSquare < ToleranceSquare * dSquare)) {
        var distance = Math.Sqrt(dSquare);
        if (distance < Epsilon)
          body.TooClose = true;
        distance = distance + Epsilon;
        var acc = Mass / (distance * distance * distance);
        body.Acceleration += d * acc;
        body.PotentialEnergy += -body.Mass * Mass / distance;
        body.Interactions++;
      } else {
        for (int i = 0; i < 8; i++)
          if (null != subTrees[i]) {
            subTrees[i].Accelerate(body);
          }
      }
    }
  }
}
