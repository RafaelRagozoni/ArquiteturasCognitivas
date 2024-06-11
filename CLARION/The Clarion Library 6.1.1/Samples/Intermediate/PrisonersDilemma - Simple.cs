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
using Clarion.Framework.Templates;

namespace Clarion.Samples
{

    public class SimplePrisonersDilemma
    {
        public const int _ALICE = 0;
        public const int _BOB = 1;
        public const int _COOPERATE = 0;
        public const int _DEFECT = 1;

        public const int _MAX_REWARDS = 2;      // one neuron per reward value

        public const int _TRIALS = 10;
        public const int _ROUNDS = 200;

        private Agent Alice;
        private Agent Bob;

        private DimensionValuePair sayWhat;

        private ExternalActionChunk sayCooperate;
        private ExternalActionChunk sayDefect;

        private WorkingMemoryUpdateActionChunk wmuacC;
        private WorkingMemoryUpdateActionChunk wmuacD;

        private StreamWriter logFile;

        private int[,,] payoff;         // target agent, Alice's action, Bob's action
        private int maxpay;
        
        private int[,,] results;         // target agent, Alice's action, Bob's action

        static void Main(string[] args)
        {
            SimplePrisonersDilemma pd = new SimplePrisonersDilemma();

            // Initialize the task
            Console.WriteLine("Initializing the Simple Prisoner's Dilemma Task");

            // Many simulation models run for thousands of iterations, during which we collect
            // statistics. Here we set up the number of iterations to run, and a counter to track
            // the number that were performed correctly.

            World.LoggingLevel = TraceLevel.Warning;
            World.LoggingFileName = "TraceSPD.txt";

            // The next two lines of code let you set up a text file where all the results 
            // of your simulation run will be written to. Put the name of the file you 
            // want the results written to inside the CreateText method. 
            // We first grab the original console port in the variable called "orig".

            pd.logFile = new StreamWriter("SimplePrisonersDilemma.txt");                

            pd.Initialize();

            pd.Run();

            pd.FinishUp();  // It's not called Finalize because that's a reserved keyword in C#
        }

