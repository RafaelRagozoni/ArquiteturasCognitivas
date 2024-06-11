using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClarionApp.Model
{
    public enum MotorSystemType
    {
        CAR,
        TWO_WHEEL
    }

    public class Creature:Thing
    {
        public Int32 index { get; set; }
		public MotorSystemType MotorSystem { get; set; }
        public double Wheel { get; set; }
        public double Speed { get; set; }
        public double Fuel { get; set; }
		public double serotonin { get; set; }
		public double endorphine { get; set; }
        public Boolean HasLeaflet { get; set; }
        public Int32 NumberOfLeaflets { get; set; }
		public List<Leaflet> leaflets {get; set; }
		public double score { get; set; }
		public double XX1 { get; set; }
		public double YY1 { get; set; }
		public double XX2 { get; set; }
		public double YY2 { get; set; }
		public string actionData { get; set; }
		public Boolean HasCollided { get; set; }

		public List<Leaflet> getLeaflets() {
			return leaflets;
		}

		public Dictionary<string,LeafletItem> getConsolidatedLeaflet() {
			Dictionary<string,LeafletItem> m = new Dictionary<string,LeafletItem>();
			LeafletItem li = new LeafletItem("Red",0,0);
			m.Add("Red",li);
			li = new LeafletItem("Green",0,0);
			m.Add("Green",li);
			li = new LeafletItem("Blue",0,0);
			m.Add("Blue",li);
			li = new LeafletItem("Yellow",0,0);
			m.Add("Yellow",li);
			li = new LeafletItem("Magenta",0,0);
			m.Add("Magenta",li);
			li = new LeafletItem("White",0,0);
			m.Add("White",li);
			foreach (Leaflet ll in leaflets) {
				Dictionary<string,LeafletItem> l = ll.getLeaflet();
				foreach(var pair in l) {
					LeafletItem l3 = l[pair.Key];
					LeafletItem glob = m[pair.Key];
					glob.totalNumber += l3.totalNumber;
					glob.collected += l3.collected;
				}
			}
			return(m);
		}

		public void PrintLeaflet() {
			Dictionary<string,LeafletItem> l = getConsolidatedLeaflet();
			Console.Write("--> ");
			foreach(var pair in l) {
				Console.Write(pair.Key+" "+pair.Value.totalNumber+" "+pair.Value.collected+" "); 
			}
			Console.WriteLine(" ");
		}

    }
}
