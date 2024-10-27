using System.Runtime.Intrinsics;
namespace NBody;

internal static class VectorExtensions
{
  static readonly Vector256<long> indices1 = Vector256.Create(1L, 2L, 0L, 3L);
  static readonly Vector256<long> indices2 = Vector256.Create(2L, 0L, 1L, 3L);

  public static Vector Cross(this Vector vector1, Vector vector2)
  {
    var v1 = vector1.AsVector256();
    var v2 = vector2.AsVector256();
    var result = (Vector256.Shuffle(v1, indices1) * Vector256.Shuffle(v2, indices2) -
            Vector256.Shuffle(v1, indices2) * Vector256.Shuffle(v2, indices1)).AsVector();
    return result;
  }

  public static double Dot(this Vector vector1, Vector vector2)
  {
    return Vector256.Dot(vector1.AsVector256(), vector2.AsVector256());
  }

  public static double MagnitudeSquared(this Vector vector)
  {
    return vector.Dot(vector);
  }

  public static double Magnitude(this Vector vector)
  {
    return Math.Sqrt(vector.MagnitudeSquared());
  }

  public static Vector Unit(this Vector vector) => vector / vector.Magnitude();

  public static double X(this Vector vector) => vector[0];
  public static double Y(this Vector vector) => vector[1];
  public static double Z(this Vector vector) => vector[2];

  public static float X(this Vector256<float> vector) => vector[0];
  public static float Y(this Vector256<float> vector) => vector[1];
  public static float Z(this Vector256<float> vector) => vector[2];
                
  public static float VX(this Vector256<float> vector) => vector[4];
  public static float VY(this Vector256<float> vector) => vector[5];
  public static float VZ(this Vector256<float> vector) => vector[6];
}
