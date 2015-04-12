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

namespace OpenTKMapMaker
{
    public partial class PrimaryEditor : Form
    {
        public static Matrix4 ortho;

        public static int vpw;
        public static int vph;

        public static List<Entity> Entities = new List<Entity>();

        public static List<Entity> Selected = new List<Entity>();

        public ContextMenuStrip entityTypeChooser;

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

        public static List<GLContext> Contexts = new List<GLContext>();

        public float mouse_sens = 5.0f;

        public void LoadEntities()
        {
            CubeEntity ce = new CubeEntity(new Location(-100, -100, -10), new Location(100, 100, 0));
            ce.Recalculate();
            Spawn(ce);
            ce = new CubeEntity(new Location(-10, -10, 0), new Location(10, 10, 10));
            ce.Recalculate();
            Spawn(ce);
            Spawn(new PointLightEntity(new Location(0, 0, 30), 100, new Location(1f, 1f, 1f), true));
            Spawn(new SpawnPointEntity(new Location(0, 0, 10)));

            entityTypeChooser = new ContextMenuStrip();
            entityTypeChooser.Items.Add("Cuboid");
            entityTypeChooser.Items.Add("Light");
            entityTypeChooser.Items.Add("Spawnpoint");
            entityTypeChooser.Items.Add("Cancel.");
            entityTypeChooser.ItemClicked += new ToolStripItemClickedEventHandler(entityTypeChooser_ItemClicked);
            entityTypeChooser.CreateControl();
        }

        void entityTypeChooser_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (Selected.Count != 1)
            {
                return;
            }
            Entity sel = Selected[0];
            Entity ent;
            // TODO:n Abstractify this switch into a registry. Also abstractify everything entity related.
            switch (e.ClickedItem.Text)
            {
                case "Cuboid":
                    ent = new CubeEntity(sel.Position - new Location(1), sel.Position + new Location(1));
                    break;
                case "Light":
                    ent = new PointLightEntity(sel.Position, 50, new Location(1, 1, 1), false);
                    break;
                case "Spawnpoint":
                    ent = new SpawnPointEntity(sel.Position);
                    break;
                default:
                    return;
            }
            ent.Recalculate();
            Spawn(ent);
            Despawn(sel);
            Select(ent);
        }

