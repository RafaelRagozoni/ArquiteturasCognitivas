using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clarion;
using Clarion.Framework;

namespace Clarion.Samples
{
    /// <summary>
    /// Demonstrates the Diversity effect in Inductive Reasoning
    /// </summary>
    /// <remarks>
    /// This task is taken from the Inductive Reasoning chapter of The Cambridge Handbook of Computational Psychology by Ron Sun
    /// 
    /// <para>
    /// Authors: Daniel Cannon & Nicholas Wilson
    /// </para>
    /// </remarks>
    class Diversity
    {
        static List<DimensionValuePair<string, int>> dvs = new List<DimensionValuePair<string, int>>();

        static List<DeclarativeChunk> chunks = new List<DeclarativeChunk>();

        static int[][] patterns = 
        {
            //mammal
            new int [] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16},

            //hippo
            new int [] {1, 3, 4, 5, 6, 8, 11, 14, 15},

            //rhino
            new int [] {1, 3, 4, 5, 6, 8, 9, 13, 16},

            //hamster
            new int [] {1, 2, 3, 4, 6, 7, 10, 12, 14}
        };

        static int nodeCount = 16;

        public static void Main()
        {
            Console.WriteLine("Demonstrating Inductive Reasoning: Diversity Effect");

            InitializeWorld();

            //agent for reasoning
            Agent reasoner = World.NewAgent();

            //Adds all of the declarative chunks to the GKS 
            foreach (DeclarativeChunk dc in chunks)
                reasoner.AddKnowledge(dc);


            //Specifies that the NACS should perform 1 reasoning iterations
            reasoner.NACS.Parameters.REASONING_ITERATION_COUNT = 1;
            //Sets the conclusion threshold to .05
            reasoner.NACS.Parameters.CONCLUSION_THRESHOLD = 0.05;
            //Indicates that any chunks used as input should not be returned as part of the conclusion
            reasoner.NACS.Parameters.RETURN_INPUTS_AS_CONCLUSIONS = false;

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
            //Initialize the dimension-value pairs
            for (int i = 1; i <= nodeCount; i++)
            {
                //names for features
                if (i == 1)
                    feature = "lactates";
                else if (i == 2)
                    feature = "hair/fur";
                else if (i == 3)
                    feature = "warm-blooded";
                else if (i == 4)
                    feature = "teeth";
                else if (i == 5)
                    feature = "tails";
                else if (i == 6)
                    feature = "lives on land";
                else if (i == 7)
                    feature = "omnivores";
                else if (i == 8)
                    feature = "herbivores";
                else if (i == 9)
                    feature = "carnivores";
                else if (i == 10)
                    feature = "small";
                else if (i == 11)
                    feature = "large";
                else if (i == 12)
                    feature = "something";
                else if (i == 13)
                    feature = "wild";
                else if (i == 14)
                    feature = "domesticated";
                else if (i == 15)
                    feature = "swims";
                else if (i == 16)
                    feature = "horns";

                dvs.Add(World.NewDimensionValuePair(feature, i));
            }

            //Initializes the declarative chunks
            for (int i = 0; i < patterns.Length; i++)
            {
                //Generates a declarative chunk
                string mammal_name = " ";
                if (i == 0)
                    mammal_name = "mammal";
                else if (i == 1)
                    mammal_name = "hippo";
                else if (i == 2)
                    mammal_name = "rhino";
                else if (i == 3)
                    mammal_name = "hamster";

                DeclarativeChunk dc =
                    World.NewDeclarativeChunk(mammal_name, addSemanticLabel: false);

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
            double act1 = 0;
            double act2 = 0;

            //Gets an input to use for reasoning. Note that the World.GetSensoryInformation method can also be used here
            ActivationCollection hiprhi = ImplicitComponentInitializer.NewDataSet();
            ActivationCollection hipham = ImplicitComponentInitializer.NewDataSet();

            //Sets up the input
            foreach (DimensionValuePair dv in dvs)
            {
                if (chunks[1].Contains(dv) || chunks[2].Contains(dv))
                    hiprhi.Add(dv, 1);
                if (chunks[1].Contains(dv) || chunks[3].Contains(dv))
                    hipham.Add(dv, 1);
                if (!chunks[1].Contains(dv) && !chunks[2].Contains(dv) && !chunks[3].Contains(dv))
                {
                    hiprhi.Add(dv, 0);
                    hipham.Add(dv, 0);
                }
            }

            Console.WriteLine("Using the features for \"hippo\" and \"rhino\" as input into reasoner:\r\n" + hiprhi);
            Console.WriteLine();
            Console.WriteLine("Output from reasoner:");

            //Performs reasoning based on the input
            var o = reasoner.NACS.PerformReasoning(hiprhi);

            //Iterates through the conclusions from reasoning
            foreach (var i in o)
            {
                //Checks to see if it has the right chunk (i.e., mammals)
                if (i.CHUNK == chunks[0])
                {
                    Console.WriteLine(i.CHUNK);
                    Console.WriteLine("The activation of the \"mammal\" chunk based on \"hippo\" and \"rhino\" is: " + Math.Round(i.ACTIVATION, 2));
                    act1 = i.ACTIVATION;
                }
                else
                    continue;
                Console.WriteLine();
            }

            Console.WriteLine("Using the features for \"hippo\" and \"hamster\" as input into reasoner:\r\n" + hiprhi);
            Console.WriteLine();
            Console.WriteLine("Output from reasoner:");

            //Performs reasoning based on the input. The conclusions returned from this method will be in the form of a
            //collection of "Chunk Tuples." A chunk tuple is simply just a chunk combined with its associated activation.
            var k = reasoner.NACS.PerformReasoning(hipham);

            //Iterates through the conclusions from reasoning
            foreach (var i in k)
            {
                if (i.CHUNK == chunks[0])
                {
                    Console.WriteLine(i.CHUNK);
                    Console.WriteLine("The activation of the \"mammal\" chunk based on \"hippo\" and \"hamster\" is: " + Math.Round(i.ACTIVATION, 2));
                    act2 = i.ACTIVATION;
                }
                else
                    continue;
                Console.WriteLine();
            }

            Console.WriteLine("Which animal combination is a stronger representation of a mammal?");
            if (act1 > act2)
                Console.WriteLine("A hippo and rhino, because they activate the mammal chunk more");
            else
                Console.WriteLine("A hippo and hamster, because they activate the mammal chunk more");
        }
    }
}

