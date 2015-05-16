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
using System.IO;
using BEPUphysics;
using BEPUphysics.Settings;

namespace OpenTKMapMaker
{
    public partial class PrimaryEditor : Form
    {

        public static PrimaryEditor PRFMain;

        public static Matrix4 ortho;

        public static int vpw;
        public static int vph;

        public static List<Entity> Entities = new List<Entity>();

        public static List<Entity> Selected = new List<Entity>();

        public ContextMenuStrip entityTypeChooser;

        public List<ClipboardEntity> Clipboard = new List<ClipboardEntity>();

        public List<List<ClipboardEntity>> History = new List<List<ClipboardEntity>>();

        public void Select(Entity e)
        {
            if (!e.Selected)
            {
                Selected.Add(e);
                e.Selected = true;
                invalidateAll();
            }
        }

        public void Deselect(Entity e)
        {
            if (e.Selected)
            {
                Selected.Remove(e);
                e.Selected = false;
                invalidateAll();
            }
        }

        public static void Spawn(Entity e)
        {
            if (!Entities.Contains(e))
            {
                Entities.Add(e);
                e.Selected = false;
            }
        }

        public static void Despawn(Entity e)
        {
            if (Entities.Contains(e))
            {
                e.OnDespawn();
                if (e.Selected)
                {
                    Selected.Remove(e);
                }
                Entities.Remove(e);
            }
        }

        public void SavePoint()
        {
            List<ClipboardEntity> clips = new List<ClipboardEntity>();
            for (int i = 0; i < Entities.Count; i++)
            {
                ClipboardEntity cbe = new ClipboardEntity();
                cbe.entitytype = Entities[i].GetEntityType();
                cbe.variables = Entities[i].GetVars();
                cbe.color = Entities[i].ViewColor;
                clips.Add(cbe);
            }
            History.Add(clips);
            if (History.Count > 10)
            {
                History.RemoveAt(0);
            }
        }

        public static List<GLContext> Contexts = new List<GLContext>();

        public float mouse_sens = 5.0f;

        public void LoadEntities()
        {
            CubeEntity ce = new CubeEntity(new Location(-80, -80, -10), new Location(80, 80, 0));
            ce.Recalculate();
            Spawn(ce);
            PointLightEntity ple = new PointLightEntity(new Location(0, 0, 50), 100, new Location(1, 1, 1), false);
            ple.Recalculate();
            Spawn(ple);
        }

        public void entityTypeChooser_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SetType(e.ClickedItem.Text);
        }

        public void triggerTouchClicked(object sender, EventArgs e)
        {
            SetType("triggertouch");
        }

        public void SetType(string text)
        {
            if (Selected.Count != 1)
            {
                return;
            }
            Entity sel = Selected[0];
            Entity ent = GetForType(text.ToLower());
            if (ent != null)
            {
                ent.Position = sel.Position;
                ent.Angle = sel.Angle;
                ent.Velocity = sel.Velocity;
                ent.Angular_Velocity = sel.Angular_Velocity;
                ent.Friction = sel.Friction;
                ent.Mass = sel.Mass;
                ent.Solid = sel.Solid;
                if (ent is CuboidalEntity && sel is CuboidalEntity)
                {
                    ((CuboidalEntity)ent).Mins = ((CuboidalEntity)sel).Mins;
                    ((CuboidalEntity)ent).Maxes = ((CuboidalEntity)sel).Maxes;
                }
                ent.Recalculate();
                SavePoint();
                Spawn(ent);
                Despawn(sel);
                Select(ent);
            }
        }

        public CollisionUtil Collision;

        public PrimaryEditor()
        {
            PRFMain = this;
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(PrimaryEditor_FormClosed);
            menuStrip1.Renderer = new MyRenderer();
            PickCameraSpawn();
            glControlView.MouseWheel += new MouseEventHandler(glControlView_MouseWheel);
            glControlTop.MouseWheel += new MouseEventHandler(glControlTop_MouseWheel);
            glControlSide.MouseWheel += new MouseEventHandler(glControlSide_MouseWheel);
            glControlOSide.MouseWheel += new MouseEventHandler(glControlOSide_MouseWheel);
            tW.Tick += new EventHandler(tW_Tick);
            tA.Tick += new EventHandler(tA_Tick);
            tS.Tick += new EventHandler(tS_Tick);
            tD.Tick += new EventHandler(tD_Tick);
            this.GotFocus += new EventHandler(PrimaryEditor_GotFocus);
            glControlView.GotFocus += new EventHandler(PrimaryEditor_GotFocus);
            entityTypeChooser = new ContextMenuStrip();
            entityTypeChooser.Items.Add("Cube");
            entityTypeChooser.Items.Add("PointLight");
            entityTypeChooser.Items.Add("Spawn");
            entityTypeChooser.Items.Add("Model");
            ToolStripDropDownButton tsddb = new ToolStripDropDownButton("Trigger*");
            tsddb.DropDownItems.Add("TriggerTouch", null, new EventHandler(triggerTouchClicked));
            entityTypeChooser.Items.Add(tsddb);
            entityTypeChooser.Items.Add("Cancel.");
            entityTypeChooser.ItemClicked += new ToolStripItemClickedEventHandler(entityTypeChooser_ItemClicked);
            tsddb.DropDownItemClicked += new ToolStripItemClickedEventHandler(entityTypeChooser_ItemClicked);
            entityTypeChooser.CreateControl();
            this.PreviewKeyDown += new PreviewKeyDownEventHandler(PrimaryEditor_PreviewKeyDown);
            this.glControlView.PreviewKeyDown += new PreviewKeyDownEventHandler(PrimaryEditor_PreviewKeyDown);
            this.glControlTop.PreviewKeyDown += new PreviewKeyDownEventHandler(PrimaryEditor_PreviewKeyDown);
            this.glControlSide.PreviewKeyDown += new PreviewKeyDownEventHandler(PrimaryEditor_PreviewKeyDown);
            this.glControlOSide.PreviewKeyDown += new PreviewKeyDownEventHandler(PrimaryEditor_PreviewKeyDown);
            SavePoint();
            PhysicsWorld = new Space();
            // Set the world's general default gravity
            PhysicsWorld.ForceUpdater.Gravity = new Location(0, 0, -9.8f).ToBVector();
            // Minimize penetration
            CollisionDetectionSettings.AllowedPenetration = 0.001f;
            // Load a CollisionUtil instance
            Collision = new CollisionUtil(PhysicsWorld);
            ents.Add("pointlight", new PointLightEntity(new Location(0), 50, new Location(1), false));
            ents.Add("cube", new CubeEntity(new Location(-1), new Location(1)));
            ents.Add("spawn", new SpawnPointEntity(new Location(0)));
            ents.Add("model", new ModelEntity(""));
            ents.Add("triggertouch", new TriggerTouchEntity());
        }

        Dictionary<string, Entity> ents = new Dictionary<string, Entity>();

        public Entity GetForType(string type)
        {
            Entity e;
            if (ents.TryGetValue(type.ToLower(), out e))
            {
                return e.CreateInstance();
            }
            SysConsole.Output(OutputType.WARNING, "Trying to get etype " + type + ", returning null!");
            return null;
        }

        public Space PhysicsWorld;

        void InitSpace()
        {
        }

