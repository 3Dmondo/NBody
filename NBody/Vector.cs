using System.Numerics;

namespace NBody
{
  public struct Vector //:
  //        IAdditionOperators<Vector, Vector, Vector>,
  //        IAdditiveIdentity<Vector, Vector>,
  //        IDivisionOperators<Vector, double, Vector>,
  //        IEquatable<Vector>,
  //        IEqualityOperators<Vector, Vector, bool>,
  //        IMultiplyOperators<Vector, Vector, double>,
  //        IMultiplyOperators<Vector, double, Vector>,
  //        ISubtractionOperators<Vector, Vector, Vector>,
  //        IUnaryNegationOperators<Vector, Vector>
  {
    public Vector(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }

    public static Vector AdditiveIdentity => Zero;
    public static Vector Zero { get; } = new Vector { X = 0, Y = 0, Z = 0 };

    public static Vector operator +(Vector left, Vector right)
    {
      return new Vector {
        X = left.X + right.X,
        Y = left.Y + right.Y,
        Z = left.Z + right.Z
      };
    }

    public static bool operator ==(Vector left, Vector right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Vector left, Vector right)
    {
      return !(left == right);
    }

    public static Vector operator -(Vector left, Vector right)
    {
      return new Vector {
        X = left.X - right.X,
        Y = left.Y - right.Y,
        Z = left.Z - right.Z
      };
    }

    public static Vector operator -(Vector value)
    {
      return new Vector {
        X = -value.X,
        Y = -value.Y,
        Z = -value.Z
      };
    }

    public static Vector operator /(Vector left, double right)
    {
      return new Vector {
        X = left.X / right,
        Y = left.Y / right,
        Z = left.Z / right
      };
    }

    public static double operator *(Vector left, Vector right)
    {
      return left.X * right.X +
             left.Y * right.Y +
             left.Z * right.Z;
    }

    public static Vector operator *(Vector left, double right)
    {
      return new Vector {
        X = left.X * right,
        Y = left.Y * right,
        Z = left.Z * right
      };
    }

    public static Vector operator *(double left, Vector right)
    {
      return right * left;
    }

    public static Vector Cross(Vector left, Vector right)
    {
      return new Vector {
        X = left.Y * right.Z - left.Z * right.Y,
        Y = left.Z * right.X - left.X * right.Z,
        Z = left.X * right.Y - left.Y * right.X,
      };
    }

    public double Magnitude()
    {
      return Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public Vector Unit()
    {
      return this / Magnitude();
    }

    public override bool Equals(object? obj)
    {
      return obj is Vector vector &&
             Equals(vector);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(X, Y, Z);
    }

    public bool Equals(Vector vector)
    {
      return X == vector.X &&
             Y == vector.Y &&
             Z == vector.Z;
    }
  }
}