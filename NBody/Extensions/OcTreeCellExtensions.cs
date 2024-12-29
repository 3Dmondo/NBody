using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace NBody.Extensions;

public static class OcTreeCellExtensions
{
  static readonly Vector256<long> base2Indices = Vector256.Create<long>([1L, 2L, 4L, 0L]);
  static readonly Vector256<double> One = Vector256.Create([1.0, 1.0, 1.0, 0.0]);
  static readonly Vector256<double> MinusOne = Vector256.Create([-1.0, -1.0, -1.0, 0.0]);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ChildIndex(Vector256<long> comparison)
  {
    return (int)Vector256.Sum(comparison & base2Indices);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> GetChildCenter(this Vector256<double> cellCenter, Vector256<double> condition, double size)
  {
    var direction = Vector256.ConditionalSelect(condition, One, MinusOne);
    return cellCenter + size * direction;
  }


}
