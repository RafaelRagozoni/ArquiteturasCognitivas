using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clarion;
using Clarion.Framework;

namespace Clarion.Samples
{
    /// <summary>
    /// Demonstrates the Typicality effect in Inductive Reasoning
    /// </summary>
    /// <remarks>
    /// This task is taken from the Inductive Reasoning chapter of The Cambridge Handbook of Computational Psychology by Ron Sun
    /// 
    /// <para>
    /// Authors: Daniel Cannon & Nicholas Wilson
    /// </para>
    /// </remarks>
    class Typicality
    {
        static List<DimensionValuePair<string, int>> dvs = new List<DimensionValuePair<string, int>>();

        static List<DeclarativeChunk> chunks = new List<DeclarativeChunk>();

        static int[][] patterns = 
        {
            //bird
            new int [] {1, 3, 4, 5, 8, 9, 11, 12},

            //robin
            new int [] {1, 3, 4, 5, 8, 9, 11, 12, 13},

            //penguin
            new int [] {2, 3, 4, 7, 8, 10, 11, 14}
        };

        static int nodeCount = 14;

        public static void Main()
        {
            Console.WriteLine("Demonstrating Inductive Reasoning: Typicality Effect");

            InitializeWorld();

            Agent reasoner = World.NewAgent();

            //Adds all of the declarative chunks to the GKS 
            foreach (DeclarativeChunk dc in chunks)
                reasoner.AddKnowledge(dc);

            //Specifies that the NACS should perform 1 reasoning iterations
            reasoner.NACS.Parameters.REASONING_ITERATION_COUNT = 1;
            //Sets the conclusion threshold to .1
            reasoner.NACS.Parameters.CONCLUSION_THRESHOLD = .1;

            //Initiates reasoning and outputs the results
            DoReasoning(reasoner);

            //Kills the reasoning agent
            reasoner.Die();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static void InitializeWorld()
        {
            string feature = " ";
            //Initializes the dimension-value pairs
            for (int i = 1; i <= nodeCount; i++)
            {
                if (i == 1)
                    feature = "wings";
                else if (i == 2)
                    feature = "flippers";
                else if (i == 3)
                    feature = "beaks";
                else if (i == 4)
                    feature = "lays eggs";
                else if (i == 5)
                    feature = "migrates in winter";
                else if (i == 6)
                    feature = "eats bugs and berries";
                else if (i == 7)
                    feature = "eats fish";
                else if (i == 8)
                    feature = "builds nests";
                else if (i == 9)
                    feature = "flies";
                else if (i == 10)
                    feature = "swims";
                else if (i == 11)
                    feature = "feathers";
                else if (i == 12)
                    feature = "colorful";
                else if (i == 13)
                    feature = "lives in trees";
                else if (i == 14)
                    feature = "lives in the polar regions";

                dvs.Add(World.NewDimensionValuePair(feature, i));
            }

            //Initializes the declarative chunks
            for (int i = 0; i < patterns.Length; i++)
            {
                //Generates a declarative chunk
                string bird_name = " ";
                if (i == 0)
                    bird_name = "birds";
                else if (i == 1)
                    bird_name = "robins";
                else if (i == 2)
                    bird_name = "penguins";

                DeclarativeChunk dc =
                    World.NewDeclarativeChunk(bird_name, addSemanticLabel: false);

                //Adds the appropriate dimension-value pairs (as indicated by the "patterns" array) for each declarative chunk pattern representation
                foreach (var dv in dvs)
                {
                    if (patterns[i].Contains(dv.Value))
                    {
                        dc.Add(dv);
                    }
                }

                //Adds the declarative chunk to the chunks list
                chunks.Add(dc);
            }
        }



        static void DoReasoning(Agent reasoner)
        {
            //Gets an input to use for reasoning. Note that the World.GetSensoryInformation method can also be used here
            ActivationCollection si = ImplicitComponentInitializer.NewDataSet();

            //activation values 
            double act1 = 0;
            double act2 = 0;
            //Sets up the input
            foreach (DimensionValuePair dv in dvs)
            {
                if (chunks[0].Contains(dv))
                {
                    si.Add(dv, 1);

                }
            }

            Console.WriteLine("Using features for \"bird\" as input to reasoner:\r\n" + si);
            Console.WriteLine();
            Console.WriteLine("Output from reasoner:");

            //Performs reasoning based on the input. 
            var o = reasoner.NACS.PerformReasoning(si);

            //Iterates through the conclusions from reasoning
            foreach (var i in o)
            {
                // if the bird chunk, skip
                if (i.CHUNK == chunks[0])
                    continue;
                else
                {
                    
                    Console.WriteLine(i.CHUNK);
                    //if the robin chunk, set the first activation value
                    if (i.CHUNK == chunks[1])
                        act1 = i.ACTIVATION;
                    //otherwise, set the second activation value for penguins    
                    else
                        act2 = i.ACTIVATION;
                    Console.WriteLine("Activation of \"" + i.CHUNK.LabelAsIComparable + "\" chunk based on \"bird\" features: " + Math.Round(i.ACTIVATION, 2));

                }
                Console.WriteLine();
            }

            Console.WriteLine("Which is more typical of a bird?");

            if (act1 > act2)
                Console.WriteLine("A robin because its chunk activation is higher (" + Math.Round(act1, 2) + " vs. " + Math.Round(act2, 2) + ")");
            else
                Console.WriteLine("A penguin because its chunk activation is higher (" + Math.Round(act2, 2) + " vs. " + Math.Round(act1, 2) + ")");
        }
    }
}
