using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;
using OpenTKMapMaker.EntitySystem;

namespace OpenTKMapMaker.JointSystem
{
    public class JointBallSocket: BaseJoint
    {
        public Location pos;

        public override string JType()
        {
            return "ballsocket";
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "position":
                    pos = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("position", pos.ToString()));
            return vars;
        }

        public override void Render(GLContext context)
        {
            Entity e1 = PrimaryEditor.GetTarget(TargetIDOne);
            Entity e2 = PrimaryEditor.GetTarget(TargetIDTwo);
            if (e1 != null && e2 != null)
            {
                context.Rendering.RenderLineBox(pos - new Location(0.1f), pos + new Location(0.1f));
                context.Rendering.RenderLine(e1.Position, pos);
                context.Rendering.RenderLine(e2.Position, pos);
            }
        }
    }
}
