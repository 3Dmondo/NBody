namespace NBody
{
  internal class OcTreeCache
  {
    private List<OcTree> OcTrees { get; } = new List<OcTree>();
    public IEnumerable<OcTree> ocTrees => OcTrees.Take(Count);
    public int Count { get; set; }
    public OcTree GetNextOcTree(Vector location, double width)
    {
      if (Count == OcTrees.Count)
        OcTrees.Add(new OcTree(this));
      var toReturn = OcTrees[Count++];
      toReturn.Reset(location, width);
      return toReturn;
    }
  }
}
