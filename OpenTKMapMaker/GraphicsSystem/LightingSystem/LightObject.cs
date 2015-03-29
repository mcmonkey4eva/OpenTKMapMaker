using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.GraphicsSystem.LightingSystem
{
    public abstract class LightObject
    {
        public List<Light> InternalLights = new List<Light>();

        public Location EyePos;

        public abstract void Reposition(Location pos);
    }
}
