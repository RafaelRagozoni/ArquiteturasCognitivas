using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Linq;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Templates;
using Clarion.Framework.Extensions;
using Clarion.Framework.Extensions.Templates;

namespace Clarion.Samples
{
	public class Identification
	{
		public enum Groups { PRIVATE, PUBLIC }
		
		static Agent Participant;
		
		static double abeBlackGun = .56;
		static double abeWhiteGun = .5;
		static double abeMaxTemp = .03;
        static double pABEBlackGun;
        static double pABEWhiteGun;

        static GenericEquation trainer = ImplicitComponentInitializer.InitializeTrainer(GenericEquation.Factory, (Equation)PreTrainingEquation);
		
		static int numTestTrials = 128;
		static int numTrainingTrials = 100;
		static int numAgents = 20;
		static Dictionary<Groups, List<int>> btErr = new Dictionary<Groups, List<int>>();
		static Dictionary<Groups, List<int>> bgErr = new Dictionary<Groups, List<int>>();
		static Dictionary<Groups, List<int>> wtErr = new Dictionary<Groups, List<int>>();
		static Dictionary<Groups, List<int>> wgErr = new Dictionary<Groups, List<int>>();

        static List<DimensionValuePair> dvs = new List<DimensionValuePair>();
        static List<ExternalActionChunk> acts = new List<ExternalActionChunk>();
		
		static List<DeclarativeChunk> guns = new List<DeclarativeChunk>();
		static List<DeclarativeChunk> tools = new List<DeclarativeChunk>();
		static List<DeclarativeChunk> black_faces = new List<DeclarativeChunk>();
		static List<DeclarativeChunk> white_faces = new List<DeclarativeChunk>();

        static Random r = new Random();
		
		public static void Main ()
		{
            Console.WriteLine("Initializing the Identification Task");
			InitializeWorld();
			
			foreach (Groups g in Enum.GetValues(typeof(Groups)))
			{
                bgErr[g] = new List<int>(numAgents);
                btErr[g] = new List<int>(numAgents);
                wgErr[g] = new List<int>(numAgents);
                wtErr[g] = new List<int>(numAgents);

				for (int i = 0; i < numAgents; i++)
				{
                    Console.WriteLine("Initializing Agent " + (i+1) + ", Group " + g);
                    InitializeAgent(g);
                    Console.WriteLine("Testing Agent " + (i+1) + ", Group " + g);
					Test(g, i);

                    Participant.Die();
                    World.Remove(Participant);
				}
			}
			
			ReportResults();

            Console.WriteLine("The Stereotype Task has finished");
            Console.WriteLine("The results have been saved to \"Stereotype.txt\"");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
		}
		
