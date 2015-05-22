using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.EntitySystem
{
    public class FuncTrackEntity: CubeEntity, EntityTargetting
    {
        public string GetTarget()
        {
            return Target;
        }
        
        public FuncTrackEntity(Location min, Location max)
            : base(min, max)
        {
        }

        public string Target = "";

        public float MinDistance = 0.1f;

        public int LoopsPerActivation = 0;

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("target", Target));
            vars.Add(new KeyValuePair<string, string>("mindistance", MinDistance.ToString()));
            vars.Add(new KeyValuePair<string, string>("loopsperactivation", LoopsPerActivation.ToString()));
            return vars;
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "target":
                    Target = value;
                    return true;
                case "mindistance":
                    MinDistance = Utilities.StringToFloat(value);
                    return true;
                case "loopsperactivation":
                    LoopsPerActivation = Utilities.StringToInt(value);
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public override string GetEntityType()
        {
            return "functrack";
        }

        public override Entity CreateInstance()
        {
            return new FuncTrackEntity(Mins, Maxes);
        }
    }
}
