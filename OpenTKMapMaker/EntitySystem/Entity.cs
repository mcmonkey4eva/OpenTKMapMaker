using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTKMapMaker.Utility;
using OpenTK.Graphics;

namespace OpenTKMapMaker.EntitySystem
{
    public abstract class Entity
    {
        public Entity()
        {
            float f1 = (float)Utilities.UtilRandom.NextDouble() * 0.8f;
            ViewColor = new Color4(1 - f1, f1, 0, 1f);
        }

        public virtual Location GetMins()
        {
            return new Location(-0.5f);
        }

        public virtual Location GetMaxes()
        {
            return new Location(0.5f);
        }

        public Color4 ViewColor;

        public Location Position;
        public Location Angle;
        public Location Velocity;
        public Location Angular_Velocity;
        public float Mass = 0f;
        public bool Selected = false;
        public float Friction = 0.5f;
        public bool Solid = true;

        public abstract string GetEntityType();

        public virtual List<KeyValuePair<string, string>> GetVars()
        {
            List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
            vars.Add(new KeyValuePair<string, string>("position", Position.ToString()));
            vars.Add(new KeyValuePair<string, string>("angle", Angle.ToString()));
            vars.Add(new KeyValuePair<string, string>("velocity", Velocity.ToString()));
            vars.Add(new KeyValuePair<string, string>("angular_velocity", Angular_Velocity.ToString()));
            vars.Add(new KeyValuePair<string, string>("mass", Mass.ToString()));
            vars.Add(new KeyValuePair<string, string>("friction", Friction.ToString()));
            vars.Add(new KeyValuePair<string, string>("solid", (Solid ? "true": "false")));
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
                case "friction":
                    Friction = Utilities.StringToFloat(value);
                    return true;
                case "solid":
                    Solid = value.ToLower() == "true";
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

        /// <summary>
        /// Called when the entity's vars have all been pushed.
        /// </summary>
        public virtual void Recalculate()
        {
        }

        /// <summary>
        /// Called when the entity is removed from the world.
        /// </summary>
        public virtual void OnDespawn()
        {
        }
    }
}
