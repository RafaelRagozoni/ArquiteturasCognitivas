using System;

namespace ClarionApp.Model
{
	public class Sack
	{
		public Sack ()
		{
		}

		public Int32 n_food { get; set; }
		public Int32 n_crystal { get; set; }
		public Int32 p_food { get; set; }
		public Int32 np_food { get; set; }
		public Int32 red_crystal { get; set; }
		public Int32 green_crystal { get; set; }
		public Int32 blue_crystal { get; set; }
		public Int32 yellow_crystal { get; set; }
		public Int32 magenta_crystal { get; set; }
		public Int32 white_crystal { get; set; }

		public void print() {
			Console.WriteLine("Sack: "+p_food+" apples, "+np_food+" nuts, "+red_crystal+" red crystals, "+green_crystal+" green crystals, "+blue_crystal+" blue crystals, "+yellow_crystal+" yellow crystals, "+magenta_crystal+" magenta crystals, "+white_crystal+" white crystals");
		}

	}
}

