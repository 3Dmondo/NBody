using System.Runtime.Intrinsics;

namespace NBody;

internal class OcTreeCellCache
{
  private List<OcTreeCell> OcTrees { get; } = new List<OcTreeCell>();
  public IEnumerable<OcTreeCell> ocTrees => OcTrees.Take(Count);
  public int Count { get; set; }
  public OcTreeCell GetNextOcTree(Vector256<double> location, double width)
  {
    if (Count == OcTrees.Count)
      OcTrees.Add(new OcTreeCell(this));
    var toReturn = OcTrees[Count++];
    toReturn.Reset(location, width);
    return toReturn;
  }
}
