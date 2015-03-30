using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTKMapMaker.Utility
{
    /// <summary>
    /// Helper code for tracing collision.
    /// </summary>
    public class CollisionUtil
    {
        /// <summary>
        /// Returns whether a box contains (intersects with) another box.
        /// </summary>
        /// <param name="elow">The low point for box 1</param>
        /// <param name="ehigh">The high point for box 1</param>
        /// <param name="Low">The low point for box 2</param>
        /// <param name="High">The high point for box 2</param>
        /// <returns>whether there is intersection</returns>
        public static bool BoxContainsBox(Location elow, Location ehigh, Location Low, Location High)
        {
            return Low.X <= ehigh.X && Low.Y <= ehigh.Y && Low.Z <= ehigh.Z &&
            High.X >= elow.X && High.Y >= elow.Y && High.Z >= elow.Z;
        }

        /// <summary>
        /// Returns whether a box contains a point.
        /// </summary>
        /// <param name="elow">The low point for the box</param>
        /// <param name="ehigh">The high point for the box</param>
        /// <param name="point">The point to check</param>
        /// <returns>whether there is intersection</returns>
        public static bool BoxContainsPoint(Location elow, Location ehigh, Location point)
        {
            return point.X <= ehigh.X && point.Y <= ehigh.Y && point.Z <= ehigh.Z &&
            point.X >= elow.X && point.Y >= elow.Y && point.Z >= elow.Z;
        }

        /// <summary>
        /// Runs a collision check between two AABB objects.
        /// </summary>
        /// <param name="Position">The block's position</param>
        /// <param name="Mins">The block's mins</param>
        /// <param name="Maxs">The block's maxs</param>
        /// <param name="Mins2">The moving object's mins</param>
        /// <param name="Maxs2">The moving object's maxs</param>
        /// <param name="start">The starting location of the moving object</param>
        /// <param name="end">The ending location of the moving object</param>
        /// <param name="normal">The normal of the hit, or NaN if none</param>
        /// <returns>The location of the hit, or NaN if none</returns>
        public static Location AABBClosestBox(Location Position, Location Mins, Location Maxs, Location Mins2, Location Maxs2, Location start, Location end, out Location normal)
        {
            Location velocity = end - start;
            Location RealMins = Position + Mins;
            Location RealMaxs = Position + Maxs;
            Location RealMins2 = start + Mins2;
            Location RealMaxs2 = start + Maxs2;
            double xInvEntry, yInvEntry, zInvEntry;
            double xInvExit, yInvExit, zInvExit;
            if (end.X >= start.X)
            {
                xInvEntry = RealMins.X - RealMaxs2.X;
                xInvExit = RealMaxs.X - RealMins2.X;
            }
            else
            {
                xInvEntry = RealMaxs.X - RealMins2.X;
                xInvExit = RealMins.X - RealMaxs2.X;
            }
            if (end.Y >= start.Y)
            {
                yInvEntry = RealMins.Y - RealMaxs2.Y;
                yInvExit = RealMaxs.Y - RealMins2.Y;
            }
            else
            {
                yInvEntry = RealMaxs.Y - RealMins2.Y;
                yInvExit = RealMins.Y - RealMaxs2.Y;
            }
            if (end.Z >= start.Z)
            {
                zInvEntry = RealMins.Z - RealMaxs2.Z;
                zInvExit = RealMaxs.Z - RealMins2.Z;
            }
            else
            {
                zInvEntry = RealMaxs.Z - RealMins2.Z;
                zInvExit = RealMins.Z - RealMaxs2.Z;
            }
            double xEntry, yEntry, zEntry;
            double xExit, yExit, zExit;
            if (velocity.X == 0f)
            {
                xEntry = xInvEntry / 0.00000000000000000000000000000001f;
                xExit = xInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                xEntry = xInvEntry / velocity.X;
                xExit = xInvExit / velocity.X;
            }
            if (velocity.Y == 0f)
            {
                yEntry = yInvEntry / 0.00000000000000000000000000000001f;
                yExit = yInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                yEntry = yInvEntry / velocity.Y;
                yExit = yInvExit / velocity.Y;
            }
            if (velocity.Z == 0f)
            {
                zEntry = zInvEntry / 0.00000000000000000000000000000001f;
                zExit = zInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                zEntry = zInvEntry / velocity.Z;
                zExit = zInvExit / velocity.Z;
            }
            double entryTime = Math.Max(Math.Max(xEntry, yEntry), zEntry);
            double exitTime = Math.Min(Math.Min(xExit, yExit), zExit);
            if (entryTime > exitTime || (xEntry < 0.0f && yEntry < 0.0f && zEntry < 0.0f) || xEntry > 1.0f || yEntry > 1.0f || zEntry > 1.0f)
            {
                normal = Location.NaN;
                return Location.NaN;
            }
            else
            {
                if (zEntry >= xEntry && zEntry >= yEntry)
                {
                    if (zInvEntry < 0)
                    {
                        normal = new Location(0, 0, 1);
                    }
                    else
                    {
                        normal = new Location(0, 0, -1);
                    }
                }
                else if (xEntry >= zEntry && xEntry >= yEntry)
                {
                    if (xInvEntry < 0)
                    {
                        normal = new Location(1, 0, 0);
                    }
                    else
                    {
                        normal = new Location(-1, 0, 0);
                    }
                }
                else
                {
                    if (yInvEntry < 0)
                    {
                        normal = new Location(0, 1, 0);
                    }
                    else
                    {
                        normal = new Location(0, -1, 0);
                    }
                }
                Location res = start + (end - start) * entryTime;
                return new Location(res.X, res.Y, res.Z);
            }
        }



        /// <summary>
        /// Runs a collision check between an AABB and a ray.
        /// </summary>
        /// <param name="Position">The block's position</param>
        /// <param name="Mins">The block's mins</param>
        /// <param name="Maxs">The block's maxs</param>
        /// <param name="start">The starting location of the ray</param>
        /// <param name="end">The ending location of the ray</param>
        /// <param name="normal">The normal of the hit, or NaN if none</param>
        /// <returns>The location of the hit, or NaN if none</returns>
        public static Location RayTraceBox(Location Position, Location Mins, Location Maxs, Location start, Location end, out Location normal)
        {
            Location velocity = end - start;
            Location RealMins = Position + Mins;
            Location RealMaxs = Position + Maxs;
            double xInvEntry, yInvEntry, zInvEntry;
            double xInvExit, yInvExit, zInvExit;
            if (end.X >= start.X)
            {
                xInvEntry = RealMins.X - start.X;
                xInvExit = RealMaxs.X - start.X;
            }
            else
            {
                xInvEntry = RealMaxs.X - start.X;
                xInvExit = RealMins.X - start.X;
            }
            if (end.Y >= start.Y)
            {
                yInvEntry = RealMins.Y - start.Y;
                yInvExit = RealMaxs.Y - start.Y;
            }
            else
            {
                yInvEntry = RealMaxs.Y - start.Y;
                yInvExit = RealMins.Y - start.Y;
            }
            if (end.Z >= start.Z)
            {
                zInvEntry = RealMins.Z - start.Z;
                zInvExit = RealMaxs.Z - start.Z;
            }
            else
            {
                zInvEntry = RealMaxs.Z - start.Z;
                zInvExit = RealMins.Z - start.Z;
            }
            double xEntry, yEntry, zEntry;
            double xExit, yExit, zExit;
            if (velocity.X == 0f)
            {
                xEntry = xInvEntry / 0.00000000000000000000000000000001f;
                xExit = xInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                xEntry = xInvEntry / velocity.X;
                xExit = xInvExit / velocity.X;
            }
            if (velocity.Y == 0f)
            {
                yEntry = yInvEntry / 0.00000000000000000000000000000001f;
                yExit = yInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                yEntry = yInvEntry / velocity.Y;
                yExit = yInvExit / velocity.Y;
            }
            if (velocity.Z == 0f)
            {
                zEntry = zInvEntry / 0.00000000000000000000000000000001f;
                zExit = zInvExit / 0.00000000000000000000000000000001f;
            }
            else
            {
                zEntry = zInvEntry / velocity.Z;
                zExit = zInvExit / velocity.Z;
            }
            double entryTime = Math.Max(Math.Max(xEntry, yEntry), zEntry);
            double exitTime = Math.Min(Math.Min(xExit, yExit), zExit);
            if (entryTime > exitTime || (xEntry < 0.0f && yEntry < 0.0f && zEntry < 0.0f) || xEntry > 1.0f || yEntry > 1.0f || zEntry > 1.0f)
            {
                normal = Location.NaN;
                return Location.NaN;
            }
            else
            {
                if (zEntry >= xEntry && zEntry >= yEntry)
                {
                    if (zInvEntry < 0)
                    {
                        normal = new Location(0, 0, 1);
                    }
                    else
                    {
                        normal = new Location(0, 0, -1);
                    }
                }
                else if (xEntry >= zEntry && xEntry >= yEntry)
                {
                    if (xInvEntry < 0)
                    {
                        normal = new Location(1, 0, 0);
                    }
                    else
                    {
                        normal = new Location(-1, 0, 0);
                    }
                }
                else
                {
                    if (yInvEntry < 0)
                    {
                        normal = new Location(0, 1, 0);
                    }
                    else
                    {
                        normal = new Location(0, -1, 0);
                    }
                }
                Location res = start + (end - start) * entryTime;
                return new Location(res.X, res.Y, res.Z);
            }
        }

        /// <summary>
        /// Gets the lowest point of two points.
        /// </summary>
        /// <param name="one">The first point</param>
        /// <param name="two">The second point</param>
        /// <returns>The lowest point</returns>
        public static Location GetLow(Location one, Location two)
        {
            return new Location(one.X < two.X ? one.X : two.X,
            one.Y < two.Y ? one.Y : two.Y,
            one.Z < two.Z ? one.Z : two.Z);
        }

        /// <summary>
        /// Gets the highest point of two points.
        /// </summary>
        /// <param name="one">The first point</param>
        /// <param name="two">The second point</param>
        /// <returns>The highest point</returns>
        public static Location GetHigh(Location one, Location two)
        {
            return new Location(one.X > two.X ? one.X : two.X,
            one.Y > two.Y ? one.Y : two.Y,
            one.Z > two.Z ? one.Z : two.Z);
        }
    }
}
