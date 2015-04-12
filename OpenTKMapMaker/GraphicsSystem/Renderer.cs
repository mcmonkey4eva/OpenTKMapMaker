using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.GraphicsSystem
{
    /// <summary>
    /// Rendering utility.
    /// 
    /// TODO: Make the objects in this class more abstract!
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// Prepare the renderer.
        /// </summary>
        public void Init()
        {
            GenerateBoxVBO();
            GenerateSquareVBO();
            GenerateLineVBO();
        }

        VBO Square;
        VBO Line;
        VBO Box;

        void GenerateSquareVBO()
        {
            Vector3[] vecs = new Vector3[4];
            ushort[] inds = new ushort[4];
            Vector3[] norms = new Vector3[4];
            Vector3[] texs = new Vector3[4];
            Vector4[] cols = new Vector4[4];
            for (ushort u = 0; u < 4; u++)
            {
                inds[u] = u;
            }
            for (int n = 0; n < 4; n++)
            {
                norms[n] = new Vector3(0, 0, 1);
            }
            for (int c = 0; c < 4; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            vecs[0] = new Vector3(1, 0, 0);
            texs[0] = new Vector3(1, 0, 0);
            vecs[1] = new Vector3(1, 1, 0);
            texs[1] = new Vector3(1, 1, 0);
            vecs[2] = new Vector3(0, 1, 0);
            texs[2] = new Vector3(0, 1, 0);
            vecs[3] = new Vector3(0, 0, 0);
            texs[3] = new Vector3(0, 0, 0);
            Square = new VBO();
            Square.Vertices = vecs.ToList();
            Square.Indices = inds.ToList();
            Square.Normals = norms.ToList();
            Square.TexCoords = texs.ToList();
            Square.Colors = cols.ToList();
            Square.GenerateVBO();
        }

        void GenerateLineVBO()
        {
            Vector3[] vecs = new Vector3[2];
            ushort[] inds = new ushort[2];
            Vector3[] norms = new Vector3[2];
            Vector3[] texs = new Vector3[2];
            Vector4[] cols = new Vector4[2];
            for (ushort u = 0; u < 2; u++)
            {
                inds[u] = u;
            }
            for (int n = 0; n < 2; n++)
            {
                norms[n] = new Vector3(0, 0, 1);
            }
            for (int c = 0; c < 2; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            vecs[0] = new Vector3(0, 0, 0);
            texs[0] = new Vector3(0, 0, 0);
            vecs[1] = new Vector3(1, 0, 0);
            texs[1] = new Vector3(1, 0, 0);
            Line = new VBO();
            Line.Vertices = vecs.ToList();
            Line.Indices = inds.ToList();
            Line.Normals = norms.ToList();
            Line.TexCoords = texs.ToList();
            Line.Colors = cols.ToList();
            Line.GenerateVBO();
        }

        void GenerateBoxVBO()
        {
            // TODO: Optimize?
            Vector3[] vecs = new Vector3[24];
            ushort[] inds = new ushort[24];
            Vector3[] norms = new Vector3[24];
            Vector3[] texs = new Vector3[24];
            Vector4[] cols = new Vector4[24];
            for (ushort u = 0; u < 24; u++)
            {
                inds[u] = u;
            }
            for (int t = 0; t < 24; t++)
            {
                texs[t] = new Vector3(0, 0, 0);
            }
            for (int n = 0; n < 24; n++)
            {
                norms[n] = new Vector3(0, 0, 1); // TODO: Accurate normals somehow? Do lines even have normals?
            }
            for (int c = 0; c < 24; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            int i = 0;
            vecs[i] = new Vector3(0, 0, 0); i++;
            vecs[i] = new Vector3(1, 0, 0); i++;
            vecs[i] = new Vector3(1, 0, 0); i++;
            vecs[i] = new Vector3(1, 1, 0); i++;
            vecs[i] = new Vector3(1, 1, 0); i++;
            vecs[i] = new Vector3(0, 1, 0); i++;
            vecs[i] = new Vector3(0, 1, 0); i++;
            vecs[i] = new Vector3(0, 0, 0); i++;
            vecs[i] = new Vector3(0, 0, 1); i++;
            vecs[i] = new Vector3(1, 0, 1); i++;
            vecs[i] = new Vector3(1, 0, 1); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(0, 1, 1); i++;
            vecs[i] = new Vector3(0, 1, 1); i++;
            vecs[i] = new Vector3(0, 0, 1); i++;
            vecs[i] = new Vector3(0, 0, 0); i++;
            vecs[i] = new Vector3(0, 0, 1); i++;
            vecs[i] = new Vector3(1, 0, 0); i++;
            vecs[i] = new Vector3(1, 0, 1); i++;
            vecs[i] = new Vector3(1, 1, 0); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(0, 1, 0); i++;
            vecs[i] = new Vector3(0, 1, 1); i++;
            Box = new VBO();
            Box.Vertices = vecs.ToList();
            Box.Indices = inds.ToList();
            Box.Normals = norms.ToList();
            Box.TexCoords = texs.ToList();
            Box.Colors = cols.ToList();
            Box.GenerateVBO();
        }

        public Renderer(TextureEngine tengine)
        {
            Engine = tengine;
        }

        public TextureEngine Engine;

        /// <summary>
        /// Renders a black line box.
        /// </summary>
        public void RenderLineBox(Location min, Location max)
        {
            Engine.White.Bind();
            Matrix4 mat = Matrix4.CreateScale((max - min).ToOVector()) * Matrix4.CreateTranslation(min.ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Box._VAO);
            GL.DrawElements(PrimitiveType.Lines, 24, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }

        /// <summary>
        /// Render a line between two points.
        /// </summary>
        /// <param name="start">The initial point</param>
        /// <param name="end">The ending point</param>
        public void RenderLine(Location start, Location end)
        {
            float len = (float)(end - start).Length();
            Location vecang = Utilities.VectorToAngles(start - end);
            Matrix4 mat = Matrix4.CreateScale(len, len, len) * Matrix4.CreateRotationZ((float)(vecang.X * Utilities.PI180))
                * Matrix4.CreateRotationY((float)(-vecang.Y * Utilities.PI180)) * Matrix4.CreateTranslation(start.ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Line._VAO);
            GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }

        public void SetColor(Color4 c)
        {
            Vector3 col = new Vector3(c.R, c.G, c.B);
            GL.Uniform3(3, ref col);
        }

        public void SetMinimumLight(float min)
        {
            if (PrimaryEditor.RenderLights)
            {
                SysConsole.Output(OutputType.INFO, "Rendering light at a minimum strength of " + min);
                GL.Uniform1(5, min);
            }
        }

        /// <summary>
        /// Renders a 2D rectangle.
        /// </summary>
        /// <param name="xmin">The lower bounds of the the rectangle: X coordinate</param>
        /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate</param>
        /// <param name="xmax">The upper bounds of the the rectangle: X coordinate</param>
        /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate</param>
        public void RenderRectangle(int xmin, int ymin, int xmax, int ymax)
        {
            Matrix4 mat = Matrix4.CreateScale(xmax - xmin, ymax - ymin, 1) * Matrix4.CreateTranslation(xmin, ymin, 0);
            GL.UniformMatrix4(2, false, ref mat);

            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Quads, 4, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }
    }
}
