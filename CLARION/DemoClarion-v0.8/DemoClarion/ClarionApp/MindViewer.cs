using System;
using System.Timers;
using ClarionApp.Model;

namespace ClarionApp
{
	public partial class MindViewer : Gtk.Window
	{	 
		public int red;
		public int green;
		public int blue;
		public int yellow;
		public int magenta;
		public int white;
		public int apples;
		public int nuts;
		public int[] leaflet1 = new int[6];
		public int[] leaflet2 = new int[6];
		public int[] leaflet3 = new int[6];

		public MindViewer () : base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			update();
			startViewTimer();
		}

		public void setBag(int r, int g, int b, int y, int m, int w, int a, int n) {
			red = r;
			green = g;
			blue = b;
			yellow = y;
			magenta = m;
			white = w;
			apples = a;
			nuts = n;
			update();
		}

		public void setBag(Sack s) {
			red = s.red_crystal;
			green = s.green_crystal;
			blue = s.blue_crystal;
			yellow = s.yellow_crystal;
			magenta = s.magenta_crystal;
			white = s.white_crystal;
			apples = s.p_food;
			nuts = s.np_food;
			//update();
		}

		public void updateLeaflet(int n, Leaflet l) {
			switch(n) {
			    case 0: leaflet1[0] = l.getRequired("Red");
				        leaflet1[1] = l.getRequired("Green");
				        leaflet1[2] = l.getRequired("Blue");
				        leaflet1[3] = l.getRequired("Yellow");
				        leaflet1[4] = l.getRequired("Magenta");
				        leaflet1[5] = l.getRequired("White");
				        break;
			    case 1: leaflet2[0] = l.getRequired("Red");
				        leaflet2[1] = l.getRequired("Green");
				        leaflet2[2] = l.getRequired("Blue");
				        leaflet2[3] = l.getRequired("Yellow");
				        leaflet2[4] = l.getRequired("Magenta");
				        leaflet2[5] = l.getRequired("White");
				        break;	
			    case 2: leaflet3[0] = l.getRequired("Red");
				        leaflet3[1] = l.getRequired("Green");
				        leaflet3[2] = l.getRequired("Blue");
				        leaflet3[3] = l.getRequired("Yellow");
				        leaflet3[4] = l.getRequired("Magenta");
				        leaflet3[5] = l.getRequired("White");
				        break;
			    default: break;
			}
		}

		public void update() {
			lred.Text = red.ToString();
			lgreen.Text = green.ToString();
			lblue.Text = blue.ToString();
			lyellow.Text = yellow.ToString();
			lmagenta.Text = magenta.ToString();
			lwhite.Text = white.ToString();
			lapples.Text = apples.ToString();
			lnuts.Text = nuts.ToString();
			l1r.Text = leaflet1[0].ToString();
			l1g.Text = leaflet1[1].ToString();
			l1b.Text = leaflet1[2].ToString();
			l1y.Text = leaflet1[3].ToString();
			l1m.Text = leaflet1[4].ToString();
			l1w.Text = leaflet1[5].ToString();
			l2r.Text = leaflet2[0].ToString();
			l2g.Text = leaflet2[1].ToString();
			l2b.Text = leaflet2[2].ToString();
			l2y.Text = leaflet2[3].ToString();
			l2m.Text = leaflet2[4].ToString();
			l2w.Text = leaflet2[5].ToString();
			l3r.Text = leaflet3[0].ToString();
			l3g.Text = leaflet3[1].ToString();
			l3b.Text = leaflet3[2].ToString();
			l3y.Text = leaflet3[3].ToString();
			l3m.Text = leaflet3[4].ToString();
			l3w.Text = leaflet3[5].ToString();
		}

		public void startViewTimer() {
			Timer viewTimer = new Timer();
			viewTimer.Elapsed += new ElapsedEventHandler( DisplayTimeEvent );
			viewTimer.Interval = 1000;
			viewTimer.Start();
		}

		public void DisplayTimeEvent( object source, ElapsedEventArgs e ) {
			Console.Write("\r{0}", DateTime.Now);
			// We need to call update this way, because the only thread able to send GTK commands is the Gtk.Application thread
			// Otherwise this might cause unpredictable thread problems
			Gtk.Application.Invoke(delegate{update();});
		}
	}
}

