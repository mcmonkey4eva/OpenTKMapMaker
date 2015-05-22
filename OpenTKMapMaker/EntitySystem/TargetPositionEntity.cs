using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.EntitySystem
{
    class TargetPositionEntity : TargetEntity, EntityTargetting
    {
        public string GetTarget()
        {
            return Target;
        }
        
        public override string GetEntityType()
        {
            return "targetposition";
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

        public override Entity CreateInstance()
        {
            return new TargetPositionEntity();
        }
    }
}
