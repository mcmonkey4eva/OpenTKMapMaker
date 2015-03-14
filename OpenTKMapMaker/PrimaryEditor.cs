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

namespace OpenTKMapMaker
{
    public partial class PrimaryEditor : Form
    {
        public static Matrix4 ortho;

        public static int vpw;
        public static int vph;

        public static List<GLContext> Contexts = new List<GLContext>();

        public PrimaryEditor()
        {
            InitializeComponent();
            SysConsole.Init();
            this.FormClosed += new FormClosedEventHandler(PrimaryEditor_FormClosed);
            menuStrip1.Renderer = new MyRenderer();
            /*Timer timer = new Timer();
            timer.Interval = 200;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();*/
        }

        void PrimaryEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        /*
        void timer_Tick(object sender, EventArgs e)
        {
            glControlView.Invalidate();
            glControlTop.Invalidate();
            glControlSide.Invalidate();
            glControlTex.Invalidate();
        }
        */

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

        private void glControlView_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextView;
            glControlView.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
            CurrentContext.FontSets.Standard.DrawColoredText("^!^e^7Hello World!", new Location(0, 0, 0));
            glControlView.SwapBuffers();
        }

        GLContext ContextTop;
        private void glControlTop_Load(object sender, EventArgs e)
        {
            glControlTop.MakeCurrent();
            ResizeTop();
            ContextTop = new GLContext();
            ContextTop.Control = glControlTop;
            InitGL(ContextTop);
        }

        private void glControlTop_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextTop;
            glControlTop.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            glControlTop.SwapBuffers();
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
