using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKMapMaker.GraphicsSystem.LightingSystem
{
    public class Light
    {
        public Vector3 eye;
        public Vector3 target;
        public Vector3 up = Vector3.UnitZ;
        public float FOV;
        public float maxrange;
        public Vector3 color;
        public int texsize;
        public int fbo_main;
        public int fbo_texture;
        public int fbo_depthtex;

        public void Create(int tsize, Vector3 pos, Vector3 targ, float fov, float max_range, Vector3 col)
        {
            texsize = tsize;
            eye = pos;
            target = targ;
            FOV = fov;
            maxrange = max_range;
            color = col;
            // TODO: Generate FBO
        }

        public void Attach()
        {
            // TODO: Attach FBO, etc.
        }

        public void Complete()
        {
            // TODO: Unattach FBO, etc.
        }

        public Matrix4 GetMatrix()
        {
            return Matrix4.Identity; // TODO: Calculate real matrix
        }
    }
}
