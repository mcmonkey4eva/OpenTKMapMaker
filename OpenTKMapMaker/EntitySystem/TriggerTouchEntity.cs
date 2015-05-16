using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.EntitySystem
{
    public class TriggerTouchEntity: CuboidalEntity
    {
        public TriggerTouchEntity()
            : base(Location.Zero, Location.One, "common/trigger")
        {
        }

        public string Target;

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("target", Target));
            return vars;
        }

        public override string GetEntityType()
        {
            return "triggertouch";
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "target":
                    Target = value;
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public override Entity CreateInstance()
        {
            return new TriggerTouchEntity();
        }
    }
}
