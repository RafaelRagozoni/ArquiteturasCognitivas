using System;
using System.Collections.Generic;

namespace ClarionApp.Model
{
	public class Leaflet
	{
		public Leaflet ()
		{
		}

		public Int64 leafletID { get; set; }
		public Int32 NumberOfLeafletItems { get; set; }
		public List<LeafletItem> items { get; set; }
		public Int32 payment { get; set; }
		public Boolean situation { get; set; }

		public void PrintLeaflet(int k) {
			Console.Write("ID: "+leafletID+" ");
			foreach (LeafletItem li in items) {
			    Console.Write(li.itemKey+" "+li.totalNumber+" "+li.collected+" ");
			}
			Console.WriteLine("PAY: "+payment);
		}

		public int getCollected(String type) {
			foreach (LeafletItem li in items) {
				if (li.itemKey.Equals(type)) return(li.collected);
			}
			return(0);
		}

		public int getRequired(String type) {
		    foreach (LeafletItem li in items) {
			    if (li.itemKey.Equals(type)) return(li.totalNumber);
			}
			return(0);
		}

		public void print() {
			Console.WriteLine("Red: "+getRequired("Red")+" Green: "+getRequired("Green")+" Blue: "+getRequired("Blue")+
			                  " Yellow: "+getRequired("Yellow")+" Magenta: "+getRequired("Magenta")+" White: "+getRequired("White"));
			
		}

		public Dictionary<string,LeafletItem> getLeaflet() {
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
			foreach (LeafletItem ll in items) {
				LeafletItem lall = m[ll.itemKey];
				lall.totalNumber += ll.totalNumber;
			}
			return(m);
		}
	}
	
}

