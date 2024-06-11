using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Extensions;
using Clarion.Framework.Templates;

namespace Clarion.Samples
{
    public class HelloWorldFull
    {
        static void Main(string[] args)
        {
            //Initialize the task
            Console.WriteLine("Initializing the Full Hello World Task");

            int CorrectCounter = 0;
            int NumberTrials = 20000;

            Random rand = new Random();

            World.LoggingLevel = TraceLevel.Off;

            int progress = 0;

            TextWriter orig = Console.Out;
            StreamWriter sw = File.CreateText("HelloWorldFull.txt");

            DimensionValuePair hi = World.NewDimensionValuePair("Salutation", "Hello");
            DimensionValuePair bye = World.NewDimensionValuePair("Salutation", "Goodbye");

            ExternalActionChunk sayHi = World.NewExternalActionChunk("Hello");
            ExternalActionChunk sayBye = World.NewExternalActionChunk("Goodbye");

            GoalChunk salute = World.NewGoalChunk("Salute");
            GoalChunk bidFarewell = World.NewGoalChunk("Bid Farewell");

            //Initialize the Agent
            Agent John = World.NewAgent("John");

            SimplifiedQBPNetwork net = AgentInitializer.InitializeImplicitDecisionNetwork(John, SimplifiedQBPNetwork.Factory);

            net.Input.Add(salute, "goals");
            net.Input.Add(bidFarewell, "goals");

            net.Input.Add(hi);
            net.Input.Add(bye);

            net.Output.Add(sayHi);
            net.Output.Add(sayBye);

            net.Parameters.LEARNING_RATE = 1;

            John.Commit(net);

            John.ACS.Parameters.VARIABLE_BL_BETA = .5;
            John.ACS.Parameters.VARIABLE_RER_BETA = .5;
            John.ACS.Parameters.VARIABLE_IRL_BETA = 0;
            John.ACS.Parameters.VARIABLE_FR_BETA = 0;

            RefineableActionRule.GlobalParameters.SPECIALIZATION_THRESHOLD_1 = -.6;
            RefineableActionRule.GlobalParameters.GENERALIZATION_THRESHOLD_1 = -.1;
            RefineableActionRule.GlobalParameters.INFORMATION_GAIN_OPTION = RefineableActionRule.IGOptions.PERFECT;

            AffiliationBelongingnessDrive ab = AgentInitializer.InitializeDrive(John, AffiliationBelongingnessDrive.Factory, rand.NextDouble(), (DeficitChangeProcessor)HelloWorldFull_DeficitChange);

            DriveEquation abd = AgentInitializer.InitializeDriveComponent(ab, DriveEquation.Factory);

            ab.Commit(abd);

            John.Commit(ab);

            AutonomyDrive aut = AgentInitializer.InitializeDrive(John, AutonomyDrive.Factory, rand.NextDouble(), (DeficitChangeProcessor)HelloWorldFull_DeficitChange);

            DriveEquation autd =
                AgentInitializer.InitializeDriveComponent(aut, DriveEquation.Factory);

            aut.Commit(autd);

            John.Commit(aut);

            GoalSelectionModule gsm = 
                AgentInitializer.InitializeMetaCognitiveModule(John, GoalSelectionModule.Factory);

            GoalSelectionEquation gse = 
                AgentInitializer.InitializeMetaCognitiveDecisionNetwork(gsm, GoalSelectionEquation.Factory);

            gse.Input.Add(ab.GetDriveStrength());
            gse.Input.Add(aut.GetDriveStrength());

            GoalStructureUpdateActionChunk su = World.NewGoalStructureUpdateActionChunk();
            GoalStructureUpdateActionChunk bu = World.NewGoalStructureUpdateActionChunk();

            su.Add(GoalStructure.RecognizedActions.SET_RESET, salute);
            bu.Add(GoalStructure.RecognizedActions.SET_RESET, bidFarewell);

            gse.Output.Add(su);
            gse.Output.Add(bu);

            gsm.SetRelevance(su, ab, 1);
            gsm.SetRelevance(bu, aut, 1);

            gsm.Commit(gse);

            John.Commit(gsm);

            John.MS.Parameters.CURRENT_GOAL_ACTIVATION_OPTION =
                MotivationalSubsystem.CurrentGoalActivationOptions.FULL;

            //Run the task
            Console.WriteLine("Running the Full Hello World Task");
            Console.SetOut(sw);

            SensoryInformation si;

            ExternalActionChunk chosen;

            for (int i = 0; i < NumberTrials; i++)
            {
                si = World.NewSensoryInformation(John);

                si[AffiliationBelongingnessDrive.MetaInfoReservations.STIMULUS, typeof(AffiliationBelongingnessDrive).Name] = 1;
                si[AutonomyDrive.MetaInfoReservations.STIMULUS, typeof(AutonomyDrive).Name] = 1;

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

            Console.WriteLine("Reporting Results for the Full Hello World Task");
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

            Console.WriteLine("The Full Hello World Task has finished");
            Console.WriteLine("The results have been saved to \"HelloWorldFull.txt\"");
            Console.Write("Press any key to exit");
            Console.ReadKey(true);
        }

        public static double HelloWorldFull_DeficitChange(ActivationCollection si, Drive target)
        {
            var cg = ((SensoryInformation)si).AffiliatedAgent.CurrentGoal;
            if (cg != null)
            {
                if ((cg == World.GetGoalChunk("Salute") && target is AffiliationBelongingnessDrive) ||
                    (cg == World.GetGoalChunk("Bid Farewell") && target is AutonomyDrive))
                    target.Parameters.DEFICIT_CHANGE_RATE = .999;
                else
                    target.Parameters.DEFICIT_CHANGE_RATE = 1.001;
            }

            return target.Deficit * target.Parameters.DEFICIT_CHANGE_RATE;
        }
    }
}