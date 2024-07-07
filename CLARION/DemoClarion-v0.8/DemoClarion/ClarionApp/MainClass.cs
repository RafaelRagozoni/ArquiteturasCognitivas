
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ClarionApp;
using ClarionApp.Model;
using ClarionApp.Exceptions;
using Gtk;

namespace ClarionApp
{
	class MainClass
	{
		#region properties
		private WSProxy ws = null;
        private ClarionAgent agent;
        String creatureId = String.Empty;
        String creatureName = String.Empty;
		#endregion

		#region constructor
		public MainClass() {
			Application.Init();
			Console.WriteLine ("ClarionApp V0.8");
			try
            {
                ws = new WSProxy("localhost", 4011);

                String message = ws.Connect();

                if (ws != null && ws.IsConnected)
                {
                    Console.Out.WriteLine ("[SUCCESS] " + message + "\n");
					ws.SendWorldReset();
                    ws.NewCreature(400, 200, 0, out creatureId, out creatureName);
					ws.SendCreateLeaflet();
                    ws.NewBrick(4, 747, 2, 800, 567);
                    ws.NewBrick(4, 50, -4, 747, 47);
                    ws.NewBrick(4, 49, 562, 796, 599);
                    ws.NewBrick(4, -2, 6, 50, 599);

                    // Create new delivery spot
                    ws.NewDeliverySpot (4, 415, 252);

                    // Create some food
                    ws.NewFood (0, 415, 212);
                    // ws.NewFood (0, 237, 321);
                    // ws.NewFood (0, 165, 440);

                    // Create 9 jewels of each color, to enable easy planning
                    ws.NewJewel (0, 200, 200);
                    ws.NewJewel (0, 200, 220);
                    ws.NewJewel (0, 200, 440);
                    ws.NewJewel (0, 420, 100);
                    ws.NewJewel (0, 420, 220);
                    ws.NewJewel (0, 420, 440);
                    ws.NewJewel (0, 640, 100);
                    ws.NewJewel (0, 640, 220);
                    ws.NewJewel (0, 640, 440);
                    ws.NewJewel (1, 200, 200);
                    ws.NewJewel (1, 140, 340);
                    ws.NewJewel (1, 140, 500);
                    ws.NewJewel (1, 340, 220);
                    ws.NewJewel (1, 340, 340);
                    ws.NewJewel (1, 340, 500);
                    ws.NewJewel (1, 600, 140);
                    ws.NewJewel (1, 600, 340);
                    ws.NewJewel (1, 600, 500);
                    ws.NewJewel (2, 250, 170);
                    ws.NewJewel (2, 250, 240);
                    ws.NewJewel (2, 250, 400);
                    ws.NewJewel (2, 440, 170);
                    ws.NewJewel (2, 440, 240);
                    ws.NewJewel (2, 440, 400);
                    ws.NewJewel (2, 530, 170);
                    ws.NewJewel (2, 530, 240);
                    ws.NewJewel (2, 530, 400);

                    ws.NewJewel (3, 260, 100);
                    ws.NewJewel (3, 260, 220);
                    ws.NewJewel (3, 260, 440);
                    ws.NewJewel (3, 500, 100);
                    ws.NewJewel (3, 500, 220);
                    ws.NewJewel (3, 480, 440);
                    ws.NewJewel (3, 700, 100);
                    ws.NewJewel (3, 700, 220);
                    ws.NewJewel (3, 700, 440);
                    ws.NewJewel (4, 200, 140);
                    ws.NewJewel (4, 200, 340);
                    ws.NewJewel (4, 200, 500);
                    ws.NewJewel (4, 400, 220);
                    ws.NewJewel (4, 400, 340);
                    ws.NewJewel (4, 400, 500);
                    ws.NewJewel (4, 660, 140);
                    ws.NewJewel (4, 660, 340);
                    ws.NewJewel (4, 660, 500);
                    ws.NewJewel (5, 310, 170);
                    ws.NewJewel (5, 310, 240);
                    ws.NewJewel (5, 310, 400);
                    ws.NewJewel (5, 500, 170);
                    ws.NewJewel (5, 500, 240);
                    ws.NewJewel (5, 500, 400);
                    ws.NewJewel (5, 590, 170);
                    ws.NewJewel (5, 590, 240);
                    ws.NewJewel (5, 590, 400);

                    if (!String.IsNullOrWhiteSpace(creatureId))
                    {
                        ws.SendStartCamera(creatureId);
                        ws.SendStartCreature(creatureId);
                    }

                    Console.Out.WriteLine("Creature created with name: " + creatureId + "\n");
					agent = new ClarionAgent(ws,creatureId,creatureName);
                    agent.Run();
					Console.Out.WriteLine("Running Simulation ...\n");
                }
				else {
					Console.Out.WriteLine("The WorldServer3D engine was not found ! You must start WorldServer3D before running this application !");
					System.Environment.Exit(1);
				}
            }
            catch (WorldServerInvalidArgument invalidArtgument)
            {
                Console.Out.WriteLine(String.Format("[ERROR] Invalid Argument: {0}\n", invalidArtgument.Message));
            }
            catch (WorldServerConnectionError serverError)
            {
                Console.Out.WriteLine(String.Format("[ERROR] Is is not possible to connect to server: {0}\n", serverError.Message));
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(String.Format("[ERROR] Unknown Error: {0}\n", ex.Message));
            }
			Application.Run();
		}
		#endregion

		#region Methods
		public static void Main (string[] args)	{
			new MainClass();
		}
			
        #endregion
	}
	
	
}
