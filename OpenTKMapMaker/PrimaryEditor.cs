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

        public float mouse_sens = 5.0f;

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
            glControlTop.MouseWheel += new MouseEventHandler(glControlTop_MouseWheel);
            glControlSide.MouseWheel += new MouseEventHandler(glControlSide_MouseWheel);
            tW.Interval = 50;
            tA.Interval = 50;
            tS.Interval = 50;
            tD.Interval = 50;
            tW.Tick += new EventHandler(tW_Tick);
            tA.Tick += new EventHandler(tA_Tick);
            tS.Tick += new EventHandler(tS_Tick);
            tD.Tick += new EventHandler(tD_Tick);
        }

        public static float side_zoom = 1;

        void glControlSide_MouseWheel(object sender, MouseEventArgs e)
        {
            side_zoom *= (e.Delta >= 0 ? 1.1f : 0.9f);
            if (side_zoom == 0f)
            {
                side_zoom = 0.001f;
            }
            glControlSide.Invalidate();
        }

        public static float top_zoom = 1;

        void glControlTop_MouseWheel(object sender, MouseEventArgs e)
        {
            top_zoom *= (e.Delta >= 0 ? 1.1f : 0.9f);
            if (top_zoom == 0f)
            {
                top_zoom = 0.001f;
            }
            glControlTop.Invalidate();
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
                ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
                CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlView.Focused ? "@" : "!") + "^e^7" + CameraYaw + "/" + CameraPitch, new Location(0, 0, 0));
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
                ortho = Matrix4.CreateOrthographicOffCenter(-500f / top_zoom + (float)top_translate.X / top_zoom, 500f / top_zoom + (float)top_translate.X / top_zoom,
                    500f / top_zoom + (float)top_translate.Y / top_zoom, -500f / top_zoom + (float)top_translate.Y / top_zoom, -100000, 100000);
                GL.UniformMatrix4(1, false, ref ortho);
                Render3D(CurrentContext);
                ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
                GL.Enable(EnableCap.Texture2D);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlTop.Focused ? "@": "!") + "^e^7" + top_zoom.ToString(), new Location(0, 0, 0));
                GL.Disable(EnableCap.Texture2D);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
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
            GL.Disable(EnableCap.Texture2D);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        private void glControlSide_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextSide;
            glControlSide.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            ortho = Matrix4.CreateOrthographicOffCenter(-500f / side_zoom + (float)side_translate.X / side_zoom, 500f / side_zoom + (float)side_translate.X / side_zoom,
                500f / side_zoom + (float)side_translate.Y / side_zoom, -500f / side_zoom + (float)side_translate.Y / side_zoom, -100000, 100000) * Matrix4.CreateRotationX(90);
            GL.UniformMatrix4(1, false, ref ortho);
            Render3D(CurrentContext);
            ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
            GL.Enable(EnableCap.Texture2D);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlSide.Focused ? "@" : "!") + "^e^7" + side_zoom.ToString(), new Location(0, 0, 0));
            GL.Disable(EnableCap.Texture2D);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
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
            ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
            CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlTex.Focused ? "@" : "!") + "^e^7Textures", new Location(0, 0, 0));
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
                float mx = (float)(e.X - glControlView.Width / 2) / 25f * mouse_sens / 5.0f;
                float my = (float)(e.Y - glControlView.Height / 2) / 25f * mouse_sens / 5.0f;
                CameraYaw -= mx;
                CameraPitch -= my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    OpenTK.Input.Mouse.SetPosition(this.Location.X + splitContainer1.SplitterDistance + splitContainer1.SplitterRectangle.Width + 8 + glControlView.Width / 2, this.Location.Y + 31 + menuStrip1.Height + glControlView.Height / 2);
                }
            }
        }

        private void glControlView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                view_selected = !view_selected;
            }
        }

        private void glControlSide_MouseEnter(object sender, EventArgs e)
        {
            glControlSide.Focus();
            invalidateAll();
        }

        private void glControlTop_MouseEnter(object sender, EventArgs e)
        {
            glControlTop.Focus();
            invalidateAll();
        }

        private void glControlTex_MouseEnter(object sender, EventArgs e)
        {
            glControlTex.Focus();
            invalidateAll();
        }

        private void glControlView_MouseEnter(object sender, EventArgs e)
        {
            glControlView.Focus();
            invalidateAll();
        }

        public void invalidateAll()
        {
            glControlView.Invalidate();
            glControlTex.Invalidate();
            glControlTop.Invalidate();
            glControlSide.Invalidate();
        }

        bool top_selected = false;

        Location top_translate = new Location(0, 0, 0);

        private void glControlTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (top_selected)
            {
                glControlTop.Invalidate();
                float mx = (float)(e.X - glControlTop.Width / 2) / 25f * mouse_sens;
                float my = (float)(e.Y - glControlTop.Height / 2) / 25f * mouse_sens;
                top_translate.X -= mx;
                top_translate.Y -= my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlTop.Width / 2, this.Location.Y + 31 + menuStrip1.Height + glControlTop.Height / 2);
                }
            }
        }

        private void glControlTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                top_selected = true;
            }
        }

        private void PrimaryEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                top_selected = false;
                side_selected = false;
            }
        }

        private void glControlTop_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                top_selected = false;
                side_selected = false;
            }
        }

        bool side_selected = false;

        Location side_translate = new Location(0, 0, 0);

        private void glControlSide_MouseMove(object sender, MouseEventArgs e)
        {
            if (side_selected)
            {
                glControlSide.Invalidate();
                float mx = (float)(e.X - glControlSide.Width / 2) / 25f * mouse_sens;
                float my = (float)(e.Y - glControlSide.Height / 2) / 25f * mouse_sens;
                side_translate.X -= mx;
                side_translate.Y -= my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlSide.Width / 2,
                        this.Location.Y + 31 + menuStrip1.Height + splitContainer2.SplitterDistance + splitContainer2.SplitterRectangle.Height + glControlSide.Height / 2);
                }
            }
        }

        private void glControlSide_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                top_selected = false;
                side_selected = false;
            }
        }

        private void glControlSide_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                side_selected = true;
            }
        }
        Timer tW = new Timer();
        Timer tA = new Timer();
        Timer tS = new Timer();
        Timer tD = new Timer();

        private void glControlView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                tW_Tick(null, null);
                tW.Start();
            }
            else if (e.KeyCode == Keys.A)
            {
                tA_Tick(null, null);
                tA.Start();
            }
            else if (e.KeyCode == Keys.S)
            {
                tS_Tick(null, null);
                tS.Start();
            }
            else if (e.KeyCode == Keys.D)
            {
                tD_Tick(null, null);
                tD.Start();
            }
        }

        private void glControlView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                tW.Stop();
            }
            else if (e.KeyCode == Keys.A)
            {
                tA.Stop();
            }
            else if (e.KeyCode == Keys.S)
            {
                tS.Stop();
            }
            else if (e.KeyCode == Keys.D)
            {
                tD.Stop();
            }
        }

        void tD_Tick(object sender, EventArgs e)
        {
            CameraPos += Utilities.ForwardVector_Deg(CameraYaw - 90, 0);
            glControlView.Invalidate();
        }

        void tS_Tick(object sender, EventArgs e)
        {
            CameraPos -= Utilities.ForwardVector_Deg(CameraYaw, CameraPitch);
            glControlView.Invalidate();
        }

        void tA_Tick(object sender, EventArgs e)
        {
            CameraPos += Utilities.ForwardVector_Deg(CameraYaw + 90, 0);
            glControlView.Invalidate();
        }

        void tW_Tick(object sender, EventArgs e)
        {
            CameraPos += Utilities.ForwardVector_Deg(CameraYaw, CameraPitch);
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
