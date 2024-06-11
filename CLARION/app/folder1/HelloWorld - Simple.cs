using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Clarion;
using Clarion.Framework;

namespace Clarion.Samples
{
    public class HelloWorldSimple
    {
        static void Main(string[] args)
        {
            //Initialize the task
            Console.WriteLine("Initializing the Simple Hello World Task");

            int CorrectCounter = 0;
            int NumberTrials = 10000;
            int progress = 0;

            World.LoggingLevel = TraceLevel.Off;

            TextWriter orig = Console.Out;
            StreamWriter sw = File.CreateText("HelloWorldSimple.txt");

            DimensionValuePair hi = World.NewDimensionValuePair("Salutation", "Hello");
            DimensionValuePair bye = World.NewDimensionValuePair("Salutation", "Goodbye");

            ExternalActionChunk sayHi = World.NewExternalActionChunk("Hello");
            ExternalActionChunk sayBye = World.NewExternalActionChunk("Goodbye");

            //Initialize the Agent
            Agent John = World.NewAgent("John");

            SimplifiedQBPNetwork net = AgentInitializer.InitializeImplicitDecisionNetwork(John, SimplifiedQBPNetwork.Factory);

            net.Input.Add(hi);
            net.Input.Add(bye);

            net.Output.Add(sayHi);
            net.Output.Add(sayBye);

            John.Commit(net);

            net.Parameters.LEARNING_RATE = 1;
            John.ACS.Parameters.PERFORM_RER_REFINEMENT = false;

            //Run the task
            Console.WriteLine("Running the Simple Hello World Task");
            Console.SetOut(sw);

            Random rand = new Random();
            SensoryInformation si;

            ExternalActionChunk chosen;

            for (int i = 0; i < NumberTrials; i++)
            {
                si = World.NewSensoryInformation(John);

                //Randomly choose an input to perceive.
                if (rand.NextDouble() < .5)
                {
                    //Say "Hello"
                    si.Add(hi, John.Parameters.MAX_ACTIVATION);
                    si.Add(bye, John.Parameters.MIN_ACTIVATION);
                }
                else
                {
                    //Say "Goodbye"
                    si.Add(hi, John.Parameters.MIN_ACTIVATION);
                    si.Add(bye, John.Parameters.MAX_ACTIVATION);
                }

                //Perceive the sensory information
                John.Perceive(si);

                //Choose an action
                chosen = John.GetChosenExternalAction(si);

                //Deliver appropriate feedback to the agent
                if (chosen == sayHi)
                {
                    //The agent said "Hello".
                    if (si[hi] == John.Parameters.MAX_ACTIVATION)
                    {
                        //The agent responded correctly
                        Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was correct");
                        //Record the agent's success.
                        CorrectCounter++;
                        //Give positive feedback.
                        John.ReceiveFeedback(si, 1.0);
                    }
                    else
                    {
                        //The agent responded incorrectly
                        Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was incorrect");
                        //Give negative feedback.
                        John.ReceiveFeedback(si, 0.0);
                    }
                }
                else
                {
                    //The agent said "Goodbye".
                    if (si[bye] == John.Parameters.MAX_ACTIVATION)
                    {
                        //The agent responded correctly
                        Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was correct");
                        //Record the agent's success.
                        CorrectCounter++;
                        //Give positive feedback.
                        John.ReceiveFeedback(si, 1.0);
                    }
                    else
                    {
                        //The agent responded incorrectly
                        Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was incorrect");
                        //Give negative feedback.
                        John.ReceiveFeedback(si, 0.0);
                    }
                }

                Console.SetOut(orig);
                progress = (int)(((double)(i+1) / (double)NumberTrials) * 100);
                Console.CursorLeft = 0;
                Console.Write(progress + "% Complete..");
                Console.SetOut(sw);
            }

            //Report Results

            Console.WriteLine("Reporting Results for the Simple Hello World Task");
            Console.WriteLine("John got " + CorrectCounter + " correct out of " + NumberTrials + " trials (" +
                (int)Math.Round(((double)CorrectCounter / (double)NumberTrials) * 100) + "%)");

            Console.WriteLine("At the end of the task, John had learned the following rules:");
            foreach (var i in John.GetInternals(Agent.InternalContainers.ACTION_RULES))
                Console.WriteLine(i);

            sw.Close();
            Console.SetOut(orig);
            Console.CursorLeft = 0;
            Console.WriteLine("100% Complete..");
            //Kill the agent to end the task
            Console.WriteLine("Killing John to end the program");
            John.Die();
            Console.WriteLine("John is Dead");

            Console.WriteLine("The Simple Hello World Task has finished");
            Console.WriteLine("The results have been saved to \"HelloWorldSimple.txt\"");
            Console.Write("Press any key to exit");
            Console.ReadKey(true);
        }
    }
}