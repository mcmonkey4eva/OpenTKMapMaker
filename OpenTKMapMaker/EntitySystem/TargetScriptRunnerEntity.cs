using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.EntitySystem
{
    class TargetScriptRunnerEntity: TargetEntity
    {
        public override string GetEntityType()
        {
            return "targetscriptrunner";
        }

        public string ScriptCommands = "";

        public override List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = base.GetVars();
            vars.Add(new KeyValuePair<string, string>("scriptcommands", ScriptCommands));
            return vars;
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "scriptcommands":
                    ScriptCommands = value;
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }

        }

        public override Entity CreateInstance()
        {
            return new TargetScriptRunnerEntity();
        }
    }
}
