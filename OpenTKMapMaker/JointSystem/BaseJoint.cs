using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.EntitySystem;

namespace OpenTKMapMaker.JointSystem
{
    public abstract class BaseJoint
    {
        public string TargetIDOne;
        public string TargetIDTwo;

        public abstract string JType();

        public virtual bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "one":
                    TargetIDOne = value;
                    return true;
                case "two":
                    TargetIDTwo = value;
                    return true;
                default:
                    return false;
            }
        }

        public virtual List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
            vars.Add(new KeyValuePair<string, string>("one", TargetIDOne));
            vars.Add(new KeyValuePair<string, string>("two", TargetIDTwo));
            return vars;
        }

        public virtual void Render(GLContext context)
        {
            Entity e1 = PrimaryEditor.GetTarget(TargetIDOne);
            Entity e2 = PrimaryEditor.GetTarget(TargetIDTwo);
            if (e1 != null && e2 != null)
            {
                context.Rendering.RenderLine(e1.Position, e2.Position);
            }
        }
    }
}
