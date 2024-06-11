using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Model
{
    public class Material3d
    {
        private double hardness;

        public double Hardness
        {
            get { return hardness; }
            set { hardness = value; }
        }

        private double energy;

        public double Energy
        {
            get { return energy; }
            set { energy = value; }
        }

        private double taste;

        public double Taste
        {
            get { return taste; }
            set { taste = value; }
        }

        private String color;

        public String Color
        {
            get { return color; }
            set { color = value; }
        }
    }
}
