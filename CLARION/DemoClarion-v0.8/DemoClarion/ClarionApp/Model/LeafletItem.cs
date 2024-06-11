using System;

namespace ClarionApp.Model
{
	public class LeafletItem
	{
		public LeafletItem ()
		{
		}

		public LeafletItem(string a1, Int32 a2, Int32 a3) {
			itemKey = a1;
			totalNumber = a2;
			collected = a3;
		}

		public string itemKey { get; set; }
		public Int32 totalNumber { get; set; }
		public Int32 collected { get; set; }
	}
}

