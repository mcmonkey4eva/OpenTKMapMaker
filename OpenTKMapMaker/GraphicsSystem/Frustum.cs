using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;
using OpenTK;

namespace OpenTKMapMaker.GraphicsSystem
{
    /// <summary>
    /// Represents a 3D Frustum.
    /// </summary>
    public class Frustum
    {
        Plane Near;

        Plane Far;

        Plane Left;

        Plane Right;

        Plane Top;

        Plane Bottom;

        public Frustum(Matrix4 matrix)
        {
            Left = new Plane(new Location(-matrix.M14 - matrix.M11, -matrix.M24 - matrix.M21, -matrix.M34 - matrix.M31), -matrix.M44 - matrix.M41);
            Right = new Plane(new Location(matrix.M11 - matrix.M14, matrix.M21 - matrix.M24, matrix.M31 - matrix.M34), matrix.M41 - matrix.M44);
            Top = new Plane(new Location(matrix.M12 - matrix.M14, matrix.M22 - matrix.M24, matrix.M32 - matrix.M34), matrix.M42 - matrix.M44);
            Bottom = new Plane(new Location(-matrix.M14 - matrix.M12, -matrix.M24 - matrix.M22, -matrix.M34 - matrix.M32), -matrix.M44 - matrix.M42);
            Near = new Plane(new Location(-matrix.M13, -matrix.M23, -matrix.M33), -matrix.M43);
            Far = new Plane(new Location(matrix.M13 - matrix.M14, matrix.M23 - matrix.M24, matrix.M33 - matrix.M34), matrix.M43 - matrix.M44);
        }

        /// <summary>
        /// Returns whether an AABB is contained by the Frustum.
        /// </summary>
        /// <param name="min">The lower coord of the AABB</param>
        /// <param name="max">The higher coord of the AABB</param>
        /// <returns>Whether it is contained</returns>
        public bool ContainsBox(Location min, Location max)
        { // TODO: Improve accuracy
            if (min == max)
            {
                return Contains(min);
            }
            Location[] locs = new Location[] {
                min, max, new Location(min.X, min.Y, max.Z),
                new Location(min.X, max.Y, max.Z),
                new Location(max.X, min.Y, max.Z),
                new Location(max.X, min.Y, min.Z),
                new Location(max.X, max.Y, min.Z),
                new Location(min.X, max.Y, min.Z)
            };
            for (int p = 0; p < 6; p++)
            {
                Plane pl = GetFor(p);
                int inC = 8;
                for (int i = 0; i < 8; i++)
                {
                    if (Math.Sign(pl.Distance(locs[i])) == 1)
                    {
                        inC--;
                    }
                }

                if (inC == 0)
                {
                    return false;
                }
            }
            return true;
            /*
            bool any = false;
            if (Contains(min)) { any = true; }
            else if (Contains(max)) { any = true; }
            else if (Contains(new Location(min.X, min.Y, max.Z))) { any = true; }
            else if (Contains(new Location(min.X, max.Y, max.Z))) { any = true; }
            else if (Contains(new Location(max.X, min.Y, max.Z))) { any = true; }
            else if (Contains(new Location(max.X, min.Y, min.Z))) { any = true; }
            else if (Contains(new Location(max.X, max.Y, min.Z))) { any = true; }
            else if (Contains(new Location(min.X, max.Y, min.Z))) { any = true; }
            return any;
            */
        }

        /// <summary>
        /// Returns whether the frustum contains a sphere.
        /// </summary>
        /// <param name="point">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <returns>Whether it intersects</returns>
        public bool ContainsSphere(Location point, float radius)
        { // TODO: Improve accuracy
            double dist;
            for (int i = 0; i < 6; i++)
            {
                Plane pl = GetFor(i);
                dist = pl.Normal.Dot(point) + pl.D;
                if (dist < -radius)
                {
                    return false;
                }
                if (Math.Abs(dist) < radius)
                {
                    return true; // TODO: Is this part needed?
                }
            }
            return true;
        }

        public Plane GetFor(int i)
        {
            switch (i)
            {
                case 0:
                    return Far;
                case 1:
                    return Near;
                case 2:
                    return Top;
                case 3:
                    return Bottom;
                case 4:
                    return Left;
                case 5:
                    return Right;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns whether the Frustum contains a point.
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>Whether it's contained</returns>
        public bool Contains(Location point)
        {
            double rel = TryPoint(point, Far);
            if (rel > 0) { return false; }
            rel = TryPoint(point, Near);
            if (rel > 0) { return false; }
            rel = TryPoint(point, Top);
            if (rel > 0) { return false; }
            rel = TryPoint(point, Bottom);
            if (rel > 0) { return false; }
            rel = TryPoint(point, Left);
            if (rel > 0) { return false; }
            rel = TryPoint(point, Right);
            if (rel > 0) { return false; }
            return true;
        }

        double TryPoint(Location point, Plane plane)
        {
            return point.X * plane.Normal.X + point.Y * plane.Normal.Y + point.Z * plane.Normal.Z + plane.D;
        }
    }
}