		public static void InitializeWorld()
		{
            World.LoggingLevel = System.Diagnostics.TraceLevel.Off;
			//Prime DVs
			
			dvs.Add(World.NewDimensionValuePair("SkinColor", "Black"));
			dvs.Add(World.NewDimensionValuePair("SkinColor", "White"));
			
			dvs.Add(World.NewDimensionValuePair("NoseShape", "Thin"));
			dvs.Add(World.NewDimensionValuePair("NoseShape", "Wide"));
			
			dvs.Add(World.NewDimensionValuePair("NoseLength", "Short"));
			dvs.Add(World.NewDimensionValuePair("NoseLength", "Long"));
			
			dvs.Add(World.NewDimensionValuePair("EyebrowShape", "Thick"));
			dvs.Add(World.NewDimensionValuePair("EyebrowShape", "Thin"));
			
			dvs.Add(World.NewDimensionValuePair("EyeSize", "Big"));
			dvs.Add(World.NewDimensionValuePair("EyeSize", "Small"));
			
			dvs.Add(World.NewDimensionValuePair("Sex", "Male"));
			dvs.Add(World.NewDimensionValuePair("Sex", "Female"));

            //Target DVs

            dvs.Add(World.NewDimensionValuePair("HandleColor", "Black"));
            dvs.Add(World.NewDimensionValuePair("HandleColor", "White"));

            dvs.Add(World.NewDimensionValuePair("Shape", "Bent"));
            dvs.Add(World.NewDimensionValuePair("Shape", "Straight"));

            dvs.Add(World.NewDimensionValuePair("HandleLength", "Long"));
            dvs.Add(World.NewDimensionValuePair("HandleLength", "Short"));

            dvs.Add(World.NewDimensionValuePair("HeadLength", "Long"));
            dvs.Add(World.NewDimensionValuePair("HeadLength", "Short"));

            dvs.Add(World.NewDimensionValuePair("HeadColor", "Black"));
            dvs.Add(World.NewDimensionValuePair("HeadColor", "White"));

            //Actions

            acts.Add(World.NewExternalActionChunk("Tool"));
            acts.Add(World.NewExternalActionChunk("Gun"));
			
			//Generate Primes
			
			black_faces.Add(World.NewDeclarativeChunk("Black Face 1"));
            black_faces[0].Add(World.GetDimensionValuePair("SkinColor", "Black"));
            black_faces[0].Add(World.GetDimensionValuePair("NoseShape", "Thin"));
            black_faces[0].Add(World.GetDimensionValuePair("NoseLength", "Short"));
            black_faces[0].Add(World.GetDimensionValuePair("EyebrowShape", "Thick"));
            black_faces[0].Add(World.GetDimensionValuePair("EyeSize", "Big"));
            black_faces[0].Add(World.GetDimensionValuePair("Sex", "Male"));
			
			black_faces.Add(World.NewDeclarativeChunk("Black Face 2"));
            black_faces[1].Add(World.GetDimensionValuePair("SkinColor", "Black"));
            black_faces[1].Add(World.GetDimensionValuePair("NoseShape", "Wide"));
            black_faces[1].Add(World.GetDimensionValuePair("NoseLength", "Long"));
            black_faces[1].Add(World.GetDimensionValuePair("EyebrowShape", "Thin"));
            black_faces[1].Add(World.GetDimensionValuePair("EyeSize", "Small"));
            black_faces[1].Add(World.GetDimensionValuePair("Sex", "Male"));
			
			black_faces.Add(World.NewDeclarativeChunk("Black Face 3"));
            black_faces[2].Add(World.GetDimensionValuePair("SkinColor", "Black"));
            black_faces[2].Add(World.GetDimensionValuePair("NoseShape", "Thin"));
            black_faces[2].Add(World.GetDimensionValuePair("NoseLength", "Short"));
            black_faces[2].Add(World.GetDimensionValuePair("EyebrowShape", "Thick"));
            black_faces[2].Add(World.GetDimensionValuePair("EyeSize", "Big"));
            black_faces[2].Add(World.GetDimensionValuePair("Sex", "Female"));
			
			black_faces.Add(World.NewDeclarativeChunk("Black Face 4"));
			black_faces[3].Add(World.GetDimensionValuePair("SkinColor", "Black"));
            black_faces[3].Add(World.GetDimensionValuePair("NoseShape", "Wide"));
            black_faces[3].Add(World.GetDimensionValuePair("NoseLength", "Long"));
            black_faces[3].Add(World.GetDimensionValuePair("EyebrowShape", "Thin"));
            black_faces[3].Add(World.GetDimensionValuePair("EyeSize", "Small"));
            black_faces[3].Add(World.GetDimensionValuePair("Sex", "Female"));
			
			white_faces.Add(World.NewDeclarativeChunk("White Face 1"));
            white_faces[0].Add(World.GetDimensionValuePair("SkinColor", "White"));
            white_faces[0].Add(World.GetDimensionValuePair("NoseShape", "Wide"));
            white_faces[0].Add(World.GetDimensionValuePair("NoseLength", "Long"));
            white_faces[0].Add(World.GetDimensionValuePair("EyebrowShape", "Thin"));
            white_faces[0].Add(World.GetDimensionValuePair("EyeSize", "Small"));
            white_faces[0].Add(World.GetDimensionValuePair("Sex", "Male"));

			white_faces.Add(World.NewDeclarativeChunk("White Face 2"));
            white_faces[1].Add(World.GetDimensionValuePair("SkinColor", "White"));
            white_faces[1].Add(World.GetDimensionValuePair("NoseShape", "Thin"));
            white_faces[1].Add(World.GetDimensionValuePair("NoseLength", "Short"));
            white_faces[1].Add(World.GetDimensionValuePair("EyebrowShape", "Thick"));
            white_faces[1].Add(World.GetDimensionValuePair("EyeSize", "Big"));
            white_faces[1].Add(World.GetDimensionValuePair("Sex", "Female"));
			
			
			white_faces.Add(World.NewDeclarativeChunk("White Face 3"));
            white_faces[2].Add(World.GetDimensionValuePair("SkinColor", "White"));
            white_faces[2].Add(World.GetDimensionValuePair("NoseShape", "Thin"));
            white_faces[2].Add(World.GetDimensionValuePair("NoseLength", "Short"));
            white_faces[2].Add(World.GetDimensionValuePair("EyebrowShape", "Thick"));
            white_faces[2].Add(World.GetDimensionValuePair("EyeSize", "Big"));
            white_faces[2].Add(World.GetDimensionValuePair("Sex", "Male"));
			

			white_faces.Add(World.NewDeclarativeChunk("White Face 4"));
            white_faces[3].Add(World.GetDimensionValuePair("SkinColor", "White"));
            white_faces[3].Add(World.GetDimensionValuePair("NoseShape", "Wide"));
            white_faces[3].Add(World.GetDimensionValuePair("NoseLength", "Long"));
            white_faces[3].Add(World.GetDimensionValuePair("EyebrowShape", "Thin"));
            white_faces[3].Add(World.GetDimensionValuePair("EyeSize", "Small"));
            white_faces[3].Add(World.GetDimensionValuePair("Sex", "Female"));
			
			//Generate Targets
			
			guns.Add(World.NewDeclarativeChunk("Gun 1"));
            guns[0].Add(World.GetDimensionValuePair("HandleColor", "Black"));
            guns[0].Add(World.GetDimensionValuePair("Shape", "Bent"));
            guns[0].Add(World.GetDimensionValuePair("HandleLength", "Long"));
            guns[0].Add(World.GetDimensionValuePair("HeadLength", "Long"));
            guns[0].Add(World.GetDimensionValuePair("HeadColor", "Black"));
			
			guns.Add(World.NewDeclarativeChunk("Gun 2"));
            guns[1].Add(World.GetDimensionValuePair("HandleColor", "Black"));
            guns[1].Add(World.GetDimensionValuePair("Shape", "Straight"));
            guns[1].Add(World.GetDimensionValuePair("HandleLength", "Short"));
            guns[1].Add(World.GetDimensionValuePair("HeadLength", "Short"));
            guns[1].Add(World.GetDimensionValuePair("HeadColor", "White"));
			
			guns.Add(World.NewDeclarativeChunk("Gun 3"));
            guns[2].Add(World.GetDimensionValuePair("HandleColor", "Black"));
            guns[2].Add(World.GetDimensionValuePair("Shape", "Bent"));
            guns[2].Add(World.GetDimensionValuePair("HandleLength", "Long"));
            guns[2].Add(World.GetDimensionValuePair("HeadLength", "Long"));
            guns[2].Add(World.GetDimensionValuePair("HeadColor", "White"));
			
			guns.Add(World.NewDeclarativeChunk("Gun 4"));
            guns[3].Add(World.GetDimensionValuePair("HandleColor", "Black"));
            guns[3].Add(World.GetDimensionValuePair("Shape", "Straight"));
            guns[3].Add(World.GetDimensionValuePair("HandleLength", "Short"));
            guns[3].Add(World.GetDimensionValuePair("HeadLength", "Short"));
            guns[3].Add(World.GetDimensionValuePair("HeadColor", "Black"));
			
			tools.Add(World.NewDeclarativeChunk("Tool 1"));
            tools[0].Add(World.GetDimensionValuePair("HandleColor", "White"));
            tools[0].Add(World.GetDimensionValuePair("Shape", "Straight"));
            tools[0].Add(World.GetDimensionValuePair("HandleLength", "Short"));
            tools[0].Add(World.GetDimensionValuePair("HeadLength", "Short"));
            tools[0].Add(World.GetDimensionValuePair("HeadColor", "White"));

			tools.Add(World.NewDeclarativeChunk("Tool 2"));
            tools[1].Add(World.GetDimensionValuePair("HandleColor", "White"));
            tools[1].Add(World.GetDimensionValuePair("Shape", "Bent"));
            tools[1].Add(World.GetDimensionValuePair("HandleLength", "Long"));
            tools[1].Add(World.GetDimensionValuePair("HeadLength", "Long"));
            tools[1].Add(World.GetDimensionValuePair("HeadColor", "Black"));
			
			
			tools.Add(World.NewDeclarativeChunk("Tool 3"));
            tools[2].Add(World.GetDimensionValuePair("HandleColor", "White"));
            tools[2].Add(World.GetDimensionValuePair("Shape", "Straight"));
            tools[2].Add(World.GetDimensionValuePair("HandleLength", "Short"));
            tools[2].Add(World.GetDimensionValuePair("HeadLength", "Short"));
            tools[2].Add(World.GetDimensionValuePair("HeadColor", "Black"));
			

			tools.Add(World.NewDeclarativeChunk("Tool 4"));
			tools[3].Add(World.GetDimensionValuePair("HandleColor", "White"));
            tools[3].Add(World.GetDimensionValuePair("Shape", "Straight"));
            tools[3].Add(World.GetDimensionValuePair("HandleLength", "Short"));
            tools[3].Add(World.GetDimensionValuePair("HeadLength", "Short"));
            tools[3].Add(World.GetDimensionValuePair("HeadColor", "Black"));

            trainer.Input.AddRange(dvs);

            trainer.Output.AddRange(acts);

            trainer.Commit();
		}