        void PrimaryEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    if (glControlView.Focused)
                    {
                        glControlView_KeyDown(null, new KeyEventArgs(e.KeyCode));
                    }
                    else if (glControlTop.Focused)
                    {
                        glControlTop_KeyDown(null, new KeyEventArgs(e.KeyCode));
                    }
                    else if (glControlSide.Focused)
                    {
                        glControlSide_KeyDown(null, new KeyEventArgs(e.KeyCode));
                    }
                    else if (glControlOSide.Focused)
                    {
                        glControlOSide_KeyDown(null, new KeyEventArgs(e.KeyCode));
                    }
                    return;
                default:
                    return;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    glControlView_KeyDown(null, e);
                    return;
                default:
                    base.OnKeyDown(e);
                    return;
            }
        }

        EntityControlForm ecf = null;

        FaceEditor fe = null;

        void PrimaryEditor_GotFocus(object sender, EventArgs e)
        {
            if (ecf != null && ecf.Visible)
            {
                ecf.Focus();
            }
            if (fe != null && fe.Visible)
            {
                fe.Focus();
            }
        }

        public static float side_zoom = 10;

        void glControlSide_MouseWheel(object sender, MouseEventArgs e)
        {
            side_zoom *= (e.Delta >= 0 ? 1.1f : 0.9f);
            if (side_zoom < 0.1f)
            {
                side_zoom = 0.1f;
            }
            else if (side_zoom > 300f)
            {
                side_zoom = 300f;
            }
            glControlSide.Invalidate();
        }

        public static float oside_zoom = 10;

        void glControlOSide_MouseWheel(object sender, MouseEventArgs e)
        {
            oside_zoom *= (e.Delta >= 0 ? 1.1f : 0.9f);
            if (oside_zoom < 0.1f)
            {
                oside_zoom = 0.1f;
            }
            else if (oside_zoom > 300f)
            {
                oside_zoom = 300f;
            }
            glControlOSide.Invalidate();
        }

        public static float top_zoom = 10;

        void glControlTop_MouseWheel(object sender, MouseEventArgs e)
        {
            top_zoom *= (e.Delta >= 0 ? 1.1f : 0.9f);
            if (top_zoom < 0.1f)
            {
                top_zoom = 0.1f;
            }
            else if (top_zoom > 300f)
            {
                top_zoom = 300f;
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

        public static bool RenderEntities = false;

        public static bool RenderLines = false;

        public static bool RenderTextures = false;

        public static bool RenderLights = false;

        public void Render3D(GLContext context, bool render_entities, bool render_lines, bool render_textures, bool render_lights)
        {
            RenderLines = render_lines;
            RenderEntities = render_entities;
            RenderTextures = render_textures;
            RenderLights = render_lights;
            for (int i = 0; i < Entities.Count; i++)
            {
                if (RenderLines)
                {
                    context.Rendering.SetColor(Entities[i].ViewColor);
                }
                Entities[i].Render(context);
            }
            if (RenderEntities && RenderLines)
            {
                context.Rendering.SetColor(Color4.Cyan);
                context.Rendering.RenderLineBox(CameraPos - new Location(1), CameraPos + new Location(1)); // TODO: Camera Model
                context.Rendering.RenderLine(CameraPos, CameraPos + Utilities.ForwardVector_Deg(CameraYaw, CameraPitch) * 10);
                context.Rendering.SetColor(Color4.White);
            }
        }

        public void renderSelections(GLContext context, bool dtest)
        {
            RenderLights = false;
            bool rt = RenderTextures;
            GL.BindTexture(TextureTarget.Texture2D, 0);
            RenderTextures = false;
            if (dtest)
            {
                GL.Disable(EnableCap.DepthTest);
            }
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(2);
            context.Textures.White.Bind();
            context.Rendering.SetColor(Color4.Red);
            for (int i = 0; i < Selected.Count; i++)
            {
                Selected[i].Render(context);
            }
            if (dtest)
            {
                GL.Enable(EnableCap.DepthTest);
            }
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.LineWidth(1);
            context.Rendering.SetColor(Color4.White);
            RenderTextures = rt;
        }

        public void renderWires(GLContext context)
        {
            RenderLights = false;
            bool rt = RenderTextures;
            GL.BindTexture(TextureTarget.Texture2D, 0);
            RenderTextures = false;
            GL.Disable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            context.Textures.White.Bind();
            context.Rendering.SetColor(Color4.Green);
            for (int i = 0; i < Entities.Count; i++)
            {
                context.Rendering.SetColor(Entities[i].ViewColor);
                Entities[i].Render(context);
            }
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.LineWidth(1);
            context.Rendering.SetColor(Color4.White);
            RenderTextures = rt;
        }

        void PrimaryEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        static void defaultBlending()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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
                defaultBlending();
                GL.Viewport(0, 0, context.Control.Width, context.Control.Height);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);
                GL.Enable(EnableCap.Multisample); // TODO: Actually use multi-sampling?
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "InitGL: " + ex.ToString());
            }
        }

        Texture top_backgrid;

        GLContext ContextTop;
        private void glControlTop_Load(object sender, EventArgs e)
        {
            glControlTop.MakeCurrent();
            ResizeTop();
            ContextTop = new GLContext();
            ContextTop.Control = glControlTop;
            InitGL(ContextTop);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            top_backgrid = ContextTop.Textures.GetTexture("mapmaker/backgrid");
        }

        Matrix4 top_proj;

        private void glControlTop_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                CurrentContext = ContextTop;
                glControlTop.MakeCurrent();
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
                float rat = (float)CurrentContext.Control.Width / (float)CurrentContext.Control.Height;
                ortho = Matrix4.CreateOrthographicOffCenter((-500f / top_zoom + (float)top_translate.X) * rat, (500f / top_zoom + (float)top_translate.X) * rat,
                    500f / top_zoom + (float)top_translate.Y, -500f / top_zoom + (float)top_translate.Y, -1000000, 1000000);
                top_proj = ortho;
                GL.UniformMatrix4(1, false, ref ortho);
                top_backgrid.Bind();
                CurrentContext.Rendering.RenderBackgrid(Matrix4.Identity);
                CurrentContext.Textures.White.Bind();
                Render3D(CurrentContext, true, true, false, false);
                renderSelections(CurrentContext, false);
                ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
                GL.Enable(EnableCap.Texture2D);
                CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlTop.Focused ? "@": "!") + "^e^7Top (x/y), zoom=" + top_zoom.ToString(), new Location(0, 0, 0));
                GL.Disable(EnableCap.Texture2D);
                glControlTop.SwapBuffers();
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "PaintTop: " + ex.ToString());
            }
        }

        Texture side_backgrid;

        GLContext ContextSide;
        private void glControlSide_Load(object sender, EventArgs e)
        {
            glControlSide.MakeCurrent();
            ResizeSide();
            ContextSide = new GLContext();
            ContextSide.Control = glControlSide;
            InitGL(ContextSide);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            side_backgrid = ContextSide.Textures.GetTexture("mapmaker/backgrid");
        }

        Matrix4 side_proj;

        private void glControlSide_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextSide;
            glControlSide.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1));
            float rat = (float)CurrentContext.Control.Width / (float)CurrentContext.Control.Height;
            ortho = Matrix4.CreateOrthographicOffCenter((-500f / side_zoom + (float)side_translate.X) * rat, (500f / side_zoom + (float)side_translate.X) * rat,
                500f / side_zoom + (float)side_translate.Y, -500f / side_zoom + (float)side_translate.Y, -1000000, 1000000) * Matrix4.CreateScale(-1, 1, 1);
            side_proj = ortho;
            GL.UniformMatrix4(1, false, ref ortho);
            side_backgrid.Bind();
            CurrentContext.Rendering.RenderBackgrid(Matrix4.Identity);
            CurrentContext.Textures.White.Bind();
            ortho = view * ortho;
            side_proj = ortho;
            GL.UniformMatrix4(1, false, ref ortho);
            Render3D(CurrentContext, true, true, false, false);
            renderSelections(CurrentContext, false);
            ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
            GL.Enable(EnableCap.Texture2D);
            CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlSide.Focused ? "@" : "!") + "^e^7Side (x/z), zoom=" + side_zoom.ToString(), new Location(0, 0, 0));
            GL.Disable(EnableCap.Texture2D);
            glControlSide.SwapBuffers();
        }

        Texture oside_backgrid;

        GLContext ContextOSide;
        void glControlOSide_Load(object sender, System.EventArgs e)
        {
            glControlOSide.MakeCurrent();
            ResizeOSide();
            ContextOSide = new GLContext();
            ContextOSide.Control = glControlOSide;
            InitGL(ContextOSide);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            oside_backgrid = ContextSide.Textures.GetTexture("mapmaker/backgrid");
        }

        Matrix4 oside_proj;

        void glControlOSide_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            CurrentContext = ContextOSide;
            glControlOSide.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1));
            float rat = (float)CurrentContext.Control.Width / (float)CurrentContext.Control.Height;
            ortho = Matrix4.CreateOrthographicOffCenter((-500f / oside_zoom + (float)oside_translate.X) * rat, (500f / oside_zoom + (float)oside_translate.X) * rat,
                500f / oside_zoom + (float)oside_translate.Y, -500f / oside_zoom + (float)oside_translate.Y, -1000000, 1000000) * Matrix4.CreateScale(-1, 1, 1); // TODO: Rotate
            oside_proj = ortho;
            GL.UniformMatrix4(1, false, ref ortho);
            oside_backgrid.Bind();
            CurrentContext.Rendering.RenderBackgrid(Matrix4.Identity);
            CurrentContext.Textures.White.Bind();
            ortho = view * ortho;
            oside_proj = ortho;
            GL.UniformMatrix4(1, false, ref ortho);
            Render3D(CurrentContext, true, true, false, false);
            renderSelections(CurrentContext, false);
            ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
            GL.Enable(EnableCap.Texture2D);
            CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlOSide.Focused ? "@" : "!") + "^e^7Side (y/z), zoom=" + oside_zoom.ToString(), new Location(0, 0, 0));
            GL.Disable(EnableCap.Texture2D);
            glControlOSide.SwapBuffers();
        }

        GLContext ContextTex;
        private void glControlTex_Load(object sender, EventArgs e)
        {
            glControlTex.MakeCurrent();
            ResizeTex();
            ContextTex = new GLContext();
            ContextTex.Control = glControlTex;
            InitGL(ContextTex);
            GL.Enable(EnableCap.Texture2D);
            glControlTex.MouseWheel += new MouseEventHandler(glControlTex_MouseWheel);
            if (ContextView != null)
            {
                for (int i = 0; i < ContextView.Textures.LoadedTextures.Count; i++)
                {
                    ContextTex.Textures.GetTexture(ContextView.Textures.LoadedTextures[i].Name);
                }
            }
        }

        void glControlTex_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0 && canscrolldown)
            {
                tex_scroll += 64;
                glControlTex.Invalidate();
            }
            else if (e.Delta > 0 && tex_scroll > 0)
            {
                tex_scroll -= 64;
                glControlTex.Invalidate();
            }
            if (tex_scroll < 0)
            {
                tex_scroll = 0;
            }
        }

        int tex_scroll = 0;

        bool canscrolldown = false;

        void ViewTextures_OnTextureLoaded(object sender, TextureLoadedEventArgs e)
        {
            if (ContextTex == null)
            {
                return;
            }
            ContextTex.Control.MakeCurrent();
            ContextTex.Textures.GetTexture(e.Tex.Name);
            ContextView.Control.MakeCurrent();
        }

        int tex_sel = 0;

        private void glControlTex_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextTex;
            glControlTex.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
            int xp = 5;
            int yp = 20;
            if (CurrentContext.Control.Width > 138)
            {
                CurrentContext.Shaders.ColorMultShader.Bind();
                GL.UniformMatrix4(1, false, ref ortho);
                for (int i = 0; i < CurrentContext.Textures.LoadedTextures.Count; i++)
                {
                    Texture t = CurrentContext.Textures.LoadedTextures[i];
                    if (yp + 138 > tex_scroll && yp - tex_scroll < CurrentContext.Control.Height)
                    {
                        if (texmouseDown && texMPos.X >= xp && texMPos.X <= xp + 128 && texMPos.Y >= yp - tex_scroll && texMPos.Y <= yp + 128 - tex_scroll)
                        {
                            tex_sel = i;
                            texmouseDown = false;
                            glControlTex.Invalidate();
                            SavePoint();
                            for (int s = 0; s < Selected.Count; s++)
                            {
                                if (Selected[s] is CubeEntity)
                                {
                                    ((CubeEntity)Selected[s]).Textures = new string[] { t.Name, t.Name, t.Name, t.Name, t.Name, t.Name };
                                    Selected[s].Recalculate();
                                }
                            }
                            glControlView.MakeCurrent();
                            glControlView.Invalidate();
                        }
                        if (tex_sel == i)
                        {
                            CurrentContext.Textures.White.Bind();
                            CurrentContext.Rendering.SetColor(Color4.Red);
                            CurrentContext.Rendering.RenderRectangle(xp - 3, yp - tex_scroll - 3, xp + 128 + 3, yp + 128 - tex_scroll + 3);
                            CurrentContext.Rendering.SetColor(Color4.White);
                        }
                        t.Bind();
                        CurrentContext.Rendering.RenderRectangle(xp, yp - tex_scroll, xp + 128, yp + 128 - tex_scroll);
                        StringBuilder tname = new StringBuilder(t.Name.Length);
                        int start = 0;
                        if (t.Name.Length > 0)
                        {
                            tname.Append(t.Name[0]);
                        }
                        int count = 0;
                        for (int x = 1; x < t.Name.Length; x++)
                        {
                            if (CurrentContext.FontSets.SlightlyBigger.MeasureFancyText("^S" + t.Name.Substring(start, x - start)) >= 128)
                            {
                                tname.Append("\n");
                                count++;
                                if (count >= 5)
                                {
                                    break;
                                }
                                start = x;
                            }
                            tname.Append(t.Name[x]);
                        }
                        CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^!^e^7" + tname, new Location(xp, yp - tex_scroll, 0));
                    }
                    xp += 138;
                    if (xp + 158 > CurrentContext.Control.Width)
                    {
                        yp += 138;
                        xp = 5;
                    }
                    if (yp - tex_scroll + 138 > CurrentContext.Control.Height)
                    {
                        canscrolldown = true;
                    }
                    else
                    {
                        canscrolldown = false;
                    }
                }
                CurrentContext.Textures.White.Bind();
                CurrentContext.Rendering.SetColor(Color4.White);
                CurrentContext.Rendering.RenderRectangle(CurrentContext.Control.Width - 20, 0, CurrentContext.Control.Width, CurrentContext.Control.Height);
                int scrollpos = (int)(((float)tex_scroll / (float)yp) * (float)CurrentContext.Control.Height);
                int scrollpos2 = (int)(((float)(tex_scroll + CurrentContext.Control.Height) / (float)yp) * (float)CurrentContext.Control.Height);
                if (scrollpos2 > CurrentContext.Control.Height)
                {
                    scrollpos2 = CurrentContext.Control.Height;
                }
                CurrentContext.Rendering.SetColor(Color4.Gray);
                CurrentContext.Rendering.RenderRectangle(CurrentContext.Control.Width - 18, scrollpos, CurrentContext.Control.Width - 2, scrollpos2);
                CurrentContext.Rendering.SetColor(Color4.White);
            }
            CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlTex.Focused ? "@" : "!") + "^e^7Textures", new Location(0, 0, 0));
            glControlTex.SwapBuffers();
            texmouseDown = false;
        }

        void ResizeTex()
        {
            glControlTex.Size = splitContainer4.Panel2.ClientSize;
        }

        void ResizeTop()
        {
            glControlTop.Size = splitContainer2.Panel1.ClientSize;
        }

        void ResizeSide()
        {
            glControlSide.Size = splitContainer2.Panel2.ClientSize;
        }

        void ResizeOSide()
        {
            glControlOSide.Size = splitContainer4.Panel1.ClientSize;
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
            ResizeOSide();
        }

        // Horizontal
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeView();
            ResizeTex();
            ResizeTop();
            ResizeSide();
            ResizeOSide();
        }

        GLContext CurrentContext;

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

        private void glControlOSide_Resize(object sender, EventArgs e)
        {
            glControlOSide.MakeCurrent();
            GL.Viewport(0, 0, glControlOSide.Width, glControlOSide.Height);
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

        private void glControlSide_MouseEnter(object sender, EventArgs e)
        {
            if (!NoClickyClicky())
            {
                glControlSide.Focus();
                invalidateAll();
            }
        }

        private void glControlOSide_MouseEnter(object sender, EventArgs e)
        {
            if (!NoClickyClicky())
            {
                glControlOSide.Focus();
                invalidateAll();
            }
        }

        private void glControlTop_MouseEnter(object sender, EventArgs e)
        {
            if (!NoClickyClicky())
            {
                glControlTop.Focus();
                invalidateAll();
            }
        }

        private void glControlTex_MouseEnter(object sender, EventArgs e)
        {
            if (!NoClickyClicky())
            {
                glControlTex.Focus();
                invalidateAll();
            }
        }

        public void invalidateAll()
        {
            glControlView.Invalidate();
            glControlTex.Invalidate();
            glControlTop.Invalidate();
            glControlSide.Invalidate();
            glControlOSide.Invalidate();
        }

        bool top_selected = false;

        Location top_translate = new Location(0, 0, 0);

        Location top_mousepos = new Location(0, 0, 0);

        private void glControlTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            Location mpos = new Location((float)e.X / ((float)glControlTop.Width / 2f) - 1f, -((float)e.Y / ((float)glControlTop.Height / 2f) - 1f), 0f);
            top_mousepos = new Location(Vector3.Transform(mpos.ToOVector(), top_proj.Inverted()));
            if (top_selected)
            {
                float mx = (float)(e.X - glControlTop.Width / 2) / 25f * mouse_sens;
                float my = (float)(e.Y - glControlTop.Height / 2) / 25f * mouse_sens;
                top_translate.X -= mx;
                top_translate.Y -= my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlTop.Width / 2, this.Location.Y + 31 + menuStrip1.Height + glControlTop.Height / 2);
                }
            }
            if (top_rotate)
            {
                foreach (Entity ent in Selected)
                {
                    Location ang = Utilities.VectorToAngles(top_mousepos - ent.Position);
                    if (ent is CubeEntity || ent is ModelEntity)
                    {
                        ent.Angle.Z = ang.X;
                    }
                    else
                    {
                        ent.Angle.X = ang.X;
                    }
                }
                invalidateAll();
            }
            if (top_moving)
            {
                Location mmpos = top_mousepos;
                if (!ModifierKeys.HasFlag(Keys.Control))
                {
                    mmpos = new Location((int)top_mousepos.X, (int)top_mousepos.Y, 0);
                }
                Location vec = top_ppos - mmpos;
                top_ppos = mmpos;
                for (int i = 0; i < Selected.Count; i++)
                {
                    Selected[i].Reposition(Selected[i].Position - vec);
                }
                invalidateAll();
            }
            else if (top_stretching)
            {
                if (Selected.Count != 1)
                {
                    top_stretching = false;
                }
                else
                {
                    ((CuboidalEntity)Selected[0]).Include(new Location(ModifierKeys.HasFlag(Keys.Control) ? top_mousepos.X : (int)top_mousepos.X,
                        ModifierKeys.HasFlag(Keys.Control) ? top_mousepos.Y : (int)top_mousepos.Y, Selected[0].Position.Z));
                }
                invalidateAll();
            }
            else
            {
                glControlTop.Invalidate();
            }
        }

        bool NoClickyClicky()
        {
            return (ecf != null && ecf.Visible) || (fe != null && fe.Visible);
        }

        private void glControlTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (e.Button == MouseButtons.Middle)
            {
                top_selected = true;
                OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlTop.Width / 2, this.Location.Y + 31 + menuStrip1.Height + glControlTop.Height / 2);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Location normal;
                Entity hit = RayTraceEntity(ContextTop, new Location(top_mousepos.X, top_mousepos.Y, 1000000), new Location(top_mousepos.X, top_mousepos.Y, -1000000), out normal);
                if (hit != null)
                {
                    if (hit.Selected)
                    {
                        Deselect(hit);
                    }
                    else
                    {
                        Select(hit);
                    }
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (Selected.Count == 1 && Selected[0] is CuboidalEntity
                    && !((CuboidalEntity)Selected[0]).ContainsPoint(new Location(top_mousepos.X, top_mousepos.Y, Selected[0].Position.Z)))
                {
                    top_stretching = true;
                    CuboidalEntity ce = (CuboidalEntity)Selected[0];
                    stretch_x = top_mousepos.X > ce.Maxes.X ? 1 : (top_mousepos.X < ce.Mins.X ? -1 : 0);
                    stretch_y = top_mousepos.Y > ce.Maxes.Y ? 1 : (top_mousepos.Y < ce.Mins.Y ? -1 : 0);
                    stretch_z = 0;
                }
                else if (Selected.Count > 0)
                {
                    top_moving = true;
                }
                top_ppos = top_mousepos;
                if (!ModifierKeys.HasFlag(Keys.Control))
                {
                    top_ppos = new Location((int)top_mousepos.X, (int)top_mousepos.Y, 0);
                }
                SavePoint();
            }
        }

        bool top_stretching = false;
        public static int stretch_x = 0;
        public static int stretch_y = 0;
        public static int stretch_z = 0;

        bool top_moving = false;
        Location top_ppos = new Location(0);

        public Entity RayTraceEntity(GLContext context, Location start, Location end, out Location normal)
        {
            Location hitloc = end;
            Entity hitent = null;
            normal = new Location(0, 0, 0);
            for (int i = 0; i < Entities.Count; i++)
            {
                Location mins;
                Location maxes;
                Location hit;
                Location _normal;
                if (Entities[i] is CubeEntity)
                {
                    if (Entities[i].Angle.IsCloseTo(new Location(0, 0, 0), 0.01f))
                    {
                        mins = ((CubeEntity)Entities[i]).Mins;
                        maxes = ((CubeEntity)Entities[i]).Maxes;
                        hit = CollisionUtil.RayTraceBox(new Location(0), mins, maxes, start, hitloc, out _normal);
                    }
                    else
                    {
                        // TODO: First-pass test using collision cuboids
                        // TODO: handle scaling
                        BEPUphysics.EntityStateManagement.MotionState ms = new BEPUphysics.EntityStateManagement.MotionState();
                        ms.Position = (((CubeEntity)Entities[i]).Maxes - ((CubeEntity)Entities[i]).Mins).ToBVector() / 2 + ((CubeEntity)Entities[i]).Mins.ToBVector();
                        ms.Orientation = BEPUutilities.Quaternion.CreateFromRotationMatrix(BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(1, 0, 0), (float)(Entities[i].Angle.X * Utilities.PI180))
                            * BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 1, 0), (float)(Entities[i].Angle.Y * Utilities.PI180))
                            * BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 0, 1), (float)(Entities[i].Angle.Z * Utilities.PI180)));
                        Location size = (((CubeEntity)Entities[i]).Maxes - ((CubeEntity)Entities[i]).Mins);
                        BEPUphysics.Entities.Prefabs.Box box = new BEPUphysics.Entities.Prefabs.Box(ms, (float)size.X, (float)size.Y, (float)size.Z, 0f);
                        box.CollisionInformation.UpdateBoundingBox();
                        box.Position = ms.Position;
                        box.Orientation = ms.Orientation;
                        BEPUphysics.CollisionShapes.ConvexShapes.BoxShape boxshape = new BEPUphysics.CollisionShapes.ConvexShapes.BoxShape(0.01f, 0.01f, 0.01f);
                        BEPUutilities.RigidTransform rt = new BEPUutilities.RigidTransform(start.ToBVector());
                        BEPUutilities.Vector3 sweep = (end - start).ToBVector();
                        BEPUutilities.RayHit rh;
                        box.CollisionInformation.ConvexCast(boxshape, ref rt, ref sweep, out rh);
                        if (rh.T > 0 && rh.T < 1)
                        {
                            hit = new Location(rh.Location.X, rh.Location.Y, rh.Location.Z);
                        }
                        else
                        {
                            hit = end;
                        }
                        _normal = new Location(rh.Normal.X, rh.Normal.Y, rh.Normal.Z);
                    }
                }
                else if (Entities[i] is CuboidalEntity)
                {
                    if (Entities[i].Angle.IsCloseTo(new Location(0, 0, 0), 0.01f))
                    {
                        mins = ((CuboidalEntity)Entities[i]).Mins;
                        maxes = ((CuboidalEntity)Entities[i]).Maxes;
                        hit = CollisionUtil.RayTraceBox(new Location(0), mins, maxes, start, hitloc, out _normal);
                    }
                    else
                    {
                        // TODO: First-pass test using collision cuboids
                        // TODO: handle scaling
                        BEPUphysics.EntityStateManagement.MotionState ms = new BEPUphysics.EntityStateManagement.MotionState();
                        ms.Position = (((CuboidalEntity)Entities[i]).Maxes - ((CuboidalEntity)Entities[i]).Mins).ToBVector() / 2 + ((CuboidalEntity)Entities[i]).Mins.ToBVector();
                        ms.Orientation = BEPUutilities.Quaternion.CreateFromRotationMatrix(BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(1, 0, 0), (float)(Entities[i].Angle.X * Utilities.PI180))
                            * BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 1, 0), (float)(Entities[i].Angle.Y * Utilities.PI180))
                            * BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 0, 1), (float)(Entities[i].Angle.Z * Utilities.PI180)));
                        Location size = (((CuboidalEntity)Entities[i]).Maxes - ((CuboidalEntity)Entities[i]).Mins);
                        BEPUphysics.Entities.Prefabs.Box box = new BEPUphysics.Entities.Prefabs.Box(ms, (float)size.X, (float)size.Y, (float)size.Z, 0f);
                        box.CollisionInformation.UpdateBoundingBox();
                        box.Position = ms.Position;
                        box.Orientation = ms.Orientation;
                        BEPUphysics.CollisionShapes.ConvexShapes.BoxShape boxshape = new BEPUphysics.CollisionShapes.ConvexShapes.BoxShape(0.01f, 0.01f, 0.01f);
                        BEPUutilities.RigidTransform rt = new BEPUutilities.RigidTransform(start.ToBVector());
                        BEPUutilities.Vector3 sweep = (end - start).ToBVector();
                        BEPUutilities.RayHit rh;
                        box.CollisionInformation.ConvexCast(boxshape, ref rt, ref sweep, out rh);
                        if (rh.T > 0 && rh.T < 1)
                        {
                            hit = new Location(rh.Location.X, rh.Location.Y, rh.Location.Z);
                        }
                        else
                        {
                            hit = end;
                        }
                        _normal = new Location(rh.Normal.X, rh.Normal.Y, rh.Normal.Z);
                    }
                }
                else if (Entities[i] is ModelEntity)
                {
                    BEPUphysics.EntityStateManagement.MotionState ms = new BEPUphysics.EntityStateManagement.MotionState();
                    ms.Position = Entities[i].Position.ToBVector();
                    ms.Orientation = BEPUutilities.Quaternion.CreateFromRotationMatrix(BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(1, 0, 0), (float)(Entities[i].Angle.X * Utilities.PI180))
                        * BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 1, 0), (float)(Entities[i].Angle.Y * Utilities.PI180))
                        * BEPUutilities.Matrix3x3.CreateFromAxisAngle(new BEPUutilities.Vector3(0, 0, 1), (float)(Entities[i].Angle.Z * Utilities.PI180)));
                    BEPUphysics.Entities.Prefabs.MobileMesh mesh = context.Models.Handler.MeshToBepu(context.Models.GetModel(((ModelEntity)Entities[i]).model).OriginalModel);
                    mesh.Position = ms.Position + mesh.Position;
                    mesh.Orientation = ms.Orientation;
                    mesh.CollisionInformation.UpdateBoundingBox();
                    BEPUphysics.CollisionShapes.ConvexShapes.BoxShape boxshape = new BEPUphysics.CollisionShapes.ConvexShapes.BoxShape(0.01f, 0.01f, 0.01f);
                    BEPUutilities.RigidTransform rt = new BEPUutilities.RigidTransform(start.ToBVector());
                    BEPUutilities.Vector3 sweep = (end - start).ToBVector();
                    BEPUutilities.RayHit rh;
                    mesh.CollisionInformation.ConvexCast(boxshape, ref rt, ref sweep, out rh);
                    if (rh.T > 0 && rh.T < 1)
                    {
                        hit = new Location(rh.Location.X, rh.Location.Y, rh.Location.Z);
                    }
                    else
                    {
                        hit = end;
                    }
                    _normal = new Location(rh.Normal.X, rh.Normal.Y, rh.Normal.Z);
                }
                else
                {
                    mins = Entities[i].GetMins();
                    maxes = Entities[i].GetMaxes();
                    hit = CollisionUtil.RayTraceBox(Entities[i].Position, mins, maxes, start, hitloc, out _normal);
                }
                if ((hit - start).LengthSquared() < (hitloc - start).LengthSquared())
                {
                    hitent = Entities[i];
                    hitloc = hit;
                    normal = _normal;
                }
            }
            return hitent;
        }

        private void PrimaryEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                top_selected = false;
                side_selected = false;
                oside_selected = false;
            }
            else if (e.Button == MouseButtons.Left)
            {
                top_moving = false;
                top_stretching = false;
                side_moving = false;
                side_stretching = false;
                oside_moving = false;
                oside_stretching = false;
            }
        }

        private void glControlTop_MouseUp(object sender, MouseEventArgs e)
        {
            PrimaryEditor_MouseUp(sender, e);
        }

        bool side_selected = false;

        Location side_translate = new Location(0, 0, 0);

        Location side_mousepos = new Location(0, 0, 0);

        bool side_stretching = false;

        bool side_moving = false;
        Location side_ppos = new Location(0);

        private void glControlSide_MouseMove(object sender, MouseEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            Location mpos = new Location((float)e.X / ((float)glControlSide.Width / 2f) - 1f, -((float)e.Y / ((float)glControlSide.Height / 2f) - 1f), 0f);
            side_mousepos = new Location(Vector3.Transform(mpos.ToOVector(), side_proj.Inverted()));
            if (side_selected)
            {
                float mx = (float)(e.X - glControlSide.Width / 2) / 25f * mouse_sens;
                float my = (float)(e.Y - glControlSide.Height / 2) / 25f * mouse_sens;
                side_translate.X += mx;
                side_translate.Y -= my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    Point p = glControlSide.PointToScreen(new Point(glControlSide.Width / 2, glControlSide.Height / 2));
                    OpenTK.Input.Mouse.SetPosition(p.X, p.Y);
                }
            }
            if (side_rotate)
            {
                foreach (Entity ent in Selected)
                {
                    Location ang = Utilities.VectorToAngles(new Location(side_mousepos.X, side_mousepos.Z, side_mousepos.Y) - new Location(ent.Position.X, ent.Position.Z, ent.Position.Y));
                    if (ent is CubeEntity || ent is ModelEntity)
                    {
                        ent.Angle.Y = -ang.X;
                    }
                    else
                    {
                        ent.Angle.Y = -ang.X;
                    }
                }
                invalidateAll();
            }
            if (side_moving)
            {
                Location mmpos = side_mousepos;
                if (!ModifierKeys.HasFlag(Keys.Control))
                {
                    mmpos = new Location((int)side_mousepos.X, 0, (int)side_mousepos.Z);
                }
                Location vec = side_ppos - mmpos;
                side_ppos = mmpos;
                for (int i = 0; i < Selected.Count; i++)
                {
                    Selected[i].Reposition(Selected[i].Position - vec);
                }
                invalidateAll();
            }
            else if (side_stretching)
            {
                if (Selected.Count != 1)
                {
                    side_stretching = false;
                }
                else
                {
                    ((CuboidalEntity)Selected[0]).Include(new Location(ModifierKeys.HasFlag(Keys.Control) ? side_mousepos.X : (int)side_mousepos.X,
                        Selected[0].Position.Y, ModifierKeys.HasFlag(Keys.Control) ? side_mousepos.Z : (int)side_mousepos.Z));
                }
                invalidateAll();
            }
            else
            {
                glControlSide.Invalidate();
            }
        }

        private void glControlSide_MouseUp(object sender, MouseEventArgs e)
        {
            PrimaryEditor_MouseUp(sender, e);
        }

        private void glControlSide_MouseDown(object sender, MouseEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (e.Button == MouseButtons.Middle)
            {
                side_selected = true;
                OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlSide.Width / 2,
                    this.Location.Y + 31 + menuStrip1.Height + splitContainer2.SplitterDistance + splitContainer2.SplitterRectangle.Height + glControlSide.Height / 2);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Location normal;
                Entity hit = RayTraceEntity(ContextSide, new Location(side_mousepos.X, 1000000, side_mousepos.Z), new Location(side_mousepos.X, -1000000, side_mousepos.Z), out normal);
                if (hit != null)
                {
                    if (hit.Selected)
                    {
                        Deselect(hit);
                    }
                    else
                    {
                        Select(hit);
                    }
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (Selected.Count == 1 && Selected[0] is CuboidalEntity
                    && !((CuboidalEntity)Selected[0]).ContainsPoint(new Location(side_mousepos.X, Selected[0].Position.Y, side_mousepos.Z)))
                {
                    side_stretching = true;
                    CuboidalEntity ce = (CuboidalEntity)Selected[0];
                    stretch_x = side_mousepos.X > ce.Maxes.X ? 1 : (side_mousepos.X < ce.Mins.X ? -1 : 0);
                    stretch_y = 0;
                    stretch_z = side_mousepos.Z > ce.Maxes.Z ? 1 : (side_mousepos.Z < ce.Mins.Z ? -1 : 0);
                }
                else if (Selected.Count > 0)
                {
                    side_moving = true;
                }
                side_ppos = side_mousepos;
                if (!ModifierKeys.HasFlag(Keys.Control))
                {
                    side_ppos = new Location((int)side_mousepos.X, 0, (int)side_mousepos.Z);
                }
                SavePoint();
            }
        }

        bool oside_selected = false;

        Location oside_translate = new Location(0, 0, 0);

        Location oside_mousepos = new Location(0, 0, 0);

        bool oside_stretching = false;

        bool oside_moving = false;
        Location oside_ppos = new Location(0);

        private void glControlOSide_MouseMove(object sender, MouseEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            Location mpos = new Location((float)e.X / ((float)glControlOSide.Width / 2f) - 1f, -((float)e.Y / ((float)glControlOSide.Height / 2f) - 1f), 0f);
            oside_mousepos = new Location(Vector3.Transform(mpos.ToOVector(), oside_proj.Inverted()));
            if (oside_selected)
            {
                float mx = (float)(e.X - glControlOSide.Width / 2) / 25f * mouse_sens;
                float my = (float)(e.Y - glControlOSide.Height / 2) / 25f * mouse_sens;
                oside_translate.X += mx;
                oside_translate.Y -= my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    Point p = glControlOSide.PointToScreen(new Point(glControlOSide.Width / 2, glControlOSide.Height / 2));
                    OpenTK.Input.Mouse.SetPosition(p.X, p.Y);
                }
            }
            if (oside_rotate)
            {
                foreach (Entity ent in Selected)
                {
                    Location ang = Utilities.VectorToAngles(new Location(oside_mousepos.Z, oside_mousepos.Y, oside_mousepos.X) - new Location(ent.Position.Z, ent.Position.Y, ent.Position.X));
                    if (ent is CubeEntity || ent is ModelEntity)
                    {
                        ent.Angle.X = -ang.X;
                    }
                    else
                    {
                        ent.Angle.Y = -ang.X;
                    }
                }
                invalidateAll();
            }
            if (oside_moving)
            {
                Location mmpos = oside_mousepos;
                if (!ModifierKeys.HasFlag(Keys.Control))
                {
                    mmpos = new Location(0, (int)oside_mousepos.Y, (int)oside_mousepos.Z);
                }
                Location vec = oside_ppos - mmpos;
                oside_ppos = mmpos;
                for (int i = 0; i < Selected.Count; i++)
                {
                    Selected[i].Reposition(Selected[i].Position - vec);
                }
                invalidateAll();
            }
            else if (oside_stretching)
            {
                if (Selected.Count != 1)
                {
                    oside_stretching = false;
                }
                else
                {
                    ((CuboidalEntity)Selected[0]).Include(new Location(Selected[0].Position.X, ModifierKeys.HasFlag(Keys.Control) ?
                        oside_mousepos.Y : (int)oside_mousepos.Y, ModifierKeys.HasFlag(Keys.Control) ? oside_mousepos.Z : (int)oside_mousepos.Z));
                }
                invalidateAll();
            }
            else
            {
                glControlOSide.Invalidate();
            }
        }

        private void glControlOSide_MouseUp(object sender, MouseEventArgs e)
        {
            PrimaryEditor_MouseUp(sender, e);
        }

        private void glControlOSide_MouseDown(object sender, MouseEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (e.Button == MouseButtons.Middle)
            {
                oside_selected = true;
                OpenTK.Input.Mouse.SetPosition(this.Location.X + splitContainer1.SplitterDistance + splitContainer1.SplitterRectangle.Width + 8 + glControlOSide.Width / 2,
                    this.Location.Y + 31 + menuStrip1.Height + splitContainer3.SplitterDistance + splitContainer3.SplitterRectangle.Height + glControlOSide.Height / 2);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Location normal;
                Entity hit = RayTraceEntity(ContextOSide, new Location(1000000, oside_mousepos.Y, oside_mousepos.Z), new Location(-1000000, oside_mousepos.Y, oside_mousepos.Z), out normal);
                if (hit != null)
                {
                    if (hit.Selected)
                    {
                        Deselect(hit);
                    }
                    else
                    {
                        Select(hit);
                    }
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (Selected.Count == 1 && Selected[0] is CuboidalEntity
                    && !((CuboidalEntity)Selected[0]).ContainsPoint(new Location(Selected[0].Position.X, oside_mousepos.Y, oside_mousepos.Z)))
                {
                    oside_stretching = true;
                    CuboidalEntity ce = (CuboidalEntity)Selected[0];
                    stretch_x = 0;
                    stretch_y = oside_mousepos.Y > ce.Maxes.Y ? 1 : (oside_mousepos.Y < ce.Mins.Y ? -1 : 0);
                    stretch_z = oside_mousepos.Z > ce.Maxes.Z ? 1 : (oside_mousepos.Z < ce.Mins.Z ? -1 : 0);
                }
                else if (Selected.Count > 0)
                {
                    oside_moving = true;
                }
                oside_ppos = oside_mousepos;
                if (!ModifierKeys.HasFlag(Keys.Control))
                {
                    oside_ppos = new Location(0, (int)oside_mousepos.Y, (int)oside_mousepos.Z);
                }
                SavePoint();
            }
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Filter = "(*.map)|*.map";
                ofd.InitialDirectory = Environment.CurrentDirectory;
                ofd.Multiselect = false;
                ofd.ShowReadOnly = false;
                ofd.Title = "Select a map file...";
                ofd.ValidateNames = true;
                DialogResult dr = ofd.ShowDialog();
                if (dr.HasFlag(DialogResult.OK) || dr.HasFlag(DialogResult.Yes))
                {
                    string f = ofd.FileName;
                    if (!File.Exists(f))
                    {
                        throw new Exception("Invalid file!");
                    }
                    else
                    {
                        SetFile(f);
                        string data = File.ReadAllText(f);
                        ClearMap();
                        LoadMap(data);
                        PickCameraSpawn();
                        SysConsole.Output(OutputType.INFO, "Finished loading " + f + "!");
                    }
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
        }

        public void ClearMap()
        {
            List<Entity> ents = new List<Entity>(Entities);
            foreach (Entity ent in ents)
            {
                Despawn(ent);
            }
        }

        public void LoadMap(string data)
        {
            data = data.Replace('\r', '\n').Replace('\t', '\n').Replace("\n", "");
            int start = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '{')
                {
                    string objo = data.Substring(start, i - start).Trim();
                    int obj_start = i + 1;
                    for (int x = i + 1; x < data.Length; x++)
                    {
                        if (data[x] == '}')
                        {
                            try
                            {
                                string objdat = data.Substring(obj_start, x - obj_start);
                                LoadObj(objo, objdat);
                            }
                            catch (Exception ex)
                            {
                                SysConsole.Output(OutputType.ERROR, "Invalid entity " + objo + ": " + ex.ToString());
                            }
                            i = x;
                            break;
                        }
                    }
                    start = i + 1;
                }
            }
        }

        public Location ambient;
        public string music;

        public void LoadObj(string name, string dat)
        {
            string[] dats = dat.Split(';');
            if (name == "general")
            {
                for (int i = 0; i < dats.Length; i++)
                {
                    if (dats[i].Length <= 0)
                    {
                        continue;
                    }
                    string trimmed = dats[i].Trim();
                    if (trimmed.Length == 0)
                    {
                        continue;
                    }
                    string[] datum = trimmed.Split(':');
                    if (datum.Length != 2)
                    {
                        throw new Exception("Invalid key '" + dats[i] + "'!");
                    }
                    switch (datum[0])
                    {
                        case "ambient":
                            ambient = Utilities.StringToLocation(datum[1]);
                            break;
                        case "music":
                            music = datum[1];
                            break;
                        default:
                            throw new Exception("Invalid key: " + datum[0].Trim() + "!");
                    }
                }
                return;
            }
            Entity e = GetForType(name.ToLower());
            for (int i = 0; i < dats.Length; i++)
            {
                if (dats[i].Length <= 0)
                {
                    continue;
                }
                string trimmed = dats[i].Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }
                string[] datum = trimmed.Split(':');
                if (datum.Length != 2)
                {
                    throw new Exception("Invalid key '" + dats[i] + "'!");
                }
                if (!e.ApplyVar(datum[0].Trim(), datum[1].Trim()))
                {
                    throw new Exception("Invalid key: " + datum[0].Trim() + "!");
                }
            }
            e.Recalculate();
            Spawn(e);
        }

        public string file = null;

        public void SetFile(string filename)
        {
            file = filename;
            this.Text = Program.GameName + " | " + file;
        }


        public bool PickFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Environment.CurrentDirectory;
            sfd.SupportMultiDottedExtensions = true;
            sfd.ValidateNames = true;
            sfd.Title = "Pick a file to save to...";
            sfd.OverwritePrompt = true;
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.Filter = "(*.map)|*.map";
            DialogResult dr = sfd.ShowDialog();
            if (dr.HasFlag(DialogResult.OK) || dr.HasFlag(DialogResult.Yes))
            {
                SetFile(sfd.FileName);
                return true;
            }
            return false;
        }

        public void SaveToFile()
        {
            try
            {
                File.WriteAllText(file, SaveToString());
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
        }

        public string SaveToString()
        {
            StringBuilder sb = new StringBuilder(Entities.Count * 200);
            sb.Append("general\n");
            sb.Append("{\n");
            sb.Append("\tambient: ").Append(ambient.ToString()).Append(";\n");
            sb.Append("\tmusic: ").Append(music).Append(";\n");
            sb.Append("}\n");
            for (int i = 0; i < Entities.Count; i++)
            {
                sb.Append(Entities[i].GetEntityType()).Append("\n");
                sb.Append("{\n");
                List<KeyValuePair<string, string>> vars = Entities[i].GetVars();
                for (int v = 0; v < vars.Count; v++)
                {
                    sb.Append("\t").Append(vars[v].Key).Append(": ").Append(vars[v].Value).Append(";\n");
                }
                sb.Append("}\n");
            }
            return sb.ToString();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (file == null)
            {
                if (!PickFile())
                {
                    return;
                }
            }
            SaveToFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (!PickFile())
            {
                return;
            }
            SaveToFile();
        }

        private void renderLightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            renderLightingToolStripMenuItem.Checked = !renderLightingToolStripMenuItem.Checked;
        }

        private void PrimaryEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                List<Entity> ents = new List<Entity>(Selected);
                for (int i = 0; i < ents.Count; i++)
                {
                    Despawn(ents[i]);
                }
                invalidateAll();
            }
            else if (e.KeyCode == Keys.T && Selected.Count == 1)
            {
                e.Handled = true;
                entityTypeChooser.Show(glControlTop, 0, 0);
            }
            else if (e.KeyCode == Keys.N && Selected.Count == 1)
            {
                SavePoint();
                ecf = new EntityControlForm(Selected[0]);
                view_selected = false;
                top_selected = false;
                side_selected = false;
                oside_selected = false;
                ecf.Show();
            }
            else if (e.KeyCode == Keys.Escape && Selected.Count > 0)
            {
                List<Entity> ents = new List<Entity>(Selected);
                for (int i = 0; i < ents.Count; i++)
                {
                    Deselect(ents[i]);
                }
            }
            else if (ModifierKeys.HasFlag(Keys.Control) && e.KeyCode == Keys.A)
            {
                bool val = Selected.Count != Entities.Count;
                List<Entity> ents = new List<Entity>(Selected);
                for (int i = 0; i < ents.Count; i++)
                {
                    Deselect(ents[i]);
                }
                if (val)
                {
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Select(Entities[i]);
                    }
                }
                invalidateAll();
            }
            else if (ModifierKeys.HasFlag(Keys.Control) && e.KeyCode == Keys.X)
            {
                SavePoint();
                Clipboard = new List<ClipboardEntity>();
                for (int i = 0; i < Selected.Count; i++)
                {
                    ClipboardEntity cbe = new ClipboardEntity();
                    cbe.entitytype = Selected[i].GetEntityType();
                    cbe.variables = Selected[i].GetVars();
                    Clipboard.Add(cbe);
                }
                List<Entity> ents = new List<Entity>(Selected);
                for (int i = 0; i < ents.Count; i++)
                {
                    Despawn(ents[i]);
                }
                invalidateAll();
            }
            else if (ModifierKeys.HasFlag(Keys.Control) && e.KeyCode == Keys.C)
            {
                Clipboard = new List<ClipboardEntity>();
                for (int i = 0; i < Selected.Count; i++)
                {
                    ClipboardEntity cbe = new ClipboardEntity();
                    cbe.entitytype = Selected[i].GetEntityType();
                    cbe.variables = Selected[i].GetVars();
                    cbe.color = null;
                    Clipboard.Add(cbe);
                }
            }
            else if (ModifierKeys.HasFlag(Keys.Control) && e.KeyCode == Keys.V)
            {
                SavePoint();
                List<Entity> ents = new List<Entity>(Selected);
                for (int i = 0; i < ents.Count; i++)
                {
                    Deselect(ents[i]);
                }
                foreach (ClipboardEntity ent in Clipboard)
                {
                    Entity et = GetForType(ent.entitytype.ToLower());
                    foreach (KeyValuePair<string, string> val in ent.variables)
                    {
                        et.ApplyVar(val.Key, val.Value);
                    }
                    if (ent.color != null && ent.color.HasValue)
                    {
                        et.ViewColor = ent.color.Value;
                    }
                    et.Recalculate();
                    Spawn(et);
                    Select(et);
                }
                invalidateAll();
            }
            else if (ModifierKeys.HasFlag(Keys.Control) && e.KeyCode == Keys.Z)
            {
                if (History.Count > 0)
                {
                    List<Entity> ents = new List<Entity>(Entities);
                    for (int i = 0; i < ents.Count; i++)
                    {
                        Despawn(ents[i]);
                    }
                    foreach (ClipboardEntity ent in History[History.Count - 1])
                    {
                        Entity et = GetForType(ent.entitytype.ToLower());
                        foreach (KeyValuePair<string, string> val in ent.variables)
                        {
                            et.ApplyVar(val.Key, val.Value);
                        }
                        if (ent.color != null && ent.color.HasValue)
                        {
                            et.ViewColor = ent.color.Value;
                        }
                        et.Recalculate();
                        Spawn(et);
                    }
                    invalidateAll();
                    History.RemoveAt(History.Count - 1);
                }
            }
        }

        bool top_rotate = false;

        private void glControlTop_KeyDown(object sender, KeyEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (e.KeyCode == Keys.Space)
            {
                MakeCuboidAt(new Location(top_mousepos.X, top_mousepos.Y, 0));
            }
            else if (e.KeyCode == Keys.R)
            {
                top_rotate = true;
            }
            else if (e.KeyCode == Keys.W)
            {
                top_translate.Y -= 10f / top_zoom;
                glControlTop.Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                top_translate.Y += 10f / top_zoom;
                glControlTop.Invalidate();
            }
            else if (e.KeyCode == Keys.A)
            {
                top_translate.X -= 10f / top_zoom;
                glControlTop.Invalidate();
            }
            else if (e.KeyCode == Keys.D)
            {
                top_translate.X += 10f / top_zoom;
                glControlTop.Invalidate();
            }
            else if (e.KeyCode == Keys.Up)
            {
                top_translate.Y -= 10f;
                glControlTop.Invalidate();
            }
            else if (e.KeyCode == Keys.Down)
            {
                top_translate.Y += 10f;
                glControlTop.Invalidate();
            }
            else if (e.KeyCode == Keys.Left)
            {
                top_translate.X -= 10f;
                glControlTop.Invalidate();
            }
            else if (e.KeyCode == Keys.Right)
            {
                top_translate.X += 10f;
                glControlTop.Invalidate();
            }
            PrimaryEditor_KeyDown(sender, e);
        }

        public void MakeCuboidAt(Location spot)
        {
            spot = new Location((int)spot.X, (int)spot.Y, (int)spot.Z);
            CubeEntity ce = new CubeEntity(spot - new Location(1), spot + new Location(1));
            string tex = ContextView.Textures.LoadedTextures[tex_sel].Name;
            for (int i = 0; i < 6; i++)
            {
                ce.Textures[i] = tex;
            }
            ce.Recalculate();
            List<Entity> es = new List<Entity>(Selected);
            for (int i = 0; i < es.Count; i++)
            {
                Deselect(es[i]);
            }
            Spawn(ce);
            Select(ce);
            invalidateAll();
        }

        bool side_rotate = false;

        private void glControlSide_KeyDown(object sender, KeyEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (e.KeyCode == Keys.Space)
            {
                MakeCuboidAt(new Location(side_mousepos.X, 0, side_mousepos.Z));
            }
            else if (e.KeyCode == Keys.R)
            {
                side_rotate = true;
            }
            else if (e.KeyCode == Keys.W)
            {
                side_translate.Y -= 10f / side_zoom;
                glControlSide.Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                side_translate.Y += 10f / side_zoom;
                glControlSide.Invalidate();
            }
            else if (e.KeyCode == Keys.A)
            {
                side_translate.X += 10f / side_zoom;
                glControlSide.Invalidate();
            }
            else if (e.KeyCode == Keys.D)
            {
                side_translate.X -= 10f / side_zoom;
                glControlSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Up)
            {
                side_translate.Y -= 10f;
                glControlSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Down)
            {
                side_translate.Y += 10f;
                glControlSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Left)
            {
                side_translate.X += 10f;
                glControlSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Right)
            {
                side_translate.X -= 10f;
                glControlSide.Invalidate();
            }
            PrimaryEditor_KeyDown(sender, e);
        }

        bool oside_rotate = false;

        private void glControlOSide_KeyDown(object sender, KeyEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            if (e.KeyCode == Keys.Space)
            {
                MakeCuboidAt(new Location(0, oside_mousepos.Y, oside_mousepos.Z));
            }
            else if (e.KeyCode == Keys.R)
            {
                oside_rotate = true;
            }
            else if (e.KeyCode == Keys.W)
            {
                oside_translate.Y -= 10f / oside_zoom;
                glControlOSide.Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                oside_translate.Y += 10f / oside_zoom;
                glControlOSide.Invalidate();
            }
            else if (e.KeyCode == Keys.A)
            {
                oside_translate.X += 10f / oside_zoom;
                glControlOSide.Invalidate();
            }
            else if (e.KeyCode == Keys.D)
            {
                oside_translate.X -= 10f / oside_zoom;
                glControlOSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Up)
            {
                oside_translate.Y -= 10f;
                glControlOSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Down)
            {
                oside_translate.Y += 10f;
                glControlOSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Left)
            {
                oside_translate.X += 10f;
                glControlOSide.Invalidate();
            }
            else if (e.KeyCode == Keys.Right)
            {
                oside_translate.X -= 10f;
                glControlOSide.Invalidate();
            }
            PrimaryEditor_KeyDown(sender, e);
        }

        private void PrimaryEditor_KeyUp(object sender, KeyEventArgs e)
        {
            top_rotate = false;
            side_rotate = false;
            oside_rotate = false;
        }

        private void glControlTop_KeyUp(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyUp(sender, e);
        }

        private void glControlSide_KeyUp(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyUp(sender, e);
        }

        private void glControlOSide_KeyUp(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyUp(sender, e);
        }

        private void glControlTex_KeyUp(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyUp(sender, e);
        }

        private void glControlTex_KeyDown(object sender, KeyEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            PrimaryEditor_KeyDown(sender, e);
        }

        private void PrimaryEditor_Resize(object sender, EventArgs e)
        {
        }

        private void glControlTex_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        bool texmouseDown = false;

        Location texMPos = new Location(0, 0, 0);

        private void glControlTex_MouseDown(object sender, MouseEventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            texmouseDown = true;
            glControlTex.Invalidate();
            texMPos = new Location(e.X, e.Y, 0);
        }

        private void glControlTex_MouseUp(object sender, MouseEventArgs e)
        {
            texmouseDown = false;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            List<Entity> ents = new List<Entity>(Entities);
            for (int i = 0; i < ents.Count; i++)
            {
                Despawn(ents[i]);
            }
            LoadEntities();
        }

        private void tODOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NoClickyClicky())
            {
                return;
            }
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Filter = "(*.png)|*.png";
                ofd.InitialDirectory = Environment.CurrentDirectory;
                ofd.Multiselect = true;
                ofd.ShowReadOnly = false;
                ofd.Title = "Select texture files...";
                ofd.ValidateNames = true;
                DialogResult dr = ofd.ShowDialog();
                if (dr.HasFlag(DialogResult.OK) || dr.HasFlag(DialogResult.Yes))
                {
                    string[] fs = ofd.FileNames;
                    ContextView.Control.MakeCurrent();
                    foreach (string f in fs)
                    {
                        string nf = f.Replace("\\", "/");
                        nf = nf.Replace(FileHandler.BaseDirectory, "");
                        if (nf.StartsWith("/"))
                        {
                            nf = nf.Substring(1);
                        }
                        if (nf.StartsWith("textures/"))
                        {
                            nf = nf.Substring("textures/".Length);
                            nf = nf.Substring(0, nf.Length - (".png").Length);
                            ContextView.Textures.GetTexture(nf);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
            invalidateAll();
        }

        private void splitContainer4_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer4_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeTex();
            ResizeOSide();
        }

        public bool wireframe = false;

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wireframe = !wireframe;
            wireframeToolStripMenuItem.Checked = wireframe;
        }

        public static bool autoStretch = false;

        private void textureAutoStretchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoStretch = !autoStretch;
            textureAutoStretchToolStripMenuItem.Checked = autoStretch;
        }
    }

    class MyRenderer : ToolStripProfessionalRenderer
    {
        public MyRenderer() : base(new MyColors()) {}
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
