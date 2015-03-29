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
        void glControlView_MouseWheel(object sender, MouseEventArgs e)
        {
            CameraPos += Utilities.ForwardVector_Deg(CameraYaw, CameraPitch) * e.Delta / 120f; // 120 = WHEEL_DELTA - By default, at least.
            glControlView.Invalidate();
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
                GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
                GL.Enable(EnableCap.DepthTest);
                if (renderLightingToolStripMenuItem.Checked)
                {
                }
                else
                {
                    CurrentContext.Shaders.ColorMultShader.Bind();
                    Location CameraTarget = CameraPos + Utilities.ForwardVector_Deg(CameraYaw, CameraPitch);
                    proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraFOV), (float)CurrentContext.Control.Width / (float)CurrentContext.Control.Height, CameraZNear, CameraZFar);
                    view = Matrix4.LookAt(CameraPos.ToOVector(), CameraTarget.ToOVector(), CameraUp.ToOVector());
                    combined = view * proj;
                    GL.UniformMatrix4(1, false, ref combined);
                    Render3D(CurrentContext);
                }
                GL.Disable(EnableCap.DepthTest);
                CurrentContext.Shaders.ColorMultShader.Bind();
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

        void ResizeView()
        {
            glControlView.Size = splitContainer3.Panel1.ClientSize;
        }

        private void glControlView_Resize(object sender, EventArgs e)
        {
            glControlView.MakeCurrent();
            GL.Viewport(0, 0, glControlView.Width, glControlView.Height);
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

        private void glControlView_MouseEnter(object sender, EventArgs e)
        {
            glControlView.Focus();
            invalidateAll();
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
}
