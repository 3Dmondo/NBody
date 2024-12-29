using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
namespace NBody;

public static class VectorExtensions
{
  static readonly Vector256<long> YZXW = Vector256.Create(1L, 2L, 0L, 3L);
  static readonly Vector256<long> ZXYW = Vector256.Create(2L, 0L, 1L, 3L);

  public static Vector256<double> Cross(this Vector256<double> vector1, Vector256<double> vector2)
  {
    var v1 = vector1;
    var v2 = vector2;
    var tmp = -Vector256.Shuffle(v1, ZXYW) * Vector256.Shuffle(v2, YZXW);

    var result = Vector256.FusedMultiplyAdd(
      Vector256.Shuffle(v1, YZXW),
      Vector256.Shuffle(v2, ZXYW),
      tmp);
    return result;
  }

  public static double MagnitudeSquared(this Vector256<double> vector)
  {
    return Vector256.Dot(vector, vector);
  }

  public static double Magnitude(this Vector256<double> vector)
  {
    return Math.Sqrt(vector.MagnitudeSquared());
  }

  public static Vector256<double> Unit(this Vector256<double> vector) => vector / vector.Magnitude();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double X(this Vector256<double> vector) => vector[0];
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Y(this Vector256<double> vector) => vector[1];
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Z(this Vector256<double> vector) => vector[2];

  public static float X(this Vector256<float> vector) => vector[0];
  public static float Y(this Vector256<float> vector) => vector[1];
  public static float Z(this Vector256<float> vector) => vector[2];

  public static float VX(this Vector256<float> vector) => vector[4];
  public static float VY(this Vector256<float> vector) => vector[5];
  public static float VZ(this Vector256<float> vector) => vector[6];
}
