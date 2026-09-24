using System.Collections;

namespace core;

public class OrderedSet<T> : ICollection<T>
{
  protected readonly List<T> Ordered = [];
  protected readonly HashSet<T> Values = [];
  public int Count => Values.Count;
  public bool IsReadOnly => false;
  public void Add(T item)
  {
    if (Contains(item))
      return;
    Ordered.Add(item);
    Values.Add(item);
  }
  public void Clear()
  {
    Ordered.Clear();
    Values.Clear();
  }
  public bool Contains(T item) => Values.Contains(item);
  public void CopyTo(T[] array, int arrayIndex) => Ordered.CopyTo(array, arrayIndex);
  public IEnumerator<T> GetEnumerator() => Ordered.GetEnumerator();
  public bool Remove(T item) => Ordered.Remove(item) && Values.Remove(item);
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
