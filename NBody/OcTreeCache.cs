namespace NBody
{
  internal class OcTreeCache
  {
    private List<OcTree> OcTrees { get; } = new List<OcTree>();
    public int Current { get; set; }
    public OcTree GetNextOcTree(Vector location, double width)
    {
      if (Current == OcTrees.Count)
        OcTrees.Add(new OcTree(this));
      var toReturn = OcTrees[Current++];
      toReturn.Reset(location, width);
      return toReturn;
    }
  }
}