        public void Initialize()
        {
            // Dimension Value Pairs:
            sayWhat = World.NewDimensionValuePair("YourAction", "What do you want to do?");
 
            // External Action Chunks: 
            sayCooperate = World.NewExternalActionChunk("Cooperate");
            sayDefect = World.NewExternalActionChunk("Defect");

            // placeholder
            // GoalChunk salute = World.NewGoalChunk("Salute");
            // GoalChunk bidFarewell = World.NewGoalChunk("Bid Farewell");

            // WM Actions:
            wmuacC = World.NewWorkingMemoryUpdateActionChunk("Remember my opponent cooperated");
            wmuacD = World.NewWorkingMemoryUpdateActionChunk("Remember my opponent defected");
          
            DeclarativeChunk dcoc = World.NewDeclarativeChunk("My opponent cooperated");
            DeclarativeChunk dcod = World.NewDeclarativeChunk("My opponent defected");

            wmuacC.Add(WorkingMemory.RecognizedActions.SET_RESET, dcoc);
            wmuacD.Add(WorkingMemory.RecognizedActions.SET_RESET, dcod);

            // Set up a two agent model (meaning two agents with the same setup, playing against each other)
            Alice = World.NewAgent("Alice");
            Bob = World.NewAgent("Bob");
            
            // Simulating environment will determine inputs to each agent based on what each agent does..

            // Feedback is determined by payoff matrix..

            payoff = new int [2,2,2];

            // Doing this the hard way. Could set this up all in-line above, but this makes the table
            // more explicit in terms of how we want to use it.
            // The payoff matrix here is called "Friend or Foe", about the simplest case
            // indices mean: FOR-WHICH-AGENT, WHAT-ALICE-DOES, WHAT-BOB-DOES
            payoff[_ALICE, _COOPERATE, _COOPERATE] = 1;
            payoff[_ALICE, _COOPERATE, _DEFECT] = 0;
            payoff[_ALICE, _DEFECT, _COOPERATE] = 2;
            payoff[_ALICE, _DEFECT, _DEFECT] = 0;
            payoff[_BOB, _COOPERATE, _COOPERATE] = 1;
            payoff[_BOB, _COOPERATE, _DEFECT] = 2;
            payoff[_BOB, _DEFECT, _COOPERATE] = 0;
            payoff[_BOB, _DEFECT, _DEFECT] = 0;

            maxpay = 2;

            results = new int[_TRIALS, 2, 2];

            // Set up a Q-learning Net =
            // -- Eligibility Condition = True if "What do you want to do?" is in input, otherwise False
            // -- Input = "My opponent cooperated", "My opponent defected", "What do you want to do?"
            // -- Output = "I want to defect", "I want to cooperate"
            //
            // Also, RER is turned ON

            QBPNetwork net_A = AgentInitializer.InitializeImplicitDecisionNetwork(Alice, QBPNetwork.Factory, QNetEC);

            net_A.Input.Add(sayWhat);
            net_A.Input.Add(sayCooperate);
            net_A.Input.Add(sayDefect);
            net_A.Output.Add(sayCooperate);
            net_A.Output.Add(sayDefect);

            Alice.Commit(net_A);
            net_A.Parameters.LEARNING_RATE = 1;
            Alice.ACS.Parameters.PERFORM_RER_REFINEMENT = true; // it's true by default anyway
            Alice.ACS.Parameters.LEVEL_SELECTION_METHOD = ActionCenteredSubsystem.LevelSelectionMethods.COMBINED;
            Alice.ACS.Parameters.LEVEL_SELECTION_OPTION = ActionCenteredSubsystem.LevelSelectionOptions.FIXED;
            Alice.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 1;
            Alice.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 1;
            Alice.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 1;
            Alice.ACS.Parameters.WM_UPDATE_ACTION_PROBABILITY = 1;

            // Rules (2 rules) =
            // Rule 1: 
            // -- Condition = "Your opponent cooperated"
            // -- Action = Set "My opponent cooperated" in WM
            // Rule 2: 
            // -- Condition = "Your opponent defected"
            // -- Action = Set "My opponent defect" in WM

            FixedRule ruleA1 = AgentInitializer.InitializeActionRule(Alice, FixedRule.Factory, wmuacC, FRSC);
            FixedRule ruleA2 = AgentInitializer.InitializeActionRule(Alice, FixedRule.Factory, wmuacD, FRSC);
            Alice.Commit(ruleA1);
            Alice.Commit(ruleA2);

            QBPNetwork net_B = AgentInitializer.InitializeImplicitDecisionNetwork(Bob, QBPNetwork.Factory, QNetEC);

            net_B.Input.Add(sayWhat);
            net_B.Input.Add(sayCooperate);
            net_B.Input.Add(sayDefect);
            net_B.Output.Add(sayCooperate);
            net_B.Output.Add(sayDefect);

            Bob.Commit(net_B);

            // Use Weighted Combination
            // NO partial match on TL
            net_B.Parameters.LEARNING_RATE = 1;
            Bob.ACS.Parameters.PERFORM_RER_REFINEMENT = true;
            Bob.ACS.Parameters.LEVEL_SELECTION_METHOD = ActionCenteredSubsystem.LevelSelectionMethods.COMBINED;
            Bob.ACS.Parameters.LEVEL_SELECTION_OPTION = ActionCenteredSubsystem.LevelSelectionOptions.FIXED;
            Bob.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 1;
            Bob.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 1;
            Bob.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 1;
            Bob.ACS.Parameters.WM_UPDATE_ACTION_PROBABILITY = 1;

            FixedRule ruleB1 = AgentInitializer.InitializeActionRule(Bob, FixedRule.Factory, wmuacC, FRSC);
            FixedRule ruleB2 = AgentInitializer.InitializeActionRule(Bob, FixedRule.Factory, wmuacD, FRSC);
            Bob.Commit(ruleB1);
            Bob.Commit(ruleB2);

            // Initially using the same parameters for RER as Full Hello World
            RefineableActionRule.GlobalParameters.SPECIALIZATION_THRESHOLD_1 = -.6;
            RefineableActionRule.GlobalParameters.GENERALIZATION_THRESHOLD_1 = -.1;
            RefineableActionRule.GlobalParameters.INFORMATION_GAIN_OPTION = RefineableActionRule.IGOptions.PERFECT;
   
            /*
             * Note -- What should be seems is that when you pass in "Your opponent…", 
             * the agent should return the "Do Nothing" external action 
             * (since it performed an internal WM action).. 
             * However, you can just ignore this either way..
             */
        }

