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
    public class PointLight: LightObject
    {
        int Texsize;

        float Radius;

        Location Color;

        public PointLight(Location pos, int tsize, float radius, Location col)
        {
            EyePos = pos;
            Texsize = tsize;
            Radius = radius;
            Color = col;
            for (int i = 0; i < 6; i++)
            {
                InternalLights.Add(new Light());
                InternalLights[i].Create(Texsize, pos.ToOVector(), (pos + Location.UnitX).ToOVector(), 90f, Radius, Color.ToOVector());
            }
            InternalLights[4].up = new Vector3(0, 1, 0);
            InternalLights[5].up = new Vector3(0, 1, 0);
            Reposition(EyePos);
        }

        public void Destroy()
        {
            for (int i = 0; i < InternalLights.Count; i++)
            {
                InternalLights[i].Destroy();
            }
        }

        public override void Reposition(Location pos)
        {
            EyePos = pos;
            for (int i = 0; i < 6; i++)
            {
                InternalLights[i].NeedsUpdate = true;
                InternalLights[i].eye = EyePos.ToOVector();
            }
            InternalLights[0].target = (EyePos + new Location(1, 0, 0)).ToOVector();
            InternalLights[1].target = (EyePos + new Location(-1, 0, 0)).ToOVector();
            InternalLights[2].target = (EyePos + new Location(0, 1, 0)).ToOVector();
            InternalLights[3].target = (EyePos + new Location(0, -1, 0)).ToOVector();
            InternalLights[4].target = (EyePos + new Location(0, 0, 1)).ToOVector();
            InternalLights[5].target = (EyePos + new Location(0, 0, -1)).ToOVector();
        }
    }
}