        public static void InitializeAgent(Groups gr)
        {
            Participant = World.NewAgent();

            BPNetwork idn = AgentInitializer.InitializeImplicitDecisionNetwork(Participant, BPNetwork.Factory);

            idn.Input.AddRange(dvs);

            idn.Output.AddRange(acts);
			
			Participant.Commit(idn);

            foreach (DeclarativeChunk t in tools)
            {
                RefineableActionRule a = AgentInitializer.InitializeActionRule(Participant, RefineableActionRule.Factory, World.GetActionChunk("Tool"));
                foreach (DimensionValuePair dv in t)
                    a.GeneralizedCondition.Add(dv, true);
                Participant.Commit(a);
            }

            foreach (DeclarativeChunk g in guns)
            {
                RefineableActionRule a = AgentInitializer.InitializeActionRule(Participant, RefineableActionRule.Factory, World.GetActionChunk("Gun"));
                foreach (DimensionValuePair dv in g)
                    a.GeneralizedCondition.Add(dv, true);
                Participant.Commit(a);
            }

            Participant.ACS.Parameters.PERFORM_RER_REFINEMENT = false;
            Participant.ACS.Parameters.PERFORM_DELETION_BY_DENSITY = false;
            Participant.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 1;
            Participant.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 1;
            Participant.ACS.Parameters.FIXED_IRL_LEVEL_SELECTION_MEASURE = 0;
            Participant.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 0;
            Participant.ACS.Parameters.B = 1;

            HonorDrive honor = AgentInitializer.InitializeDrive(Participant, HonorDrive.Factory, r.NextDouble());

            GenericEquation hd = AgentInitializer.InitializeDriveComponent(honor, GenericEquation.Factory, (Equation)TangentEquation);

            var ins = Drive.GenerateTypicalInputs(honor);

            ParameterChangeActionChunk pac = World.NewParameterChangeActionChunk();
            pac.Add(Participant.ACS, "MCS_RER_SELECTION_MEASURE", .5);

            hd.Input.AddRange(ins);

            hd.Parameters.MAX_ACTIVATION = 5;

            honor.Commit(hd);

            honor.Parameters.DRIVE_GAIN = (gr == Groups.PRIVATE) ? .1 / 5 : .2 / 5;

            Participant.Commit(honor);

            ParameterSettingModule lpm = AgentInitializer.InitializeMetaCognitiveModule(Participant, ParameterSettingModule.Factory);

            ACSLevelProbabilitySettingEquation lpe = AgentInitializer.InitializeMetaCognitiveDecisionNetwork(lpm, ACSLevelProbabilitySettingEquation.Factory, Participant);

            lpe.Input.Add(honor.GetDriveStrength());

            lpm.Commit(lpe);

            Participant.Commit(lpm);

            lpm.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 1;
            lpm.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 0;

            //Pre-train the IDN in the ACS
            PreTrainACS(idn);
        }
		
