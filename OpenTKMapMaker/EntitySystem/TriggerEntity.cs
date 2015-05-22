using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.EntitySystem
{
    public abstract class TriggerEntity: CuboidalEntity, EntityTargetting
    {
        public string GetTarget()
        {
            return Target;
        }
        
        public TriggerEntity()
            : base(Location.Zero, Location.One, "common/trigger")
        {
        }

        public string Target = "";

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("target", Target));
            return vars;
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
    }
}
