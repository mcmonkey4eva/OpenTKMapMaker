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
        public bool NeedsUpdate = true;

        public void Create(int tsize, Vector3 pos, Vector3 targ, float fov, float max_range, Vector3 col)
        {
            PrimaryEditor.ContextView.Control.MakeCurrent();
            texsize = tsize;
            eye = pos;
            target = targ;
            FOV = fov;
            maxrange = max_range;
            color = col;
            // FBO
            fbo_main = GL.GenFramebuffer();
            // Build the texture
            fbo_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texsize, texsize, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            // Attach it to the FBO
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_texture, 0);
            // Build the depth texture
            fbo_depthtex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fbo_depthtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, texsize, texsize, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)DepthFunction.Lequal);
            // Attach it to the FBO
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, fbo_depthtex, 0);
            // Wrap up
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Attach()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.Viewport(0, 0, texsize, texsize);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
            PrimaryEditor.vpw = texsize;
            PrimaryEditor.vph = texsize;
            Matrix4 mat = GetMatrix();
            GL.UniformMatrix4(1, false, ref mat);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
        }

        public void Complete()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
        }

        public Matrix4 GetMatrix()
        {
            return Matrix4.LookAt(eye, target, up) *
                Matrix4.CreatePerspectiveFieldOfView(FOV * (float)Math.PI / 180f, 1, 0.1f, maxrange);
        }
    }
}