		public static void PreTrainACS(BPNetwork idn)
		{
            Console.Write("Pre-training ACS...");

            pABEBlackGun = abeBlackGun + (((r.NextDouble() * 2) - 1) * abeMaxTemp);
            pABEWhiteGun = abeWhiteGun + (((r.NextDouble() * 2) - 1) * abeMaxTemp);

            List<ActivationCollection> dataSets = new List<ActivationCollection>();

            List<DeclarativeChunk> primes = new List<DeclarativeChunk>();
            primes.AddRange(white_faces);
            primes.AddRange(black_faces);

            List<DeclarativeChunk> targets = new List<DeclarativeChunk>();
            targets.AddRange(guns);
            targets.AddRange(tools);

            foreach (DeclarativeChunk p in primes)
            {
                foreach (DeclarativeChunk t in targets)
                {
                    ActivationCollection ds = ImplicitComponentInitializer.NewDataSet();
                    ds.AddRange(p, 1);
                    ds.AddRange(t, 1);

                    dataSets.Add(ds);
                }
            }

            ImplicitComponentInitializer.Train(idn, trainer, numIterations: numTrainingTrials, randomTraversal: true, dataSets: dataSets.ToArray());
			
            Console.WriteLine("Finished");
		}
		
		public static void Test(Groups g, int a)
		{
            Console.Write("Performing Task...");
			int [,] shuffler = new int[8,8];
            bgErr[g].Add(0);
            btErr[g].Add(0);
            wgErr[g].Add(0);
            wtErr[g].Add(0);
			List<DeclarativeChunk> primes = new List<DeclarativeChunk>();
			primes.AddRange(white_faces);
			primes.AddRange(black_faces);
			
			List<DeclarativeChunk> targets = new List<DeclarativeChunk>();
			targets.AddRange(guns);
			targets.AddRange(tools);
			
			for (int i = 0; i < numTestTrials; i++)
			{
				int p = r.Next(8);
				int t = r.Next(8);
				
				while (shuffler[p,t] == 2)
				{
					p = r.Next(8);
					t = r.Next(8);
				}

                shuffler[p,t]++;
				
				SensoryInformation si = World.NewSensoryInformation(Participant);
                si.AddRange(primes[p], 1);
                si.Add(targets[t], 1);

                si[Drive.MetaInfoReservations.STIMULUS, typeof(HonorDrive).Name] = (double)1/(double)5;
				
				Participant.Perceive(si);
				ExternalActionChunk chosen = Participant.GetChosenExternalAction(si);
				
				if((chosen.LabelAsIComparable.Equals("Tool") && !tools.Contains(targets[t])))
				{
					//The participant made an inaccurate judgment on a gun trial
					
					if(si.Contains(World.GetDimensionValuePair("SkinColor","Black")))		//The error was on a black trial
						bgErr[g][a]++;
					else
						wgErr[g][a]++;
				}
				else if ((chosen.LabelAsIComparable.Equals("Gun") && !guns.Contains(targets[t])))
				{
					//The participant made an inaccurate judgment on a tool trial
					
					if(si.Contains(World.GetDimensionValuePair("SkinColor","Black")))		//The error was on a black trial
						btErr[g][a]++;
					else
						wtErr[g][a]++;
				}
			}
            Console.WriteLine("Finished");
		}
		
