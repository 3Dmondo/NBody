namespace NBody
{
  internal class CircularBuffer<T>
  {
    private readonly int Capacity;
    private readonly T[] Items;
    private int Start;

    public CircularBuffer(int capacity, T initialValue)
    {
      Capacity = capacity;
      Items = Enumerable.Repeat(initialValue, Capacity).ToArray();
      Start = 0;
    }

    public void Add(T item)
    {
      Items[Start] = item;
      if (++Start == Capacity)
        Start = 0;
    }

    public IEnumerable<T> GetItems()
    {
      for (int i = Start; i < Capacity; i++)
        yield return Items[i];
      for (int i = 0; i < Start; i++)
        yield return Items[i];
    }
  }
}
