using System.Diagnostics;
using System.Runtime.Intrinsics;
using NBody.Extensions;

namespace NBody;

internal class OcTreeCell
{
  internal readonly OcTreeCellCache ocTreeCache;
  internal readonly OcTreeCell?[] Children = new OcTreeCell[8];
  private const double Tolerance = 0.5;
  private const double ToleranceSquare = Tolerance * Tolerance;
  private const double Epsilon = 0.005;
  public int BodyCount;
  public double Mass;
  public Vector256<double> Center { get; private set; }
  private double Width;
  public double HalfWidth { get; private set; }
  internal double QuarterWidth;
  private double WidthSquare;
  public Vector256<double> CenterOfMass { get; private set; }
  private Body? FirstBody { get; set; }

  public OcTreeCell(OcTreeCellCache ocTreeCache)
  {
    this.ocTreeCache = ocTreeCache;
  }

  public void Reset(Vector256<double> location, double width)
  {
    BodyCount = 0;
    Mass = 0;
    CenterOfMass = Vector256<double>.Zero;
    Center = location;
    Width = width;
    HalfWidth = Width / 2.0;
    QuarterWidth = HalfWidth / 2.0;
    WidthSquare = Width * Width;
    for (int i = 0; i < 8; i++) {
      Children[i] = null;
    }
  }

  public void Add(Body body)
  {
    CenterOfMass = (Mass * CenterOfMass + body.Mass * body.Position) / (Mass + body.Mass);
    Mass += body.Mass;
    BodyCount++;
    if (BodyCount == 1)
      FirstBody = body;
    else {
      AddToSubtree(body);
      if (BodyCount == 2)
        AddToSubtree(FirstBody!);
    }
  }

  private void AddToSubtree(Body body)
  {
    double subtreeWidth = HalfWidth;
#if false
    (int childIndex, Vector childDirection) = Center.GetChildIndexAndPosition(body.Position);
    var cell = Children[childIndex];
    if (cell == null) {
      var childrenCenter = Center + QuarterWidth * childDirection;
      cell = ocTreeCache.GetNextOcTree(childrenCenter, subtreeWidth);
      Children[childIndex] = cell;
    }
#else
    var comparison = Vector256.GreaterThanOrEqual(Center, body.Position);
    var childIndex = OcTreeCellExtensions.ChildIndex(comparison.AsInt64());
    var cell = Children[childIndex];
    if (cell == null) {
      var childrenCenter = Center.GetChildCenter(comparison, QuarterWidth);
      cell = ocTreeCache.GetNextOcTree(childrenCenter, subtreeWidth);
      Children[childIndex] = cell;
    }
#endif
    cell.Add(body);
  }

  public void Accelerate(Body body)
  {
    var d = CenterOfMass - body.Position;
    var dSquare = d.MagnitudeSquared();

    if ((BodyCount == 1 && body != FirstBody) ||
        (WidthSquare < ToleranceSquare * dSquare)) {
      var distance = Math.Sqrt(dSquare);
      if (distance < Epsilon) {
        body.TooClose = true;
        distance = distance + Epsilon;
      }
      var acc = Mass / (distance * distance * distance);
      body.Acceleration += d * acc;
      body.PotentialEnergy += -0.5 * body.Mass * Mass / distance;
      body.Interactions++;
    } else {
      for (int i = 0; i < 8; i++)
        if (null != Children[i]) {
          Children[i]!.Accelerate(body);
        }
    }
  }
}
