using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Core;
using Clarion.Plugins;

namespace Clarion.Samples
{
    public class AsynchronousXOR : AsynchronousSimulatingEnvironment
    {
        #region Fields

        /// <summary>
        /// The agent who is running this task.
        /// </summary>
        public Agent John;

        /// <summary>
        /// A counter to keep track of how many trials the agent gets correct.
        /// </summary>
        public int CorrectCounter = 0;
        /// <summary>
        /// The number of trials to be run.
        /// </summary>
        public int NumberTrials = 2000;

        public int NumberRepeats = 20;

        public TextWriter orig = Console.Out;

        public StreamWriter sw = File.CreateText("XOR.txt");

        public AutoResetEvent trialWaitHold = new AutoResetEvent(false);

        #endregion

        #region Main Method

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing XOR Task");
            AsynchronousXOR xor = new AsynchronousXOR();
            Console.WriteLine("Running XOR Task");
            xor.Run();
        }

        #endregion

        #region Constructor & Methods

        public AsynchronousXOR()
        {
            World.LoggingLevel = TraceLevel.Off;

            John = World.NewAgent("John");

            SimplifiedQBPNetwork net = AgentInitializer.InitializeImplicitDecisionNetwork(John, SimplifiedQBPNetwork.Factory);

            net.Input.Add(World.NewDimensionValuePair("Boolean 1", true));
            net.Input.Add(World.NewDimensionValuePair("Boolean 1", false));
            net.Input.Add(World.NewDimensionValuePair("Boolean 2", true));
            net.Input.Add(World.NewDimensionValuePair("Boolean 2", false));

            net.Output.Add(World.NewExternalActionChunk(true));
            net.Output.Add(World.NewExternalActionChunk(false));

            John.ACS.Parameters.PERFORM_RER_REFINEMENT = false;
            John.ACS.Parameters.SELECTION_TEMPERATURE = .01;

            John.ACS.Parameters.LEVEL_SELECTION_OPTION = ActionCenteredSubsystem.LevelSelectionOptions.FIXED;
            
            John.ACS.Parameters.FIXED_IRL_LEVEL_SELECTION_MEASURE = 0;
            John.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 0;

            //Tweak these parameters to see the impact each has on accuracy and learning
            net.Parameters.LEARNING_RATE = 1;
            net.Parameters.MOMENTUM = .02;
            John.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = .5;
            John.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = .5;

            John.Commit(net);

            John.RegisterAsynchronousSimulatingEnvironment(this);
        }

        public void Run()
        {
            Random rand = new Random();
            SensoryInformation si;
            int block_count = 0;
            double r;
            do
            {
                CorrectCounter = 0;
                ++block_count;
                //Run the task for the specified number of trials.
                for (int i = 0; i < NumberTrials; i++)
                {
                    Console.CursorLeft = 0;
                    Console.Out.Write("Running Trial #" + (i + 1) + " of Block #" + block_count);
                    r = rand.NextDouble();
                    si = World.NewSensoryInformation(John);

                    //Randomly choose an input to perceive.
                    if (r < .25)
                    {
                        //True:True
                        si.Add(World.GetDimensionValuePair("Boolean 1", true), 1);
                        si.Add(World.GetDimensionValuePair("Boolean 1", false), 0);
                        si.Add(World.GetDimensionValuePair("Boolean 2", true), 1);
                        si.Add(World.GetDimensionValuePair("Boolean 2", false), 0);
                    }
                    else if (r < .5)
                    {
                        //True:False
                        si.Add(World.GetDimensionValuePair("Boolean 1", true), 1);
                        si.Add(World.GetDimensionValuePair("Boolean 1", false), 0);
                        si.Add(World.GetDimensionValuePair("Boolean 2", true), 0);
                        si.Add(World.GetDimensionValuePair("Boolean 2", false), 1);
                    }
                    else if (r < .75)
                    {
                        //False:True
                        si.Add(World.GetDimensionValuePair("Boolean 1", true), 0);
                        si.Add(World.GetDimensionValuePair("Boolean 1", false), 1);
                        si.Add(World.GetDimensionValuePair("Boolean 2", true), 1);
                        si.Add(World.GetDimensionValuePair("Boolean 2", false), 0);
                    }
                    else
                    {
                        //False:False
                        si.Add(World.GetDimensionValuePair("Boolean 1", true), 0);
                        si.Add(World.GetDimensionValuePair("Boolean 1", false), 1);
                        si.Add(World.GetDimensionValuePair("Boolean 2", true), 0);
                        si.Add(World.GetDimensionValuePair("Boolean 2", false), 1);
                    }

                    John.Perceive(si);

                    trialWaitHold.WaitOne();
                }
            }while (!ReportResults(block_count));

            sw.Close();

            Console.SetOut(orig);
            Console.WriteLine("John has completed the task");
            Console.WriteLine("Killing John");
            John.Die();
            Console.WriteLine("John is Dead");
            Console.WriteLine("XOR Task Completed. See XOR.txt for Results");
            Console.Write("Press any key to exit");
            Console.ReadKey(true);
        }

