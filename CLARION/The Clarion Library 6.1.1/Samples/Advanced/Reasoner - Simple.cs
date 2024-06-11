using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clarion;
using Clarion.Framework;

namespace Clarion.Samples
{
    /// <summary>
    /// Demonstrates a simple reasoning task using the NACS
    /// </summary>
    /// <remarks>
    /// The task demonstrated by this sample is fairly basic and was mainly written in order to show how to setup and use the reasoning mechanism within the NACS.
    /// The specifics of the task are as follows:
    /// <list type="bullet">
    /// <item>1x <see cref="HopfieldNetwork"/> acting as an Associative Memory Network (in the bottom level of the NACS)</item>
    /// <item>4x <see cref="AssociativeRule">AssociativeRules</see> (in the top level of the NACS)</item>
    /// <item>5x <see cref="DeclarativeChunk">DeclarativeChunks</see> (in the GKS, and encoded into the bottom level)</item>
    /// <item><b>Initialization - </b>
    /// <list type="number">
    /// <item>30 dimension-value pairs are initialized in the <see cref="World"/> and specified as nodes in the Hopfield network</item>
    /// <item>5 unique "patterns" of these dimension-value pairs are setup (as declarative chunks) and are added to the GKS</item>
    /// <item>All patterns are then encoded into the Hopfield network (using the <see cref="ImplicitComponentInitializer"/>)</item>
    /// <item>
    /// <para>4 associative rules are setup and added to the associative rule store. These rules are of the following form:</para>
    /// <code>If "Chunk X" then infer "Chunk X + 1"</code>
    /// </item>
    /// </list>
    /// </item>
    /// <item><b>Performing Reasoning - </b>
    /// <list type="bullet">
    /// <item><i>Parameters = </i> 2x Reasoning iterations, Top & Bottom Level set to "ON", Conclusion threshold set to 1 (i.e., no partial match), 40% noise added to input 
    /// for reasoning mechanism</item>
    /// <item>Each "pattern" is used (with noise) to setup the input into reasoning</item>
    /// <item><b>Outcomes - </b>
    /// <list type="bullet">
    /// <item><i>1st round of reasoning = </i> Bottom level will "reconstruct" the appropriate pattern based on the input</item>
    /// <item><i>2nd round of reasoning = </i> Based on the reconstructed pattern (and inferred chunk from bottom-up activation), the top level will infer the next chunk in
    /// the sequence</item>
    /// </list>
    /// </item>
    /// <item><b>Conclusions - </b>
    /// <list type="bullet">
    /// <item>The declarative chunk for the pattern associated with the noisy input</item>
    /// <item>The declarative chunk for the next pattern in the sequence</item>
    /// <item><i>Example = </i> If the input was constructed based on pattern #1, then the conclusions would be the chunk representing pattern #1, and the chunk representing
    /// pattern #2</item>
    /// </list>
    /// </item>
    /// </list>
    /// </item>
    /// </list>
    /// </remarks>
    class ReasonerSimple
    {
        /// <summary>
        /// A collection containing the dimension-value pairs used in this task
        /// </summary>
        static List<DistributedDimensionValuePair<int>> dvs = new List<DistributedDimensionValuePair<int>>();

        /// <summary>
        /// A collection containing the declarative chunk used in this task
        /// </summary>
        static List<DeclarativeChunk> chunks = new List<DeclarativeChunk>();

        /// <summary>
        /// The five unique patterns
        /// </summary>
        /// <remarks>Each integer value corresponds to a slot in the dimension-value pair list and indicates that dimension-value pair should be
        /// activated as part of the pattern</remarks>
        static int [][] patterns = 
        {
            new int [] {1, 3, 5, 11, 13, 16, 19, 23, 27},
            new int [] {3, 6, 7, 8, 12, 15, 20, 21, 26},
            new int [] {2, 4, 8, 9, 11, 17, 18, 24, 30},
            new int [] {1, 4, 10, 12, 15, 17, 19, 22, 29},
            new int [] {3, 5, 8, 10, 14, 18, 20, 25, 28}
        };
        
        /// <summary>
        /// Indicates the number of dimension-value pairs (i.e., nodes) that are in the Hopfield network on the bottom level of the NACS
        /// </summary>
        static int nodeCount = 30;

        /// <summary>
        /// Specifies the amount of "noise" to apply to the input into reasoning
        /// </summary>
        /// <remarks>
        /// <note type="implementnotes">Noise is not applied to the input in the conventional sense (i.e., randomly). Instead this value actually indicates
        /// the percentage of the pattern that gets "zeroed-out." For example, using the default of .4, the first 60% of the input will be constructed using the 
        /// pattern, but all of the dimension-value pairs for the last 40% will have activations set to 0.</remarks>
        static double noise = .4;

