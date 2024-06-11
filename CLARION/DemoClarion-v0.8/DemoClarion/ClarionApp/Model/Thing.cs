using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Model
{
    public class Thing
    {
        public const int CATEGORY_CREATURE = 0;
        public const int CATEGORY_BRICK = 1;
        public const int CATEGORY_FOOD = 2;
        public const int categoryPFOOD = 21;
        public const int CATEGORY_NPFOOD = 22;
        public const int CATEGORY_JEWEL = 3;
        public const int CATEGORY_DeliverySPOT = 4;

        public String Name { get; set; }
        public Int32 CategoryId { get; set; }
        public Boolean IsOccluded { get; set; }
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }
        public double Pitch { get; set; }
        public double Size { get; set; }
        public Material3d Material { get; set; }
        public double Reward { get; set; }
        public double DistanceToCreature { get; set; }
		public double comX { get; set; }
		public double comY { get; set; }
    }
}
