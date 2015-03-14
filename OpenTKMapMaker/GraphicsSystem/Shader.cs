using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKMapMaker.GraphicsSystem
{
    public class ShaderEngine
    {
        /// <summary>
        /// A full list of currently loaded shaders.
        /// </summary>
        public List<Shader> LoadedShaders;

        /// <summary>
        /// A common shader that multiplies colors.
        /// </summary>
        public Shader ColorMultShader;

        /// <summary>
        /// A common shader that removes black color.
        /// </summary>
        public Shader TextCleanerShader;

        /// <summary>
        /// Starts or restarts the shader system.
        /// </summary>
        public void InitShaderSystem()
        {
            // Dispose existing shaders
            if (LoadedShaders != null)
            {
                for (int i = 0; i < LoadedShaders.Count; i++)
                {
                    LoadedShaders[i].Remove();
                    i--;
                }
            }
            // Reset shader list
            LoadedShaders = new List<Shader>();
            // Pregenerate a few needed shader
            ColorMultShader = GetShader("color_mult");
            TextCleanerShader = GetShader("text_cleaner");
        }

        /// <summary>
        /// Gets the shader object for a specific shader name.
        /// </summary>
        /// <param name="shadername">The name of the shader</param>
        /// <returns>A valid shader object</returns>
        public Shader GetShader(string shadername)
        {
            shadername = FileHandler.CleanFileName(shadername);
            for (int i = 0; i < LoadedShaders.Count; i++)
            {
                if (LoadedShaders[i].Name == shadername)
                {
                    return LoadedShaders[i];
                }
            }
            Shader Loaded = LoadShader(shadername);
            if (Loaded == null)
            {
                Loaded = new Shader();
                Loaded.Name = shadername;
                Loaded.Internal_Program = ColorMultShader.Original_Program;
                Loaded.Original_Program = ColorMultShader.Original_Program;
                Loaded.LoadedProperly = false;
                Loaded.Engine = this;
            }
            LoadedShaders.Add(Loaded);
            return Loaded;
        }

        /// <summary>
        /// Loads a shader from file.
        /// </summary>
        /// <param name="filename">The name of the file to use</param>
        /// <returns>The loaded shader, or null if it does not exist</returns>
        public Shader LoadShader(string filename)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!FileHandler.Exists("shaders/" + filename + ".vs"))
                {
                    SysConsole.Output(OutputType.ERROR, "Cannot load shader, file '" +
                        TextStyle.Color_Standout + "shaders/" + filename + ".vs" + TextStyle.Color_Error +
                        "' does not exist.");
                    return null;
                }
                if (!FileHandler.Exists("shaders/" + filename + ".fs"))
                {
                    SysConsole.Output(OutputType.ERROR, "Cannot load shader, file '" +
                        TextStyle.Color_Standout + "shaders/" + filename + ".fs" + TextStyle.Color_Error +
                        "' does not exist.");
                    return null;
                }
                string VS = FileHandler.ReadText("shaders/" + filename + ".vs");
                string FS = FileHandler.ReadText("shaders/" + filename + ".fs");
                return CreateShader(VS, FS, filename);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load shader from filename '" +
                    TextStyle.Color_Standout + "shaders/" + filename + ".fs or .vs" + TextStyle.Color_Error + "': " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Creates a full Shader object for a VS/FS input.
        /// </summary>
        /// <param name="VS">The input VertexShader code</param>
        /// <param name="FS">The input FragmentShader code</param>
        /// <param name="name">The name of the shader</param>
        /// <returns>A valid Shader object</returns>
        public Shader CreateShader(string VS, string FS, string name)
        {
            uint Program = CompileToProgram(VS, FS);
            Shader generic = new Shader();
            generic.Name = name;
            generic.LoadedProperly = true;
            generic.Internal_Program = Program;
            generic.Original_Program = Program;
            generic.Engine = this;
            return generic;
        }

        /// <summary>
        /// Compiles a VertexShader and FragmentShader to a usable shader program.
        /// </summary>
        /// <param name="VS">The input VertexShader code</param>
        /// <param name="FS">The input FragmentShader code</param>
        /// <returns>The internal OpenGL program ID</returns>
        public uint CompileToProgram(string VS, string FS)
        {
            int VertexObject = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexObject, VS);
            GL.CompileShader(VertexObject);
            string VS_Info = GL.GetShaderInfoLog(VertexObject);
            int VS_Status = 0;
            GL.GetShader(VertexObject, ShaderParameter.CompileStatus, out VS_Status);
            if (VS_Status != 1)
            {
                throw new Exception("Error creating VertexShader. Error status: " + VS_Status + ", info: " + VS_Info);
            }
            int FragmentObject = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentObject, FS);
            GL.CompileShader(FragmentObject);
            string FS_Info = GL.GetShaderInfoLog(FragmentObject);
            int FS_Status = 0;
            GL.GetShader(FragmentObject, ShaderParameter.CompileStatus, out FS_Status);
            if (FS_Status != 1)
            {
                throw new Exception("Error creating VertexShader. Error status: " + FS_Status + ", info: " + FS_Info);
            }
            int Program = GL.CreateProgram();
            GL.AttachShader(Program, FragmentObject);
            GL.AttachShader(Program, VertexObject);
            GL.LinkProgram(Program);
            string str = GL.GetProgramInfoLog(Program);
            if (str.Length != 0)
            {
                SysConsole.Output(OutputType.INFO, "Linked shader with message: '" + str + "'");
            }
            GL.DeleteShader(FragmentObject);
            GL.DeleteShader(VertexObject);
            return (uint)Program;
        }

    }

    /// <summary>
    /// Wraps an OpenGL shader.
    /// </summary>
    public class Shader
    {
        public ShaderEngine Engine;

        /// <summary>
        /// The name of the shader
        /// </summary>
        public string Name;

        /// <summary>
        /// The shader this shader was remapped to.
        /// </summary>
        public Shader RemappedTo;

        /// <summary>
        /// The internal OpenGL ID for the shader program.
        /// </summary>
        public uint Internal_Program;

        /// <summary>
        /// The original OpenGL ID that formed this shader program.
        /// </summary>
        public uint Original_Program;

        /// <summary>
        /// Whether the shader loaded properly.
        /// </summary>
        public bool LoadedProperly = false;

        /// <summary>
        /// Removes the shader from the system.
        /// </summary>
        public void Remove()
        {
            if (GL.IsProgram(Original_Program))
            {
                GL.DeleteProgram(Original_Program);
            }
            Engine.LoadedShaders.Remove(this);
        }

        /// <summary>
        /// Binds this shader to OpenGL.
        /// </summary>
        public void Bind()
        {
            GL.UseProgram(Internal_Program);
        }
    }
}