        public static void Main()
        {
            Agent reasoner = World.NewAgent();
            
            InitializeWorld(reasoner);

            //Adds all of the declarative chunks to the GKS 
            foreach (DeclarativeChunk dc in chunks)
                reasoner.AddKnowledge(dc);

            //Initializes the Hopfield network in the bottom level of the NACS
            HopfieldNetwork net = AgentInitializer.InitializeAssociativeMemoryNetwork
                (reasoner, HopfieldNetwork.Factory);

            //Species all of the dimension-value pairs as nodes for the Hopfield network
            net.Nodes.AddRange(dvs);

            //Commits the Hopfield network
            reasoner.Commit(net);

            //Encodes the patterns into the Hopfield network
            EncodeHopfieldNetwork(net);

            //Sets up the rules in the top level of the NAS
            SetupRules(reasoner);

            //Specifies that the NACS should perform 2 reasoning iterations
            reasoner.NACS.Parameters.REASONING_ITERATION_COUNT = 2;
            //Sets the conclusion threshold to 1 
            //(indicating that only fully matched conclusions should be returned)
            reasoner.NACS.Parameters.CONCLUSION_THRESHOLD = 1;

            //Initiates reasoning and outputs the results
            DoReasoning(reasoner);

            //Kills the reasoning agent
            reasoner.Die();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        /// <summary>
        /// This method initializes the dimension-value pairs and declarative chunks in the <see cref="World"/>
        /// </summary>
        static void InitializeWorld(Agent a)
        {
            //Initialize the dimension-value pairs
            for (int i = 1; i <= nodeCount; i++)
            {
                dvs.Add(World.NewDistributedDimensionValuePair(a, i));
            }

            //Initializes the declarative chunks
            for (int i = 0; i < patterns.Length; i++)
            {
                //Generates a declarative chunk and specifies that the semantic label associated with the declarative chunk should NOT be added to the
                //dimension-value pairs of that chunk
                DeclarativeChunk dc = 
                    World.NewDeclarativeChunk(i, addSemanticLabel:false);

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

        /// <summary>
        /// Encodes the patterns into the specified Hopfield network and then tests to make sure they have been successfully encoded
        /// </summary>
        /// <remarks>
        /// <note type="implementnotes">Most of the work that is done by this method is actually also performed by the implicit component initializer's
        /// <see cref="ImplicitComponentInitializer.Encode{T}(T, ImplicitComponentInitializer.EncodeTerminationConditions, int, ActivationCollection[])">
        /// Encode</see> method. However, we must separate the "encode" and "recall" phases in this example since we are using a different 
        /// <see cref="HopfieldNetwork.TransmissionOptions">transmission option</see> between these encoding process.</note>
        /// </remarks>
        /// <param name="net">the network where the patterns are to be encoded</param>
        static void EncodeHopfieldNetwork(HopfieldNetwork net)
        {
            //Tracks the accuracy of correctly encoded patterns
            double accuracy = 0;

            //Continue encoding until all of the patterns are successfully recalled
            do
            {
                //Specifies to use the "N spins" transmission option during the encoding phase
                net.Parameters.TRANSMISSION_OPTION =
                    HopfieldNetwork.TransmissionOptions.N_SPINS;

                List<ActivationCollection> sis = new List<ActivationCollection>();
                foreach (DeclarativeChunk dc in chunks)
                {
                    //Gets a new "data set" object (to be used by the Encode method to encode the pattern)
                    ActivationCollection si = ImplicitComponentInitializer.NewDataSet();

                    //Sets up the pattern
                    si.AddRange(dc, 1);

                    sis.Add(si);
                }

                //Encodes the pattern into the Hopfield network
                ImplicitComponentInitializer.Encode(net, sis);


                //Specifies to use the "let settle" transmission option during the testing phase
                net.Parameters.TRANSMISSION_OPTION =
                    HopfieldNetwork.TransmissionOptions.LET_SETTLE;

                //Tests the net to see if it has learned the patterns
                accuracy = ImplicitComponentInitializer.Encode(net, sis, testOnly: true);

                Console.WriteLine(((int)accuracy * 100) + "% of the patterns were successfully recalled.");
            } while (accuracy < 1);
        }

        /// <summary>
        /// Sets up the associative rules in the top level of the NACS
        /// </summary>
        /// <param name="reasoner">The agent in whose NACS the rules are being placed</param>
        static void SetupRules(Agent reasoner)
        {
            //Iterates through each of the chunks (except the last one, for obvious reasons) and creates an associative rule using that chunk as the 
            //condition, and the next chunk in the chunks list as the conclusion.
            for (int i = 0; i < chunks.Count - 1; i++)
            {
                //Initializes the rule
                RefineableAssociativeRule ar = 
                    AgentInitializer.InitializeAssociativeRule(reasoner, 
                    RefineableAssociativeRule.Factory, chunks[i + 1]);
                
                //Specifies that the current chunk must be activated as part of the condition for the rule
                ar.GeneralizedCondition.Add(chunks[i], true);

                //Commits the rule
                reasoner.Commit(ar);
            }
        }

        /// <summary>
        /// Performs reasoning using a "noisy" input based on each pattern
        /// </summary>
        /// <param name="reasoner">The reasoner who is performing the reasoning</param>
        static void DoReasoning(Agent reasoner)
        {
            int correct = 0;

            //Iterates through each pattern
            foreach (DeclarativeChunk dc in chunks)
            {
                //Gets an input to use for reasoning. Note that the World.GetSensoryInformation method can also be used here
                ActivationCollection si = ImplicitComponentInitializer.NewDataSet();

                int count = 0;

                //Sets up the input
                foreach (DimensionValuePair dv in dvs)
                {
                    if (((double)count / (double)dc.Count < (1 - noise)))
                    {
                        if (dc.Contains(dv))
                        {
                            si.Add(dv, 1);
                            ++count;
                        }
                        else
                            si.Add(dv, 0);
                    }
                    else
                        si.Add(dv, 0);      //Zeros out the dimension-value pair if "above the noise level"
                }

                Console.WriteLine("Input to reasoner:\r\n" + si);

                Console.WriteLine("Output from reasoner:");

                //Performs reasoning based on the input. The conclusions returned from this method will be in the form of a
                //collection of "Chunk Tuples." A chunk tuple is simply just a chunk combined with its associated activation.
                var o = reasoner.NACS.PerformReasoning(si);

                //Iterates through the conclusions from reasoning
                foreach (var i in o)
                {
                    Console.WriteLine(i.CHUNK);
                    if (i.CHUNK == dc)
                        correct++;
                }
            }
            Console.WriteLine("Retrieval Accuracy: " + 
                (int)(((double)correct / (double)chunks.Count) * 100) + "%");
        }
    }
}
