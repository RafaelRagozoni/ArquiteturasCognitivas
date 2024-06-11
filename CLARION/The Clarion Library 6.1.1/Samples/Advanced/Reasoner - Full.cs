using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Templates;
using Clarion.Framework.Extensions.Templates;
using Clarion.Framework.Core;

namespace Clarion.Samples
{
    /// <summary>
    /// Demonstrates a simple reasoning task that integrates the ACS and NACS
    /// </summary>
    /// <remarks>
    /// This task is a variation on the "Simple Reasoner" task. It is meant to demonstrate how the ACS and NACS can be used in conjunction.
    /// Unlike the "Simple Reasoner", however, this task only makes use of the bottom level of the NACS (and the top level of the ACS).
    /// 
    /// <para>
    /// Authors: Shane Bretz and Nicholas Wilson
    /// </para>
    /// </remarks>
    class ReasonerFull
    {
        /// <summary>
        /// A collection containing the dimension-value pairs used in this task
        /// </summary>
        static List<DimensionValuePair<string, int>> dvs = new List<DimensionValuePair<string, int>>();

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

        public static void Main ()
		{

			Agent reasoner = World.NewAgent ();

			InitializeWorld (reasoner);

			//Adds all of the declarative chunks to the GKS 
			foreach (DeclarativeChunk dc in chunks)
				reasoner.AddKnowledge (dc);

			//Initializes the Hopfield network in the bottom level of the NACS
			HopfieldNetwork net = AgentInitializer.InitializeAssociativeMemoryNetwork
                (reasoner, HopfieldNetwork.Factory);

			//Species all of the dimension-value pairs as nodes for the Hopfield network
			net.Nodes.AddRange (dvs);

			//Commits the Hopfield network
			reasoner.Commit (net);

			//Encodes the patterns into the Hopfield network
			EncodeHopfieldNetwork (net);

			//Specifies that the NACS should perform 2 reasoning iterations
			reasoner.NACS.Parameters.REASONING_ITERATION_COUNT = 1;
			//Sets the conclusion threshold to 1 
			//(indicating that only fully matched conclusions should be returned)
			reasoner.NACS.Parameters.CONCLUSION_THRESHOLD = 1;
			
			// Add Some Action Chunks for the ACS
			World.NewExternalActionChunk ("Yes");
			World.NewExternalActionChunk ("No");
			ReasoningRequestActionChunk think = World.NewReasoningRequestActionChunk ("DoReasoning");
			think.Add (NonActionCenteredSubsystem.RecognizedReasoningActions.NEW, 1, false);

			World.NewDimensionValuePair ("state", 1);
			World.NewDimensionValuePair ("state", 2);
			World.NewDimensionValuePair ("state", 3);
			
			// Add ACS Rule to use chunks
            RefineableActionRule yes = AgentInitializer.InitializeActionRule (reasoner, RefineableActionRule.Factory, World.GetActionChunk ("Yes"));
            yes.GeneralizedCondition.Add (World.GetDeclarativeChunk (1), true);
            reasoner.Commit (yes);
			
            RefineableActionRule no = AgentInitializer.InitializeActionRule (reasoner, RefineableActionRule.Factory, World.GetActionChunk ("No"));
            no.GeneralizedCondition.Add (World.GetDeclarativeChunk (0), true, "altdim");
            no.GeneralizedCondition.Add (World.GetDeclarativeChunk (2), true, "altdim");
            no.GeneralizedCondition.Add (World.GetDeclarativeChunk (3), true, "altdim");
            no.GeneralizedCondition.Add (World.GetDeclarativeChunk (4), true, "altdim");
            reasoner.Commit (no);
			
			RefineableActionRule doReasoning = AgentInitializer.InitializeActionRule (reasoner, RefineableActionRule.Factory, World.GetActionChunk ("DoReasoning"));
			doReasoning.GeneralizedCondition.Add (World.GetDimensionValuePair ("state", 1));
			reasoner.Commit (doReasoning);

			RefineableActionRule doNothing = AgentInitializer.InitializeActionRule (reasoner, RefineableActionRule.Factory, ExternalActionChunk.DO_NOTHING);
			doNothing.GeneralizedCondition.Add (World.GetDimensionValuePair ("state", 2));
			reasoner.Commit (doNothing);
			
			reasoner.ACS.Parameters.PERFORM_RER_REFINEMENT = false;
			reasoner.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 0;
			reasoner.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 1;
			reasoner.ACS.Parameters.FIXED_IRL_LEVEL_SELECTION_MEASURE = 0;
			reasoner.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 0;
			reasoner.ACS.Parameters.LEVEL_SELECTION_METHOD = ActionCenteredSubsystem.LevelSelectionMethods.STOCHASTIC;
			reasoner.ACS.Parameters.LEVEL_SELECTION_OPTION = ActionCenteredSubsystem.LevelSelectionOptions.FIXED;
			reasoner.ACS.Parameters.NACS_REASONING_ACTION_PROBABILITY = 1;
			reasoner.ACS.Parameters.EXTERNAL_ACTION_PROBABILITY = 1;
			reasoner.NACS.Parameters.REASONING_ITERATION_TIME = 3000;

			//Initiates the simulation and outputs the results
			Run (reasoner);

			//Kills the reasoning agent
			reasoner.Die ();

			Console.WriteLine ("Press any key to exit");
			Console.ReadKey ();
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
            } while (accuracy < 1);
        }

        /// <summary>
        /// Performs reasoning using a "noisy" input based on each pattern
        /// </summary>
        /// <param name="reasoner">The reasoner who is performing the reasoning</param>
        static void Run (Agent reasoner)
		{
			int pcounter = 0;
			//Iterates through each pattern
			foreach (DeclarativeChunk dc in chunks) {
				//Gets an input to use for reasoning. Note that the World.GetSensoryInformation method can also be used here
				ExternalActionChunk chosen = null;
			
				++pcounter;
				Console.Write("Presenting degraded pattern ");
				Console.WriteLine(pcounter);
			
				int state_counter = 1;
				while (chosen == null || chosen == ExternalActionChunk.DO_NOTHING) 
                {
					SensoryInformation si = World.NewSensoryInformation (reasoner);
					si.Add (World.GetDimensionValuePair ("state", state_counter), 1);

					int count = 0;
					//Sets up the input
					foreach (DimensionValuePair dv in dvs) {
						if (((double)count / (double)dc.Count < (1 - noise))) {
							if (dc.Contains (dv)) {
								si.Add (dv, 1);
								++count;
							} else
								si.Add (dv, 0);
						} else
							si.Add (dv, 0);      //Zeros out the dimension-value pair if "above the noise level"
					}

					reasoner.Perceive (si);
					chosen = reasoner.GetChosenExternalAction (si);	

					if(reasoner.GetInternals(Agent.InternalWorldObjectContainers.WORKING_MEMORY).Count() > 0)
						state_counter = 3;
					else
						state_counter = 2;
				}
				Console.Write ("Is this pattern 2? Agent says: ");
				Console.WriteLine (chosen.LabelAsIComparable);
				reasoner.ResetWorkingMemory();
			}
		}
    }
}
