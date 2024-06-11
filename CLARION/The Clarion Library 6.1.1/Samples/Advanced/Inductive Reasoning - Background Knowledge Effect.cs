using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.IO;
using Clarion;
using Clarion.Framework;
using Clarion.Plugins;
using Clarion.Framework.Extensions;
using Clarion.Framework.Extensions.Templates;
using Clarion.Framework.Templates;

namespace Clarion.Samples
{
    /// <summary>
    /// Demonstrates the Background Knowledge effect in Inductive Reasoning
    /// </summary>
    /// <remarks>
    /// This task is taken from the Inductive Reasoning chapter of The Cambridge Handbook of Computational Psychology by Ron Sun
    /// 
    /// <para>
    /// Authors: Daniel Cannon & Nicholas Wilson
    /// </para>
    /// </remarks>
    class BackgroundKnowledge
    {
        public static void Main()
        {
            Console.WriteLine("Demonstrating Inductive Reasoning: Background Knowledge Effect");
            Agent reasoner = World.NewAgent();

            //Sets up and trains the background knowledge into the bottom level of the NACS
            SetupBPNetwork(reasoner);

            reasoner.NACS.Parameters.REASONING_ITERATION_COUNT = 1;
           
            reasoner.NACS.Parameters.CONCLUSION_THRESHOLD = .1;

            reasoner.NACS.Parameters.RETURN_INPUTS_AS_CONCLUSIONS = false;

            Console.WriteLine("Performing Reasoning");
            //Initiates reasoning and outputs the results
            DoReasoning(reasoner);

            //Kills the reasoning agent
            reasoner.Die();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
       
        static void SetupBPNetwork(Agent reasoner)
        {    
            //Chunks for the whales, tuna, and bears
            DeclarativeChunk TunaChunk = World.NewDeclarativeChunk("Tuna");
            DeclarativeChunk WhaleChunk = World.NewDeclarativeChunk("Whale");
            DeclarativeChunk BearChunk = World.NewDeclarativeChunk("Bear");

            //The 2 properties (as DV pairs)
            DimensionValuePair livesinwater = World.NewDimensionValuePair("lives in", "water");
            DimensionValuePair eatsfish = World.NewDimensionValuePair("eats", "fish");

            //The BP network to be used in the bottom level of the NACS
            BPNetwork net = AgentInitializer.InitializeAssociativeMemoryNetwork(reasoner, BPNetwork.Factory);

            //Adds the properties (as inputs) and chunks (as outputs) to the BP network
            net.Input.Add(livesinwater);
            net.Input.Add(eatsfish);
            net.Output.Add(TunaChunk);
            net.Output.Add(WhaleChunk);
            net.Output.Add(BearChunk);

            reasoner.Commit(net);
            
            //Adds the chunks to the GKS
            reasoner.AddKnowledge(TunaChunk);
            reasoner.AddKnowledge(WhaleChunk);
            reasoner.AddKnowledge(BearChunk);

            //Initializes a trainer to use to train the BP network
            GenericEquation trainer = ImplicitComponentInitializer.InitializeTrainer(GenericEquation.Factory, (Equation)trainerEQ);

            //Adds the properties (as inputs) and chunks (as outputs) to the trainer
            trainer.Input.Add(livesinwater);
            trainer.Input.Add(eatsfish);
            trainer.Output.Add(TunaChunk);
            trainer.Output.Add(WhaleChunk);
            trainer.Output.Add(BearChunk);

            trainer.Commit();
            
            //Sets up data sets for each of the 2 properties
            List<ActivationCollection> sis = new List<ActivationCollection>();
            ActivationCollection si = ImplicitComponentInitializer.NewDataSet();
            si.Add(livesinwater, 1);
            sis.Add(si);

            si = ImplicitComponentInitializer.NewDataSet();
            si.Add(eatsfish, 1);
            sis.Add(si);

            Console.Write("Training AMN...");
            //Trains the BP network to report associative knowledge between the properties and the chunks
            ImplicitComponentInitializer.Train(net, trainer, sis, ImplicitComponentInitializer.TrainingTerminationConditions.SUM_SQ_ERROR);
            Console.WriteLine("Finished!");
        }

        /// <summary>
        /// The training equation that is used to train the BP network in the bottom level of the NACS
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        static void trainerEQ(ActivationCollection input, ActivationCollection output)
        {
            //If the input is the "lives in water" property, the activations for each chunk are as follows:
            if (input.Contains(World.GetDimensionValuePair("lives in", "water"), 1))
            {
                output[World.GetDeclarativeChunk("Tuna")] = 1;      //Tunas and whales live in water
                output[World.GetDeclarativeChunk("Whale")] = 1;
                output[World.GetDeclarativeChunk("Bear")] = .5;     //Bears don't exactly live in water, but they do spend some time in it
            }

            //If the input is the "eats fish" property, the activations for each chunk are as follows:
            else if (input.Contains(World.GetDimensionValuePair("eats", "fish"), 1))
            {
                output[World.GetDeclarativeChunk("Tuna")] = .2;     //Tuna aren't really known to eat other fish (although they may eat smaller fish)
                output[World.GetDeclarativeChunk("Whale")] = .75;   //Whales definitely eat other fish (although not exclusively)
                output[World.GetDeclarativeChunk("Bear")] = 1;      //Everyone knows bears love salmon!
            }
        }

        static void DoReasoning(Agent reasoner)
        {
            /////Property Test: "lives in water"/////
            SensoryInformation si = World.NewSensoryInformation(reasoner);

            //Used to compare activations later
            double TunaWhale = 0;
            double WhaleBear = 0;

            //Adds the "lives in water" property to the input
            si.Add(World.GetDimensionValuePair("lives in", "water"), 1);

            var o = reasoner.NACS.PerformReasoning(si);

            //Checks each conclusion chunk and adds up the activations
            foreach (var i in o)
            {
                if ((string)i.CHUNK.LabelAsIComparable == "Tuna")
                    TunaWhale += i.ACTIVATION;
                else if ((string)i.CHUNK.LabelAsIComparable == "Whale")
                {
                    TunaWhale += i.ACTIVATION;
                    WhaleBear += i.ACTIVATION;
                }
                else if ((string)i.CHUNK.LabelAsIComparable == "Bear")
                    WhaleBear += i.ACTIVATION;
            }

            Console.WriteLine();
            Console.WriteLine("Which animal pairing is more likely to live in water?");  

            if (TunaWhale > WhaleBear)
                Console.WriteLine("A tuna and whale (" + Math.Round(TunaWhale, 2) + " vs. " + Math.Round(WhaleBear, 2) + ")");
            else
                Console.WriteLine("A bear and whale (" + Math.Round(WhaleBear, 2) + " vs. " + Math.Round(TunaWhale, 2) + ")");

            /////Property Test: "eats fish"/////
            si = World.NewSensoryInformation(reasoner);

            //Resets the activation counters
            TunaWhale = 0;
            WhaleBear = 0;

            //Adds the "eats fish" property to the input
            si.Add(World.GetDimensionValuePair("eats", "fish"), 1);

            o = reasoner.NACS.PerformReasoning(si);

            //Checks each conclusion chunk and adds up the activations
            foreach (var i in o)
            {
                if ((string)i.CHUNK.LabelAsIComparable == "Tuna")
                    TunaWhale += i.ACTIVATION;
                else if ((string)i.CHUNK.LabelAsIComparable == "Whale")
                {
                    TunaWhale += i.ACTIVATION;
                    WhaleBear += i.ACTIVATION;
                }
                else if ((string)i.CHUNK.LabelAsIComparable == "Bear")
                    WhaleBear += i.ACTIVATION;
            }

            Console.WriteLine();
            Console.WriteLine("Which animal pairing is more likely to eat fish?");  
            if (TunaWhale > WhaleBear)
                Console.WriteLine("A tuna and whale (" + Math.Round(TunaWhale, 2) + " vs. " + Math.Round(WhaleBear, 2) + ")");
            else
                Console.WriteLine("A bear and whale (" + Math.Round(WhaleBear, 2) + " vs. " + Math.Round(TunaWhale, 2) + ")");
        }
    }
}