		public static void ReportResults()
		{
			Console.WriteLine("Outputting Results to Stereotype.txt");
			TextWriter orig = Console.Out;
            StreamWriter sw = File.CreateText("Stereotype.txt");
			Console.SetOut(sw);
			foreach (Groups g in Enum.GetValues(typeof(Groups)))
			{
                int bgErrCount = 0;
                int btErrCount = 0;
                int wgErrCount = 0;
                int wtErrCount = 0;

				Console.WriteLine("Results for group: " + g);
				Console.WriteLine("Participant\tWhiteGunErrors\tBlackToolErrors\tWhiteToolErrors\tWhiteToolErrors");
				for (int i = 0; i < numAgents; i++)
				{
					Console.WriteLine(i+"\t"+ ((double)bgErr[g][i]/(double)numTestTrials) + "\t" +
					                  ((double)wgErr[g][i]/(double)numTestTrials) + "\t" +
					                  ((double)btErr[g][i]/(double)numTestTrials) + "\t" +
					                  ((double)wtErr[g][i]/(double)numTestTrials) + "\t");
					bgErrCount += bgErr[g][i];
					btErrCount += btErr[g][i];
					wgErrCount += wgErr[g][i];
					wtErrCount += wtErr[g][i];
				}
				Console.SetOut(orig);
				Console.WriteLine("Average error rates for group: " + g);
				Console.WriteLine("Black:Gun trials = " + ((double)bgErrCount/((double)numTestTrials*numAgents)));
				Console.WriteLine("White:Gun trials = " + ((double)wgErrCount/((double)numTestTrials*numAgents)));
				Console.WriteLine("Black:Tool trials = " + ((double)btErrCount/((double)numTestTrials*numAgents)));
				Console.WriteLine("White:Tool trials = " + ((double)wtErrCount/((double)numTestTrials*numAgents)));
				Console.SetOut(sw);
			}
			sw.Close();
            Console.SetOut(orig);
		}

        public static void TangentEquation(ActivationCollection input, ActivationCollection output)
        {
            output[Drive.MetaInfoReservations.DRIVE_STRENGTH, typeof(HonorDrive).Name] =
                5 * Math.Tanh(input[Drive.MetaInfoReservations.DRIVE_GAIN, typeof(HonorDrive).Name] * input[Drive.MetaInfoReservations.DEFICIT, typeof(HonorDrive).Name]);
        }

        public static void PreTrainingEquation(ActivationCollection input, ActivationCollection output)
        {
            if (input["SkinColor", "Black"] == 1)
            {
                if (r.NextDouble() <= pABEBlackGun)
                    output[World.GetActionChunk("Gun")] = 1;
                else
                    output[World.GetActionChunk("Tool")] = 1;
            }
            else
            {
                if (r.NextDouble() <= pABEWhiteGun)
                    output[World.GetActionChunk("Gun")] = 1;
                else
                    output[World.GetActionChunk("Tool")] = 1;
            }
        }
	}
}

