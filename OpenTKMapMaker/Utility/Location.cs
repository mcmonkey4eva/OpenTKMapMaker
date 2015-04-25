using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTKMapMaker.Utility
{
    public struct Location : IEquatable<Location>
    {
        /// <summary>
        /// A Location of (0, 0, 0).
        /// </summary>
        public static Location Zero = new Location(0);
        /// <summary>
        /// A Location of (1, 1, 1).
        /// </summary>
        public static Location One = new Location(1);
        /// <summary>
        /// A location of (1, 0, 0).
        /// </summary>
        public static Location UnitX = new Location(1, 0, 0);
        /// <summary>
        /// A location of (0, 1, 0).
        /// </summary>
        public static Location UnitY = new Location(0, 1, 0);
        /// <summary>
        /// A location of (0, 0, 1).
        /// </summary>
        public static Location UnitZ = new Location(0, 0, 1);
        /// <summary>
        /// A location of (NaN, NaN, NaN).
        /// </summary>
        public static Location NaN = new Location(double.NaN, double.NaN, double.NaN);
        /// <summary>
        /// The X coordinate of this location.
        /// </summary>
        public double X;
        /// <summary>
        /// The Y coordinate of this location.
        /// </summary>
        public double Y;
        /// <summary>
        /// The Z coordinate of this location.
        /// </summary>
        public double Z;

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new ArgumentOutOfRangeException("index", index, "Must be between 0 and 2");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index", index, "Must be between 0 and 2");
                }
            }
        }

        public Location(double _X, double _Y, double _Z)
        {
            X = _X;
            Y = _Y;
            Z = _Z;
        }

        public Location(double _Point)
        {
            X = _Point;
            Y = _Point;
            Z = _Point;
        }

        public Location(OpenTK.Vector3 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
        }

        public Location(float _X, float _Y, float _Z)
        {
            X = _X;
            Y = _Y;
            Z = _Z;
        }

        public Location(float _Point)
        {
            X = _Point;
            Y = _Point;
            Z = _Point;
        }

        /// <summary>
        /// Returns the full linear length of the vector location, squared for efficiency.
        /// </summary>
        /// <returns>The squared length</returns>
        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// Returns the full linear length of the vector location, which goes through a square-root operation (inefficient).
        /// </summary>
        /// <returns>The square-rooted length</returns>
        public double Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Returns whether the location is NaN.
        /// </summary>
        /// <returns>whether the location is NaN</returns>
        public bool IsNaN()
        {
            return double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z);
        }

        /// <summary>
        /// Returns the dot product of this and another location.
        /// </summary>
        /// <param name="two">The second location</param>
        /// <returns>The dot product</returns>
        public double Dot(Location two)
        {
            return X * two.X + Y * two.Y + Z * two.Z;
        }

        /// <summary>
        /// Returns the location as a string in the form: (X, Y, Z)
        /// Inverts .FromString()
        /// </summary>
        /// <returns>The location string</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        /// <summary>
        /// Returns the location as a string in the form: X, Y, Z
        /// Inverts .FromString()
        /// </summary>
        /// <returns>The location string</returns>
        public string ToSimpleString()
        {
            return X + ", " + Y + ", " + Z;
        }

        /// <summary>
        /// Returns a normal form of this location.
        /// </summary>
        /// <returns>A valid normal location</returns>
        public Location Normalize()
        {
            double len = Length();
            if (len == 0)
            {
                return Location.Zero;
            }
            return new Location(X / len, Y / len, Z / len);
        }

        /// <summary>
        /// Returns the cross product of this location with another.
        /// </summary>
        /// <param name="two">The second location vector</param>
        /// <returns>The cross product of the two</returns>
        public Location CrossProduct(Location two)
        {
            return new Location(Y * two.Z - two.Y * Z, two.X * Z - X * two.Z, X * two.Y - Y * two.X);
        }

        /// <summary>
        /// Reflect a location vector against a normal.
        /// </summary>
        /// <param name="normal">The normal vector</param>
        /// <returns>The reflected vector</returns>
        public Location Reflect(Location normal)
        {
            return this - (2 * this.Dot(normal) * normal);
        }

        /// <summary>
        /// Converts the Location to a simple byte[] representation.
        /// Contains 12 bytes.
        /// Inverts .FromBytes()
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] toret = new byte[12];
            BitConverter.GetBytes((float)X).CopyTo(toret, 0);
            BitConverter.GetBytes((float)Y).CopyTo(toret, 4);
            BitConverter.GetBytes((float)Z).CopyTo(toret, 8);
            return toret;
        }

        /// <summary>
        /// Returns a copy of this location.
        /// </summary>
        /// <returns>A copy of the location</returns>
        public Location Duplicate()
        {
            return new Location(X, Y, Z);
        }

        public override bool Equals(object obj)
        {
            return (obj is Location) && this == (Location)obj;
        }

        public bool Equals(Location v)
        {
            return this == v;
        }

        public static bool operator ==(Location v1, Location v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static bool operator !=(Location v1, Location v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

        public override int GetHashCode()
        {
            return (int)(X + Y + Z);
        }

        public static Location operator +(Location v1, Location v2)
        {
            return new Location(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Location operator -(Location v)
        {
            return new Location(-v.X, -v.Y, -v.Z);
        }

        public static Location operator -(Location v1, Location v2)
        {
            return new Location(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Location operator *(Location v1, Location v2)
        {
            return new Location(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static Location operator /(Location v1, Location v2)
        {
            return new Location(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        public static Location operator *(Location v, float scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator *(float scale, Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator /(Location v, float scale)
        {
            return new Location(v.X / scale, v.Y / scale, v.Z / scale);
        }

        public static Location operator *(Location v, double scale)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator *(double scale, Location v)
        {
            return new Location(v.X * scale, v.Y * scale, v.Z * scale);
        }

        public static Location operator /(Location v, double scale)
        {
            return new Location(v.X / scale, v.Y / scale, v.Z / scale);
        }

        /// <summary>
        /// Converts a string representation of a location to a Location object.
        /// Inverts .ToString(), .ToSimpleString()
        /// </summary>
        /// <param name="input">The location string</param>
        /// <returns>the location object</returns>
        public static Location FromString(string input)
        {
            string[] data = input.Replace('(', ' ').Replace(')', ' ').Replace(" ", "").Split(',');
            if (data.Length != 3)
            {
                return new Location(0);
            }
            return new Location(Utilities.StringToFloat(data[0]), Utilities.StringToFloat(data[1]), Utilities.StringToFloat(data[2]));
        }

        /// <summary>
        /// Reads the byte array to a Location object.
        /// Expects 12 bytes.
        /// Inverts .ToBytes()
        /// </summary>
        /// <param name="bytes">The bytes to read</param>
        /// <param name="index">The index to start at</param>
        /// <returns>the location object</returns>
        public static Location FromBytes(byte[] bytes, int index)
        {
            if (bytes.Length - index < 12)
            {
                return new Location(0);
            }
            float X = BitConverter.ToSingle(bytes, index);
            float Y = BitConverter.ToSingle(bytes, index + 4);
            float Z = BitConverter.ToSingle(bytes, index + 8);
            return new Location(X, Y, Z);
        }

        /// <summary>
        /// Converts the location to an OpenTK Vector3.
        /// </summary>
        /// <returns>The created vector</returns>
        public OpenTK.Vector3 ToOVector()
        {
            return new OpenTK.Vector3((float)X, (float)Y, (float)Z);
        }

        public BEPUutilities.Vector3 ToBVector()
        {
            return new BEPUutilities.Vector3((float)X, (float)Y, (float)Z);
        }

        public static Location FromBVector(BEPUutilities.Vector3 vec)
        {
            return new Location(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// Gets the location of the block this location is within.
        /// </summary>
        /// <returns>The block location</returns>
        public Location GetBlockLocation()
        {
            return new Location(Math.Floor(X), Math.Floor(Y), Math.Floor(Z));
        }

        public bool IsCloseTo(Location two, double delta)
        {
            return X + delta > two.X && X - delta < two.X
                && Y + delta > two.Y && Y - delta < two.Y
                && Z + delta > two.Z && Z - delta < two.Z;
        }
    }
}
