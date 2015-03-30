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

        uint box_VBO;
        uint box_IndexVBO;
        uint box_NormalVBO;
        uint box_TexCoordVBO;
        uint box_ColorVBO;
        uint box_VAO;

        uint square_VBO;
        uint square_IndexVBO;
        uint square_NormalVBO;
        uint square_TexCoordVBO;
        uint square_ColorVBO;
        uint square_VAO;

        uint line_VBO;
        uint line_IndexVBO;
        uint line_NormalVBO;
        uint line_TexCoordVBO;
        uint line_ColorVBO;
        uint line_VAO;

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
            // Vertex buffer
            GL.GenBuffers(1, out square_VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes),
                    vecs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Normal buffer
            GL.GenBuffers(1, out square_NormalVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_NormalVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes),
                    norms, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // TexCoord buffer
            GL.GenBuffers(1, out square_TexCoordVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_TexCoordVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes),
                    texs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Color buffer
            GL.GenBuffers(1, out square_ColorVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_ColorVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes),
                    cols, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.GenBuffers(1, out square_IndexVBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, square_IndexVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(ushort)),
                    inds, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.GenVertexArrays(1, out square_VAO);
            GL.BindVertexArray(square_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_NormalVBO);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_TexCoordVBO);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, square_ColorVBO);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, square_IndexVBO);
            // Clean up
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
            // Vertex buffer
            GL.GenBuffers(1, out line_VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes),
                    vecs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Normal buffer
            GL.GenBuffers(1, out line_NormalVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_NormalVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes),
                    norms, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // TexCoord buffer
            GL.GenBuffers(1, out line_TexCoordVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_TexCoordVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes),
                    texs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Color buffer
            GL.GenBuffers(1, out line_ColorVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_ColorVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes),
                    cols, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.GenBuffers(1, out line_IndexVBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, line_IndexVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(ushort)),
                    inds, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.GenVertexArrays(1, out line_VAO);
            GL.BindVertexArray(line_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_NormalVBO);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_TexCoordVBO);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, line_ColorVBO);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, line_IndexVBO);
            // Clean up
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
            // Vertex buffer
            GL.GenBuffers(1, out box_VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes),
                    vecs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Normal buffer
            GL.GenBuffers(1, out box_NormalVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_NormalVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes),
                    norms, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // TexCoord buffer
            GL.GenBuffers(1, out box_TexCoordVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_TexCoordVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes),
                    texs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Color buffer
            GL.GenBuffers(1, out box_ColorVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_ColorVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes),
                    cols, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.GenBuffers(1, out box_IndexVBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, box_IndexVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(ushort)),
                    inds, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.GenVertexArrays(1, out box_VAO);
            GL.BindVertexArray(box_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_NormalVBO);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_TexCoordVBO);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_ColorVBO);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, box_IndexVBO);
            // Clean up
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
            GL.BindVertexArray(box_VAO);
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
            GL.BindVertexArray(line_VAO);
            GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }

        public void SetColor(Color4 c)
        {
            Vector3 col = new Vector3(c.R, c.G, c.B);
            GL.Uniform3(3, ref col);
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

            GL.BindVertexArray(square_VAO);
            GL.DrawElements(PrimitiveType.Quads, 4, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }
    }
}
