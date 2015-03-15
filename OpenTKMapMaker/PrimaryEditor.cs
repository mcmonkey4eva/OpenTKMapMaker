using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTKMapMaker.GraphicsSystem;
using OpenTKMapMaker.Utility;
using OpenTKMapMaker.EntitySystem;

namespace OpenTKMapMaker
{
    public partial class PrimaryEditor : Form
    {
        public static Matrix4 ortho;

        public static int vpw;
        public static int vph;

        public static List<Entity> Entities = new List<Entity>();

        public static List<GLContext> Contexts = new List<GLContext>();

        public PrimaryEditor()
        {
            InitializeComponent();
            SysConsole.Init();
            this.FormClosed += new FormClosedEventHandler(PrimaryEditor_FormClosed);
            menuStrip1.Renderer = new MyRenderer();
            Entities.Add(new CubeEntity(new Location(-100, -100, -10), new Location(100, 100, 0)));
            Entities.Add(new PointLightEntity(new Location(0, 0, 30), 100, new Location(1f, 1f, 1f)));
            Entities.Add(new SpawnPointEntity(new Location(0, 0, 10)));
            PickCameraSpawn();
            auto_redrawer.Tick += new EventHandler(auto_redrawer_Tick);
        }

        public Location CameraPos;
        public float CameraYaw = 0;
        public float CameraPitch = 0;
        public float CameraFOV = 45f;
        public float CameraZNear = 0.1f;
        public float CameraZFar = 1000f;
        public Location CameraUp = new Location(0, 0, 1);