        public PrimaryEditor()
        {
            InitializeComponent();
            SysConsole.Init();
            this.FormClosed += new FormClosedEventHandler(PrimaryEditor_FormClosed);
            menuStrip1.Renderer = new MyRenderer();
            PickCameraSpawn();
            glControlView.MouseWheel += new MouseEventHandler(glControlView_MouseWheel);
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

        public static bool RenderEntities = false;

        public static bool RenderLines = false;

        public static bool RenderTextures = false;

        public void Render3D(GLContext context, bool render_entities, bool render_lines, bool render_textures)
        {
            RenderLines = render_lines;
            RenderEntities = render_entities;
            RenderTextures = render_textures;
            for (int i = 0; i < Entities.Count; i++)
            {
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
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Selected)
                {
                    Entities[i].Render(context);
                }
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
                GL.Viewport(0, 0, context.Control.Width, context.Control.Height);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);
                GL.Enable(EnableCap.Multisample); // TODO: Actually use multi-sampling
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "InitGL: " + ex.ToString());
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
            GL.Disable(EnableCap.DepthTest);
        }

        Matrix4 top_proj;

        private void glControlTop_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                CurrentContext = ContextTop;
                glControlTop.MakeCurrent();
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
                ortho = Matrix4.CreateOrthographicOffCenter(-500f / top_zoom + (float)top_translate.X, 500f / top_zoom + (float)top_translate.X,
                    500f / top_zoom + (float)top_translate.Y, -500f / top_zoom + (float)top_translate.Y, -1000000, 1000000);
                top_proj = ortho;
                GL.UniformMatrix4(1, false, ref ortho);
                Render3D(CurrentContext, true, true, false);
                renderSelections(CurrentContext, false);
                ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
                GL.Enable(EnableCap.Texture2D);
                CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlTop.Focused ? "@": "!") + "^e^7" + top_zoom.ToString(), new Location(0, 0, 0));
                GL.Disable(EnableCap.Texture2D);
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
            GL.Disable(EnableCap.DepthTest);
        }

        Matrix4 side_proj;

        private void glControlSide_Paint(object sender, PaintEventArgs e)
        {
            CurrentContext = ContextSide;
            glControlSide.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1));
            ortho = view * Matrix4.CreateOrthographicOffCenter(-500f / side_zoom + (float)side_translate.X, 500f / side_zoom + (float)side_translate.X,
                500f / side_zoom + (float)side_translate.Y, -500f / side_zoom + (float)side_translate.Y, -1000000, 1000000) * Matrix4.CreateScale(-1, 1, 1);
            side_proj = ortho;
            GL.UniformMatrix4(1, false, ref ortho);
            Render3D(CurrentContext, true, true, false);
            renderSelections(CurrentContext, false);
            ortho = Matrix4.CreateOrthographicOffCenter(0, CurrentContext.Control.Width, CurrentContext.Control.Height, 0, -1, 1);
            GL.Enable(EnableCap.Texture2D);
            CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlSide.Focused ? "@" : "!") + "^e^7" + side_zoom.ToString(), new Location(0, 0, 0));
            GL.Disable(EnableCap.Texture2D);
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
            GL.Enable(EnableCap.Texture2D);
            if (ContextView != null)
            {
                for (int i = 0; i < ContextView.Textures.LoadedTextures.Count; i++)
                {
                    ContextTex.Textures.GetTexture(ContextView.Textures.LoadedTextures[i].Name);
                }
            }
        }

        int tex_scroll = 0;

        void ViewTextures_OnTextureLoaded(object sender, TextureLoadedEventArgs e)
        {
            if (ContextTex == null)
            {
                return;
            }
            ContextTex.Control.MakeCurrent();
            ContextView.Textures.GetTexture(e.Tex.Name);
            ContextView.Control.MakeCurrent();
        }

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
                    if (yp + 138 > tex_scroll)
                    {
                        t.Bind();
                        CurrentContext.Rendering.RenderRectangle(xp, yp - tex_scroll, xp + 128, yp + 128 - tex_scroll);
                        StringBuilder tname = new StringBuilder(t.Name.Length);
                        int start = 0;
                        if (t.Name.Length > 0)
                        {
                            tname.Append(t.Name[0]);
                        }
                        for (int x = 1; x < t.Name.Length; x++)
                        {
                            if (CurrentContext.FontSets.SlightlyBigger.MeasureFancyText("^S" + t.Name.Substring(start, x - start)) >= 128)
                            {
                                tname.Append("\n");
                                start = x;
                            }
                            tname.Append(t.Name[x]);
                        }
                        CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^!^e^7" + tname, new Location(xp, yp - tex_scroll, 0));
                    }
                    xp += 138;
                    if (xp + 148 > CurrentContext.Control.Width)
                    {
                        yp += 138;
                        xp = 5;
                    }
                    if (yp > CurrentContext.Control.Height)
                    {
                        break;
                    }
                }
            }
            CurrentContext.FontSets.SlightlyBigger.DrawColoredText("^S^" + (glControlTex.Focused ? "@" : "!") + "^e^7Textures", new Location(0, 0, 0));
            glControlTex.SwapBuffers();
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

        public void invalidateAll()
        {
            glControlView.Invalidate();
            glControlTex.Invalidate();
            glControlTop.Invalidate();
            glControlSide.Invalidate();
        }

        bool top_selected = false;

        Location top_translate = new Location(0, 0, 0);

        Location top_mousepos = new Location(0, 0, 0);

        private void glControlTop_MouseMove(object sender, MouseEventArgs e)
        {
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
                    ((CubeEntity)Selected[0]).Include(new Location(ModifierKeys.HasFlag(Keys.Control) ? top_mousepos.X : (int)top_mousepos.X,
                        ModifierKeys.HasFlag(Keys.Control) ? top_mousepos.Y : (int)top_mousepos.Y, Selected[0].Position.Z));
                }
                invalidateAll();
            }
            else
            {
                glControlTop.Invalidate();
            }
        }

        private void glControlTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                top_selected = true;
                OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlTop.Width / 2, this.Location.Y + 31 + menuStrip1.Height + glControlTop.Height / 2);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Entity hit = RayTraceEntity(new Location(top_mousepos.X, top_mousepos.Y, 1000000), new Location(top_mousepos.X, top_mousepos.Y, -1000000));
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
                if (Selected.Count == 1 && Selected[0] is CubeEntity
                    && !((CubeEntity)Selected[0]).ContainsPoint(new Location(top_mousepos.X, top_mousepos.Y, Selected[0].Position.Z)))
                {
                    top_stretching = true;
                    CubeEntity ce = (CubeEntity)Selected[0];
                    stretch_x = top_mousepos.X > ce.Maxes.X ? 1 : (top_mousepos.X < ce.Mins.X ? -1 : 0);
                    stretch_y = top_mousepos.Y > ce.Maxes.Y ? 1 : (top_mousepos.Y < ce.Mins.Y ? -1 : 0);
                    stretch_z = 0;
                }
                else if (Selected.Count > 0)
                {
                    top_moving = true;
                }
                top_ppos = top_mousepos;
            }
        }

        bool top_stretching = false;
        public static int stretch_x = 0;
        public static int stretch_y = 0;
        public static int stretch_z = 0;

        bool top_moving = false;
        Location top_ppos = new Location(0);

        public Entity RayTraceEntity(Location start, Location end)
        {
            Location hitloc = end;
            Entity hitent = null;
            for (int i = 0; i < Entities.Count; i++)
            {
                Location mins;
                Location maxes;
                Location hit;
                Location normal;
                if (Entities[i] is CubeEntity)
                {
                    mins = ((CubeEntity)Entities[i]).Mins;
                    maxes = ((CubeEntity)Entities[i]).Maxes;
                    hit = CollisionUtil.RayTraceBox(new Location(0), mins, maxes, start, hitloc, out normal);
                }
                else
                {
                    mins = new Location(-0.5f);
                    maxes = new Location(0.5f);
                    hit = CollisionUtil.RayTraceBox(Entities[i].Position, mins, maxes, start, hitloc, out normal);
                }
                if ((hit - start).LengthSquared() < (hitloc - start).LengthSquared())
                {
                    hitent = Entities[i];
                    hitloc = hit;
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
            }
            else if (e.Button == MouseButtons.Left)
            {
                top_moving = false;
                top_stretching = false;
                side_moving = false;
                side_stretching = false;
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
            Location mpos = new Location((float)e.X / ((float)glControlSide.Width / 2f) - 1f, -((float)e.Y / ((float)glControlSide.Height / 2f) - 1f), 0f);
            side_mousepos = new Location(Vector3.Transform(mpos.ToOVector(), side_proj.Inverted()));
            if (side_selected)
            {
                float mx = (float)(e.X - glControlSide.Width / 2) / 25f * mouse_sens;
                float my = (float)(e.Y - glControlSide.Height / 2) / 25f * mouse_sens;
                side_translate.X -= mx;
                side_translate.Y += my;
                if (Math.Abs(mx) > 0.1 || Math.Abs(my) > 0.1)
                {
                    OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlSide.Width / 2,
                        this.Location.Y + 31 + menuStrip1.Height + splitContainer2.SplitterDistance + splitContainer2.SplitterRectangle.Height + glControlSide.Height / 2);
                }
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
                    ((CubeEntity)Selected[0]).Include(new Location(ModifierKeys.HasFlag(Keys.Control) ? side_mousepos.X : (int)side_mousepos.X,
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
            if (e.Button == MouseButtons.Middle)
            {
                side_selected = true;
                OpenTK.Input.Mouse.SetPosition(this.Location.X + 8 + glControlSide.Width / 2,
                    this.Location.Y + 31 + menuStrip1.Height + splitContainer2.SplitterDistance + splitContainer2.SplitterRectangle.Height + glControlSide.Height / 2);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Entity hit = RayTraceEntity(new Location(side_mousepos.X, 1000000, side_mousepos.Z), new Location(side_mousepos.X, -1000000, side_mousepos.Z));
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
                if (Selected.Count == 1 && Selected[0] is CubeEntity
                    && !((CubeEntity)Selected[0]).ContainsPoint(new Location(side_mousepos.X, Selected[0].Position.Y, side_mousepos.Z)))
                {
                    side_stretching = true;
                    CubeEntity ce = (CubeEntity)Selected[0];
                    stretch_x = side_mousepos.X > ce.Maxes.X ? 1 : (side_mousepos.X < ce.Mins.X ? -1 : 0);
                    stretch_y = 0;
                    stretch_z = side_mousepos.Z > ce.Maxes.Z ? 1 : (side_mousepos.Z < ce.Mins.Z ? -1 : 0);
                }
                else if (Selected.Count > 0)
                {
                    side_moving = true;
                }
                side_ppos = side_mousepos;
            }
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            Entities.Clear();
            Selected.Clear();
        }

        public void LoadMap(string data)
        {
            data = data.Replace('\r', '\n').Replace('\t', '\n').Replace("\n", "");
            int start = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '{')
                {
                    string objo = data.Substring(start, i - start);
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
            for (int i = 0; i < Entities.Count; i++)
            {
                SysConsole.Output(OutputType.INFO, "Entity " + i + ": " + Entities[i].ToString());
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
                    string[] datum = dats[i].Split(':');
                    if (datum.Length != 2)
                    {
                        throw new Exception("Invalid key '" + datum + "'!");
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
            Entity e;
            switch (name) // TODO: Registry
            {
                case "cube":
                    e = new CubeEntity(new Location(0), new Location(0));
                    break;
                case "point_light":
                    e = new PointLightEntity(new Location(0), 1, new Location(1), false);
                    break;
                case "spawn":
                    e = new SpawnPointEntity(new Location(0));
                    break;
                default:
                    throw new Exception("Invalid entity type '" + name + "'!");
            }
            for (int i = 0; i < dats.Length; i++)
            {
                if (dats[i].Length <= 0)
                {
                    continue;
                }
                string[] datum = dats[i].Split(':');
                if (datum.Length != 2)
                {
                    throw new Exception("Invalid key '" + datum + "'!");
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
                file = sfd.FileName;
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
            if (e.KeyCode == Keys.Delete)
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
                entityTypeChooser.Show(glControlTop, 0, 0);
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
        }

        private void glControlTop_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                MakeCuboidAt(new Location(top_mousepos.X, top_mousepos.Y, 0));
            }
            PrimaryEditor_KeyDown(sender, e);
        }

        public void MakeCuboidAt(Location spot)
        {
            spot = new Location((int)spot.X, (int)spot.Y, (int)spot.Z);
            CubeEntity ce = new CubeEntity(spot - new Location(1), spot + new Location(1));
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

        private void glControlSide_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                MakeCuboidAt(new Location(side_mousepos.X, 0, side_mousepos.Z));
            }
            PrimaryEditor_KeyDown(sender, e);
        }

        private void PrimaryEditor_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void glControlTop_KeyUp(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyUp(sender, e);
        }

        private void glControlSide_KeyUp(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyUp(sender, e);
        }

        private void glControlTex_KeyUp(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyUp(sender, e);
        }

        private void glControlTex_KeyDown(object sender, KeyEventArgs e)
        {
            PrimaryEditor_KeyDown(sender, e);
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
