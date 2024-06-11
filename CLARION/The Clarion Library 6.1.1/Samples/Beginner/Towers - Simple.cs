using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Clarion;
using Clarion.Framework;

namespace Clarion.Samples
{
	public class SimpleTowers
	{
		private Agent John;
		
		private DimensionValuePair p1;
		private DimensionValuePair p2;
		private DimensionValuePair p3;
		private DimensionValuePair p4;
		private DimensionValuePair p5;
		
		private ExternalActionChunk mp1;
		private ExternalActionChunk mp2;
		private ExternalActionChunk mp3;
		private ExternalActionChunk mp4;
		private ExternalActionChunk mp5;
		
		private int numTrials = 10000;
		private int numBlocks = 10;
		private int numCorrect = 0;
		
		private SimplifiedQBPNetwork net;
		
		private Random rand = new Random();
		private int [] corelations = new int [5];

		public static void Main ()
		{
			SimpleTowers t = new SimpleTowers();
			
			t.Initialize();
			
			t.Run();
		}
		
		private void Initialize()
		{
            World.LoggingLevel = TraceLevel.Off;

			p1 = World.NewDimensionValuePair("Peg", 1);
			p2 = World.NewDimensionValuePair("Peg", 2);
			p3 = World.NewDimensionValuePair("Peg", 3);
			p4 = World.NewDimensionValuePair("Peg", 4);
			p5 = World.NewDimensionValuePair("Peg", 5);
			
			mp1 = World.NewExternalActionChunk();
			mp2 = World.NewExternalActionChunk();
			mp3 = World.NewExternalActionChunk();
			mp4 = World.NewExternalActionChunk();
			mp5 = World.NewExternalActionChunk();
			
			mp1 += p1;
			mp2 += p2;
			mp3 += p3;
			mp4 += p4;
			mp5 += p5;
			
			John = World.NewAgent();
			
			net = AgentInitializer.InitializeImplicitDecisionNetwork(John, SimplifiedQBPNetwork.Factory);
			
			net.Input.Add(p1);
			net.Input.Add(p2);
			net.Input.Add(p3);
			net.Input.Add(p4);
			net.Input.Add(p5);
			
			net.Output.Add(mp1);
			net.Output.Add(mp2);
			net.Output.Add(mp3);
			net.Output.Add(mp4);
			net.Output.Add(mp5);

            net.Parameters.LEARNING_RATE = 1;
            net.Parameters.MOMENTUM = .01;
			
			John.Commit(net);

			RefineableActionRule.GlobalParameters.GENERALIZATION_THRESHOLD_1 = -.01;
			RefineableActionRule.GlobalParameters.SPECIALIZATION_THRESHOLD_1 = -.4;
		}
		
		private void Run()
		{
			SensoryInformation si;
			
			bool shuffle = true;
			for (int b = 0; b < numBlocks; b++)
			{
				Console.Write("Starting block # " + (b+1));
				if(shuffle)
				{
					Console.WriteLine("... Shuffling pegs.");
					for(int i = 0; i < 5; i++)
					{
						corelations[i] = rand.Next(5);
						while(corelations[i] == i)
							corelations[i] = rand.Next(5);
						
						Console.WriteLine("Starting Peg " + (i+1) + " --> Target Peg " + (corelations[i] + 1));
					}
				}
				else
					Console.WriteLine();

                int progress = 0;

				for (int i = 0; i < numTrials; i++)
				{
					si = World.NewSensoryInformation(John);
                    int peg = 0;
					switch(rand.Next(5))
					{
					case 0:
						si.Add(p1, 1);
                        si.Add(p2, 0);
                        si.Add(p3, 0);
                        si.Add(p4, 0);
                        si.Add(p5, 0);
                        peg = 1;
						break;
					case 1:
						si.Add(p1, 0);
                        si.Add(p2, 1);
                        si.Add(p3, 0);
                        si.Add(p4, 0);
                        si.Add(p5, 0);
                        peg = 2;
						break;
					case 2:
						si.Add(p1, 0);
                        si.Add(p2, 0);
                        si.Add(p3, 1);
                        si.Add(p4, 0);
                        si.Add(p5, 0);
                        peg = 3;
						break;
					case 3:
						si.Add(p1, 0);
                        si.Add(p2, 0);
                        si.Add(p3, 0);
                        si.Add(p4, 1);
                        si.Add(p5, 0);
                        peg = 4;
						break;
					case 4:
						si.Add(p1, 0);
                        si.Add(p2, 0);
                        si.Add(p3, 0);
                        si.Add(p4, 0);
                        si.Add(p5, 1);
                        peg = 5;
						break;
					}

                    Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "For block " + (b + 1) + ", trial # " + (i + 1) + ": The starting peg is " + 
					              peg + ", the target peg is " + 
					              (corelations[peg - 1] + 1));
					
					John.Perceive(si);
					
					ExternalActionChunk chosen = John.GetChosenExternalAction(si);
					
					if(corelations[peg - 1] + 1 == (int)chosen.First().Value.AsIComparable)
					{
                        Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was correct");
						John.ReceiveFeedback(si, 1);
						numCorrect++;
					}
					else
					{
                        Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was incorrect");
						John.ReceiveFeedback(si, 0);
					}

                    progress = (int)(((double)(i + 1) / (double)numTrials) * 100);
                    Console.CursorLeft = 0;
                    Console.Write(progress + "% Complete..");
				}
                Console.WriteLine();

				Console.WriteLine("Block " + (b+1) + " is finished. Let's see how John did...");
				Console.WriteLine("John's performance on block " + (b+1) + ": " + 
				                  Math.Round(((double)numCorrect/(double)numTrials)*100) + "%");
				Console.WriteLine("Rules John learned:");
				foreach(var r in John.GetInternals(Agent.InternalContainers.ACTION_RULES))
					Console.WriteLine(r);
				Console.Write("For the next block, would you like to shuffle the pegs to see if John can adjust (y = yes, n = no, x = exit)?");
				char ans = Console.ReadKey().KeyChar;
                Console.WriteLine();

                if (ans == 'y')
                    shuffle = true;
                else if (ans == 'n')
                    shuffle = false;
                else if (ans == 'x')
                    break;
				numCorrect = 0;
			}

            //Kill the agent to end the task
            Console.WriteLine("Killing John to end the program");
            John.Die();
            Console.WriteLine("John is Dead");

            Console.WriteLine("The Simple Tower of Hanoi Task has finished");
            Console.Write("Press any key to exit");
            Console.ReadKey(true);
		}
	}
}