        private void Run()
        {
            SensoryInformation siA;
            SensoryInformation siB;

            ExternalActionChunk chosenA = ExternalActionChunk.DO_NOTHING;
            ExternalActionChunk chosenB = ExternalActionChunk.DO_NOTHING;

            double payA, payB;

            Random rand = new Random();

           // int indxA = _COOPERATE;  // 0 or 1 for Cooperate or Defect
          //  int indxB = _COOPERATE;  // 0 or 1 for Cooperate or Defect

            // In this run, we have Alice playing Rappoport's Tit for Tat (TFT) strategy
            // over a long series of trials, while Bob does whatever he wants.
            // Eventually, each should arrive at Cooperate. 

            for (int i = 0; i < _TRIALS; i++)
            {
                for (int j = 0; j < _ROUNDS; j++)
                {
                    PrintToConsole("Trial #" + i + ", Round #" + j +"     ");

                    if (chosenA == ExternalActionChunk.DO_NOTHING && chosenB == ExternalActionChunk.DO_NOTHING)
                    {
                        siA = World.NewSensoryInformation(Alice);
                        siB = World.NewSensoryInformation(Bob);

                        siA.Add(sayWhat, 1);
                        siB.Add(sayWhat, 1);

                        // Perceive the sensory information
                        Alice.Perceive(siA);
                        Bob.Perceive(siB);
                        
                        chosenA = Alice.GetChosenExternalAction(siA);
                        chosenB = Bob.GetChosenExternalAction(siB);
                    }
                    else
                    {
                        PrintToLog("OOPS");
                        throw new DivideByZeroException("OOPS");
                    }

                    payA = ComputePayoff(_ALICE, chosenA, chosenB);
                    payB = ComputePayoff(_BOB, chosenA, chosenB);
                    TallyResults(i, chosenA, chosenB);

                    PrintToLog("Alice gets " + payA + "; Bob gets " + payB);

                    Alice.ReceiveFeedback(siA, payA);
                    Bob.ReceiveFeedback(siB, payB);

                    siA = World.NewSensoryInformation(Alice);
                    siB = World.NewSensoryInformation(Bob);
                    
                    // Perceive the other player's chosen action
                    siA.Add(chosenB, 1);
                    siB.Add(chosenA, 1);

                    Alice.Perceive(siA);
                    Bob.Perceive(siB);

                    // Choose an action (Note: DO_NOTHING is expected here)
                    chosenA = Alice.GetChosenExternalAction(siA);
                    chosenB = Bob.GetChosenExternalAction(siB);
                }
            }
        }

        public double ComputePayoff(int agnt, ExternalActionChunk ac, ExternalActionChunk bc)
        {
            if (ac == sayCooperate)
            {
                if (bc == sayCooperate)
                    return (double)payoff[agnt, _COOPERATE, _COOPERATE] / (double)maxpay;
                else
                    return (double)payoff[agnt, _COOPERATE, _DEFECT] / (double)maxpay;
            }
            else
            {
                if (bc == sayCooperate)
                    return (double)payoff[agnt, _DEFECT, _COOPERATE] / (double)maxpay;
                else
                    return (double)payoff[agnt, _DEFECT, _DEFECT] / (double)maxpay;
            }
        }

        public void TallyResults(int itrial, ExternalActionChunk ac, ExternalActionChunk bc)
        {
            if (ac == sayCooperate)
            {
                if (bc == sayCooperate)
                    results[itrial, _COOPERATE, _COOPERATE]++;
                else
                    results[itrial, _COOPERATE, _DEFECT]++;
            }
            else
            {
                if (bc == sayCooperate)
                    results[itrial, _DEFECT, _COOPERATE]++;
                else
                    results[itrial, _DEFECT, _DEFECT]++;
            }
        }

        public bool QNetEligibility(ActivationCollection currentInput = null, ClarionComponent target = null)
        {
            return (currentInput.Contains(sayWhat, 1.0)) ? true : false;    
        }

        private EligibilityChecker QNetEC { get { return QNetEligibility; } }
        
        public double RuleSupport(ActivationCollection currentInput, Rule target = null)
        {
            return ((currentInput.Contains(sayCooperate, 1.0) && target.OutputChunk == wmuacC) ||
                (currentInput.Contains(sayDefect, 1.0) && target.OutputChunk == wmuacD)) ? 1.0 : 0.0;  
        }

        private SupportCalculator FRSC { get { return RuleSupport; } }
        
        private void FinishUp()
        {
            //Kill the agent to end the task
            Console.WriteLine("Killing Alice and Bob to end the program");
            Alice.Die();
            Bob.Die();

            Console.WriteLine("Alice and Bob are Dead");

            PrintToLog("");
            PrintToLog("          Alice   |   Alice    |   Alice    |   Alice    ");
            PrintToLog("       cooperates | cooperates |  defects   |  defects   ");
            PrintToLog("           Bob    |    Bob     |     Bob    |    Bob     ");
            PrintToLog("Trial  cooperates |  defects   | cooperates |  defects   ");
            PrintToLog("------------------|------------|------------|------------");

            for (int i = 0; i < _TRIALS; i++)
            {
                PrintToLog(i + "          " + results[i, _COOPERATE, _COOPERATE] + "          " + results[i, _COOPERATE, _DEFECT] + 
                    "           " + results[i, _DEFECT, _COOPERATE] + "           " + results[i, _DEFECT, _DEFECT]); 
            }

            Console.WriteLine("The Simple Prisoner's Dilemma Task has finished");
            Console.WriteLine("The results have been saved to \"SimplePrisonersDilemma.txt\"");
            Console.Write("Press any key to exit");
            Console.ReadKey(true);

            logFile.Close();
        }

        private void PrintToConsole(string s)
        {
            Console.CursorLeft = 0;
            Console.Write(s);
        }
        private void PrintToLog(string s)
        {
            logFile.WriteLine(s);
        }
    }
}
