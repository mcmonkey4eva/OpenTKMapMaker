using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;

namespace OpenTKMapMaker.EntitySystem
{
    public abstract class Entity
    {
        public Location Position;
        public Location Angle;
        public Location Velocity;
        public Location Angular_Velocity;
        public float Mass = 0;

        public abstract string GetEntityType();

        public virtual List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
            vars.Add(new KeyValuePair<string, string>("position", Position.ToString()));
            vars.Add(new KeyValuePair<string, string>("angle", Angle.ToString()));
            vars.Add(new KeyValuePair<string, string>("velocity", Velocity.ToString()));
            vars.Add(new KeyValuePair<string, string>("angular_velocity", Angular_Velocity.ToString()));
            vars.Add(new KeyValuePair<string, string>("mass", Mass.ToString()));
            return vars;
        }

        public virtual bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "position":
                    Position = Location.FromString(value);
                    return true;
                case "angle":
                    Angle = Location.FromString(value);
                    return true;
                case "velocity":
                    Velocity = Location.FromString(value);
                    return true;
                case "angular_velocity":
                    Angular_Velocity = Location.FromString(value);
                    return true;
                case "mass":
                    Mass = Utilities.StringToFloat(value);
                    return true;
                default:
                    return false;
            }
        }

        public abstract void Render(GLContext context);

        public virtual void Reposition(Location pos)
        {
            Position = pos;
        }
    }
}