        public bool ReportResults(int repeatCount)
        {
            int accuracy = (int)Math.Round(((double)CorrectCounter / (double)NumberTrials) * 100);
            Console.SetOut(sw);
            Console.WriteLine("Reporting Results for trial block #" + repeatCount);
            Console.WriteLine("John got " + CorrectCounter + " correct out of " + NumberTrials + " trials (" +
                 accuracy + "%)");

            Console.WriteLine("By the end of trial block # " + repeatCount + ", John learned the following rules:");
            foreach (var i in John.GetInternals(Agent.InternalContainers.ACTION_RULES))
                Console.WriteLine(i.ToString());

            Console.SetOut(orig);
            Console.CursorLeft = 0;
            Console.WriteLine("Finished Trail Block #" + repeatCount + ", Accuracy = " +
                accuracy + "%");

            return accuracy == 100;
        }

        /// <summary>
        /// The event handler for new external action chosen events
        /// </summary>
        protected override void ProcessChosenExternalAction(Agent actor, ExternalActionChunk chosenAction, SensoryInformation relatedSI, 
            Dictionary<ActionChunk, double> finalActionActivations, long performedAt, long responseTime)
        {
            if ((bool)chosenAction.LabelAsIComparable)
            {
                //The agent said "True".
                if ((relatedSI["Boolean 1", true] == John.Parameters.MAX_ACTIVATION
                    && relatedSI["Boolean 2", false] == John.Parameters.MAX_ACTIVATION) ||
                    (relatedSI["Boolean 1", false] == John.Parameters.MAX_ACTIVATION
                    && relatedSI["Boolean 2", true] == John.Parameters.MAX_ACTIVATION))
                {
                    //The agent responded correctly
                    Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was correct");
                    //Record the agent's success.
                    CorrectCounter++;
                    //Give positive feedback.
                    John.ReceiveFeedback(relatedSI, 1.0);
                }
                else
                {
                    //The agent responded incorrectly
                    Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was incorrect");
                    //Give negative feedback.
                    John.ReceiveFeedback(relatedSI, 0.0);
                }
            }
            else
            {
                //The agent said "False".
                if ((relatedSI["Boolean 1", true] == John.Parameters.MAX_ACTIVATION
                    && relatedSI["Boolean 2", true] == John.Parameters.MAX_ACTIVATION) ||
                    (relatedSI["Boolean 1", false] == John.Parameters.MAX_ACTIVATION
                    && relatedSI["Boolean 2", false] == John.Parameters.MAX_ACTIVATION))
                {
                    //The agent responded correctly
                    Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was correct");
                    //Record the agent's success.
                    CorrectCounter++;
                    //Give positive feedback.
                    John.ReceiveFeedback(relatedSI, 1.0);
                }
                else
                {
                    //The agent responded incorrectly
                    Trace.WriteLineIf(World.LoggingSwitch.TraceWarning, "John was incorrect");
                    //Give negative feedback.
                    John.ReceiveFeedback(relatedSI, 0.0);
                }
            }

            trialWaitHold.Set();
        }

        #endregion
    }
}