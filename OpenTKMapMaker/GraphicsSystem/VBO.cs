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
    public class VBO
    {
        uint _VertexVBO;
        uint _IndexVBO;
        uint _NormalVBO;
        uint _TexCoordVBO;
        uint _ColorVBO;
        public uint _VAO;

        public Texture Tex;

        public List<Vector3> Vertices;
        public List<ushort> Indices;
        public List<Vector3> Normals;
        public List<Vector3> TexCoords;
        public List<Vector4> Colors;

        public void AddSide(Location normal)
        {
            // TODO: IMPROVE!
            for (int i = 0; i < 6; i++)
            {
                Normals.Add(normal.ToOVector());
                Colors.Add(new Vector4(1f, 1f, 1f, 1f));
                Indices.Add((ushort)Indices.Count);
            }
            if (normal.Z == 1)
            {
                // T1
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 1));
                TexCoords.Add(new Vector3(0, 1, 0));
                Vertices.Add(new Vector3(0, 1, 1));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 1));
                // T2
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 1));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 1));
                TexCoords.Add(new Vector3(1, 0, 0));
                Vertices.Add(new Vector3(1, 0, 1));
            }
            else if (normal.Z == -1)
            {
                // T1
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 0));
                TexCoords.Add(new Vector3(0, 1, 0));
                Vertices.Add(new Vector3(0, 1, 0));
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 0));
                // T2
                TexCoords.Add(new Vector3(1, 0, 0));
                Vertices.Add(new Vector3(1, 0, 0));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 0));
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 0));
            }
            else if (normal.X == 1)
            {
                // T1
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 1));
                TexCoords.Add(new Vector3(0, 1, 0));
                Vertices.Add(new Vector3(1, 1, 0));
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(1, 0, 0));
                // T2
                TexCoords.Add(new Vector3(1, 0, 0));
                Vertices.Add(new Vector3(1, 0, 1));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 1));
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(1, 0, 0));
            }
            else if (normal.X == -1)
            {
                // T1
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 0));
                TexCoords.Add(new Vector3(0, 1, 0));
                Vertices.Add(new Vector3(0, 1, 0));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(0, 1, 1));
                // T2
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 0));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(0, 1, 1));
                TexCoords.Add(new Vector3(1, 0, 0));
                Vertices.Add(new Vector3(0, 0, 1));
            }
            else if (normal.Y == 1)
            {
                // T1
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 1));
                TexCoords.Add(new Vector3(0, 1, 0));
                Vertices.Add(new Vector3(0, 1, 1));
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 1, 0));
                // T2
                TexCoords.Add(new Vector3(1, 0, 0));
                Vertices.Add(new Vector3(1, 1, 0));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 1, 1));
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 1, 0));
            }
            else if (normal.Y == -1)
            {
                // T1
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 0));
                TexCoords.Add(new Vector3(0, 1, 0));
                Vertices.Add(new Vector3(0, 0, 1));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 0, 1));
                // T2
                TexCoords.Add(new Vector3(0, 0, 0));
                Vertices.Add(new Vector3(0, 0, 0));
                TexCoords.Add(new Vector3(1, 1, 0));
                Vertices.Add(new Vector3(1, 0, 1));
                TexCoords.Add(new Vector3(1, 0, 0));
                Vertices.Add(new Vector3(1, 0, 0));
            }
            else
            {
                throw new Exception("Lazy code can't handle unique normals! Only axis-aligned, normalized normals!");
            }
        }

        public void Prepare()
        {
            Vertices = new List<Vector3>();
            Indices = new List<ushort>();
            Normals = new List<Vector3>();
            TexCoords = new List<Vector3>();
            Colors = new List<Vector4>();
        }

        bool generated = false;

        public void Destroy()
        {
            if (generated)
            {
                GL.DeleteVertexArray(_VAO);
                GL.DeleteBuffer(_VertexVBO);
                GL.DeleteBuffer(_IndexVBO);
                GL.DeleteBuffer(_NormalVBO);
                GL.DeleteBuffer(_TexCoordVBO);
                GL.DeleteBuffer(_ColorVBO);
            }
        }

        public void GenerateVBO()
        {
            if (generated)
            {
                Destroy();
            }
            if (Vertices.Count == 0)
            {
                return;
            }
            Vector3[] vecs = Vertices.ToArray();
            ushort[] inds = Indices.ToArray();
            Vector3[] norms = Normals.ToArray();
            Vector3[] texs = TexCoords.ToArray();
            Vector4[] cols = Colors.ToArray();
            // Vertex buffer
            GL.GenBuffers(1, out _VertexVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes),
                    vecs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Normal buffer
            GL.GenBuffers(1, out _NormalVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes),
                    norms, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // TexCoord buffer
            GL.GenBuffers(1, out _TexCoordVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TexCoordVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes),
                    texs, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Color buffer
            GL.GenBuffers(1, out _ColorVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes),
                    cols, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.GenBuffers(1, out _IndexVBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(ushort)),
                    inds, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.GenVertexArrays(1, out _VAO);
            GL.BindVertexArray(_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexVBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalVBO);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TexCoordVBO);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorVBO);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexVBO);
            // Clean up
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            generated = true;
        }

        public void Render()
        {
            if (!generated)
            {
                return;
            }
            if (Tex != null)
            {
                Tex.Bind();
            }
            GL.BindVertexArray(_VAO);
            GL.DrawElements(PrimitiveType.Triangles, Vertices.Count, DrawElementsType.UnsignedShort, IntPtr.Zero);
            GL.BindVertexArray(0);
        }
    }
}
