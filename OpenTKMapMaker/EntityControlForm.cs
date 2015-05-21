using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTKMapMaker.EntitySystem;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker
{
    public partial class EntityControlForm : Form
    {
        Entity ent;

        public EntityControlForm(Entity e)
        {
            ent = e;
            InitializeComponent();
            recalc();
            listBox1.MouseDoubleClick += new MouseEventHandler(listBox1_MouseDoubleClick);
            textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
        }

        public void recalc()
        {
            if (ent == null)
            {
                label1.Text = "Entity: /WORLD/";
                textBox2.Text = "/WORLD/";
                textBox2.ReadOnly = true;
            }
            else
            {
                label1.Text = "Entity: " + ent.GetEntityType();
                textBox2.Text = ent.GetEntityType();
                textBox2.ReadOnly = false;
            }
            resetListBoxContents();
            textBox1.ReadOnly = true;
        }

        void resetListBoxContents()
        {
            listBox1.Items.Clear();
            List<KeyValuePair<string, string>> vars;
            if (ent != null)
            {
                vars = ent.GetVars();
            }
            else
            {
                vars = PrimaryEditor.GetVars();
            }
            foreach (KeyValuePair<string, string> kvp in vars)
            {
                if (kvp.Value == null)
                {
                    listBox1.Items.Add(kvp.Key + ": &NULL");
                }
                else
                {
                    listBox1.Items.Add(kvp.Key + ": " + kvp.Value.Replace("&", "&amp").Replace("\n", "&nl"));
                }
            }
        }

        void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0 && !textBox1.ReadOnly)
            {
                string s2 = textBox1.Text.Replace(Environment.NewLine, "\n").Replace("\r", "");
                if (ent != null)
                {
                    ent.ApplyVar(label2.Text, s2);
                    ent.Recalculate();
                }
                else
                {
                    PrimaryEditor.ApplyVar(label2.Text, s2);
                }
                resetListBoxContents();
                PrimaryEditor.PRFMain.invalidateAll();
            }
        }

        void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string res = null;
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                Rectangle r = listBox1.GetItemRectangle(i);
                if (r.Contains(e.X, e.Y))
                {
                    res = listBox1.Items[i].ToString();
                }
            }
            if (res != null)
            {
                string[] data = res.Split(new char[] { ':' }, 2);
                label2.Text = data[0];
                textBox1.Text = data[1].Trim().Replace("&nl", "\r\n").Replace("&amp", "&");
                textBox1.ReadOnly = false;
                if (data[0].Contains("scriptcommands"))
                {
                    textBox1.Multiline = true;
                }
                else
                {
                    textBox1.Multiline = false;
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (ent != null)
            {
                if (textBox2.Text.ToLower() != ent.GetEntityType().ToLower())
                {
                    PrimaryEditor.PRFMain.SetType(textBox2.Text);
                    if (PrimaryEditor.Selected.Count != 1)
                    {
                        Close();
                        return;
                    }
                    if (ent != PrimaryEditor.Selected[0])
                    {
                        ent = PrimaryEditor.Selected[0];
                        recalc();
                    }
                }
            }
        }
    }
}