        public void PickCameraSpawn()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is SpawnPointEntity)
                {
                    CameraPos = Entities[i].Position;
                    return;
                }
            }
            CameraPos = new Location(0, 0, 0);
        }

        public void Render3D(GLContext context)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Render(context);
            }
        }

        void PrimaryEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        static void InitGL(GLContext context)
        {
            try
            {
                context.Control.MakeCurrent();
                Contexts.Add(context);
                context.Textures = new TextureEngine();
                context.Textures.InitTextureSystem();
                context.Shaders = new ShaderEngine();
                context.Shaders.InitShaderSystem();
                context.Fonts = new GLFontEngine(context.Shaders);
                context.Fonts.Init();
                context.FontSets = new FontSetEngine(context.Fonts);
                context.FontSets.Init();
                context.Models = new ModelEngine();
                context.Models.Init();
                context.Rendering = new Renderer(context.Textures);
                context.Rendering.Init();
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "InitGL: " + ex.ToString());
            }
        }

        GLContext ContextView;
        private void glControlView_Load(object sender, EventArgs e)
        {
            glControlView.MakeCurrent();
            ResizeView();
            ContextView = new GLContext();
            ContextView.Control = glControlView;
            InitGL(ContextView);
        }

        Matrix4 proj;
        Matrix4 view;
        Matrix4 combined;

        private void glControlView_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                CurrentContext = ContextView;
                glControlView.MakeCurrent();
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
                CurrentContext.Shaders.ColorMultShader.Bind();
                Location CameraTarget = CameraPos + Utilities.ForwardVector_Deg(CameraYaw, CameraPitch);
                proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraFOV), (float)CurrentContext.Control.Width / (float)CurrentContext.Control.Height, CameraZNear, CameraZFar);
                view = Matrix4.LookAt(CameraPos.ToOVector(), CameraTarget.ToOVector(), CameraUp.ToOVector());
                combined = view * proj;
                GL.UniformMatrix4(1, false, ref combined);
                Render3D(CurrentContext);
                ortho = combined;
                Matrix4 mat = Matrix4.Identity;
                GL.UniformMatrix4(2, false, ref mat);
                CurrentContext.FontSets.Standard.DrawColoredText("^!^e^7Hello World!", new Location(0, 0, 0));
                ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
                CurrentContext.FontSets.Standard.DrawColoredText("^!^e^7Hello World! YAW:" + CameraYaw + ", PITCH: " + CameraPitch + ", R: " + Utilities.UtilRandom.NextDouble().ToString(), new Location(0, 0, 0));
                glControlView.SwapBuffers();
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "PaintView: " + ex.ToString());
            }
        }

        GLContext ContextTop;
        private void glControlTop_Load(object sender, EventArgs e)
        {
            glControlTop.MakeCurrent();
            ResizeTop();
            ContextTop = new GLContext();
            ContextTop.Control = glControlTop;
            InitGL(ContextTop);
            GL.Disable(EnableCap.Texture2D);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        private void glControlTop_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                CurrentContext = ContextTop;
                glControlTop.MakeCurrent();
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
                ortho = Matrix4.CreateOrthographicOffCenter(-500, 500, 500, -500, -100000, 100000);
                GL.UniformMatrix4(1, false, ref ortho);
                Render3D(CurrentContext);
                Matrix4 mat = Matrix4.Identity;
                GL.UniformMatrix4(2, false, ref mat);
                CurrentContext.FontSets.Standard.DrawColoredText("^!^e^7Hello World!", new Location(0, 0, 0));
                glControlTop.SwapBuffers();
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "PaintTop: " + ex.ToString());
            }
        }

        GLContext ContextSide;
        private void glControlSide_Load(object sender, EventArgs e)
        {
            glControlSide.MakeCurrent();
            ResizeSide();
            ContextSide = new GLContext();
            ContextSide.Control = glControlSide;
            InitGL(ContextSide);
        }

        private void glControlSide_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextSide;
            glControlSide.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            glControlSide.SwapBuffers();
        }

        GLContext ContextTex;
        private void glControlTex_Load(object sender, EventArgs e)
        {
            glControlTex.MakeCurrent();
            ResizeTex();
            ContextTex = new GLContext();
            ContextTex.Control = glControlTex;
            InitGL(ContextTex);
        }

        private void glControlTex_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextTex;
            glControlTex.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            glControlTex.SwapBuffers();
        }

        void ResizeView()
        {
            glControlView.Size = splitContainer3.Panel1.ClientSize;
        }

        void ResizeTex()
        {
            glControlTex.Size = splitContainer3.Panel2.ClientSize;
        }

        void ResizeTop()
        {
            glControlTop.Size = splitContainer2.Panel1.ClientSize;
        }

        void ResizeSide()
        {
            glControlSide.Size = splitContainer2.Panel2.ClientSize;
        }

        // Vertical - Left
        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeTop();
            ResizeSide();
        }

        // Vertical - Right
        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeView();
            ResizeTex();
        }

        // Horizontal
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeView();
            ResizeTex();
            ResizeTop();
            ResizeSide();
        }

        GLContext CurrentContext;

        private void glControlView_Resize(object sender, EventArgs e)
        {
            glControlView.MakeCurrent();

            GL.Viewport(0, 0, glControlView.Width, glControlView.Height);
        }

        private void glControlTop_Resize(object sender, EventArgs e)
        {
            glControlTop.MakeCurrent();
            GL.Viewport(0, 0, glControlTop.Width, glControlTop.Height);
        }

        private void glControlSide_Resize(object sender, EventArgs e)
        {
            glControlSide.MakeCurrent();
            GL.Viewport(0, 0, glControlSide.Width, glControlSide.Height);
        }

        private void glControlTex_Resize(object sender, EventArgs e)
        {
            glControlTex.MakeCurrent();
            GL.Viewport(0, 0, glControlTex.Width, glControlTex.Height);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        bool view_selected = false;

        private void glControlView_MouseMove(object sender, MouseEventArgs e)
        {
            if (view_selected)
            {
                glControlView.Invalidate();
                float mx = (float)(e.X - glControlView.Width / 2) / 25f;
                float my = (float)(e.Y - glControlView.Height / 2) / 25f;
                CameraYaw -= mx;
                CameraPitch -= my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    OpenTK.Input.Mouse.SetPosition(this.Location.X + splitContainer1.SplitterDistance + splitContainer1.SplitterRectangle.Width + 8 + glControlView.Width / 2, this.Location.Y + 31 + menuStrip1.Height + glControlView.Height / 2);
                }
            }
        }

        Timer auto_redrawer = new Timer();

        private void glControlView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                view_selected = !view_selected;
                auto_redrawer.Interval = 50;
                if (view_selected)
                {
                    auto_redrawer.Start();
                }
                else
                {
                    auto_redrawer.Stop();
                }
            }
        }

        void auto_redrawer_Tick(object sender, EventArgs e)
        {
            glControlView.Invalidate();
        }
    }

    class MyRenderer : ToolStripProfessionalRenderer
    {
        public MyRenderer() : base(new MyColors()) { }
    }

    class MyColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected { get { return Color.DarkGreen; } }
        public override Color MenuItemSelectedGradientBegin { get { return Color.DarkGreen; } }
        public override Color MenuItemSelectedGradientEnd { get { return Color.DarkGreen; } }
        public override Color ButtonPressedHighlight { get { return Color.DarkGreen; } }
        public override Color ButtonPressedGradientBegin { get { return Color.DarkGreen; } }
        public override Color ButtonPressedGradientEnd { get { return Color.DarkGreen; } }
        public override Color ButtonSelectedHighlight { get { return Color.DarkGreen; } }
        public override Color ButtonSelectedGradientBegin { get { return Color.DarkGreen; } }
        public override Color ButtonSelectedGradientEnd { get { return Color.DarkGreen; } }
        public override Color MenuItemPressedGradientBegin { get { return Color.DarkGreen; } }
        public override Color MenuItemPressedGradientEnd { get { return Color.DarkGreen; } }
    }
}
