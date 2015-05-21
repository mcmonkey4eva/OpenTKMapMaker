using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTKMapMaker.EntitySystem
{
    public class TriggerGenericEntity: TriggerEntity
    {
        public override string GetEntityType()
        {
            return "triggergeneric";
        }

        public TriggerType Trigger_Type = TriggerType.USE;

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("trigger_type", Trigger_Type.ToString()));
            return vars;
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "trigger_type":
                    TriggerType ttype;
                    if (Enum.TryParse(value.ToUpper(), out ttype))
                    {
                        Trigger_Type = ttype;
                    }
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }

        }

        public override Entity CreateInstance()
        {
            return new TriggerGenericEntity();
        }
    }

    public enum TriggerType: byte
    {
        NONE = 0,
        TOUCH = 1,
        USE = 2
    }
}
