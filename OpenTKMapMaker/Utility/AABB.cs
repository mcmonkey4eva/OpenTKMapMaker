using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTKMapMaker.Utility
{
    public class AABB
    {
        public Location Min;
        public Location Max;

        public void Include(Location pos)
        {
            if (pos.X < Min.X)
            {
                Min.X = pos.X;
            }
            if (pos.Y < Min.Y)
            {
                Min.Y = pos.Y;
            }
            if (pos.Z < Min.Z)
            {
                Min.Z = pos.Z;
            }
            if (pos.X > Max.X)
            {
                Max.X = pos.X;
            }
            if (pos.Y > Max.Y)
            {
                Max.Y = pos.Y;
            }
            if (pos.Z > Max.Z)
            {
                Max.Z = pos.Z;
            }
        }
    }
}
