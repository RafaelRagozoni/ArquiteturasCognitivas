using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Templates;
using Clarion.Framework.Extensions.Templates;
using Clarion.Framework.Core;

namespace Clarion.Samples
{
    public class ProcessControl
    {
        public enum Tasks { SUGAR, PERSON }
        public enum Groups { CONTROL, VERBALIZATION, MEMORY, SIMPLE_RULE }
        public enum IRL_Rule_Sets { ONE, TWO }

        #region IRL Fields

        public readonly double[] As = { 1, 2 };

        public readonly double[] Bs = { -1, -2, 0, 1, 2 };

        public readonly double[] Cs = { -1, -2, 1, 2 };

        public double threshold_4 = .2;

        #endregion

        public double[] NoiseOptions = { -1, 0, 1 };

        public double target = 6;

        public Agent John;

        public Random rand = new Random();

        public static int numTestTrials = 20000;

        public static int numRepeats = 1;

        public int[, ,] results = new int[Enum.GetValues(typeof(Tasks)).Length, Enum.GetValues(typeof(Groups)).Length, numRepeats];

        static void Main(string[] args)
        {
            ProcessControl pc = new ProcessControl();
            pc.Run();

            Console.WriteLine("Press Any Key to Exit");
            Console.ReadKey();
        }

        public void Initialize(Groups group)
        {
            World.Initialize();
            John = World.NewAgent();

            QBPNetwork idn = AgentInitializer.InitializeImplicitDecisionNetwork(John, QBPNetwork.Factory);

            World.NewDimensionValuePair("Target P", target);
            World.NewDimensionValuePair("Current P", target);
            World.NewExternalActionChunk(target);

            for (double i = 0; i < 12; i++)
            {
                if (World.GetDimensionValuePair("Target P", i) == null)
                {
                    idn.Input.Add(World.NewDimensionValuePair("Target P", i));
                    idn.Input.Add(World.NewDimensionValuePair("Current P", i));
                    idn.Input.Add(World.NewExternalActionChunk(i));
                    idn.Output.Add(World.GetActionChunk(i));
                }
                else
                {
                    idn.Input.Add(World.GetDimensionValuePair("Target P", i));
                    idn.Input.Add(World.GetDimensionValuePair("Current P", i));
                    idn.Input.Add(World.GetActionChunk(i));
                    idn.Output.Add(World.GetActionChunk(i));
                }
            }

            foreach (double i in As)
                World.NewDimensionValuePair("A", i);
            foreach (double i in Bs)
                World.NewDimensionValuePair("B", i);
            foreach (double i in Cs)
                World.NewDimensionValuePair("C", i);

            switch (group)
            {
                case Groups.VERBALIZATION:
                    idn.Parameters.POSITIVE_MATCH_THRESHOLD = 1;
                    RefineableActionRule.GlobalParameters.POSITIVE_MATCH_THRESHOLD = 1;
                    RefineableActionRule.GlobalParameters.GENERALIZATION_THRESHOLD_1 = 1;
                    RefineableActionRule.GlobalParameters.SPECIALIZATION_THRESHOLD_1 = .5;
                    threshold_4 = .5;
                    break;
                case Groups.MEMORY:
                    for (double i = 0; i < 12; i++)
                    {
                        ExternalActionChunk w = (ExternalActionChunk)World.GetActionChunk((double)rand.Next(12));
                        var p = World.GetDimensionValuePair("Current P", FactoryOutput(i, (double)w.LabelAsIComparable));
                        ExternalActionChunk w1 = (ExternalActionChunk)World.GetActionChunk(Math.Round((target + p.Value + NoiseOptions[rand.Next(3)]) / 2));
                        FixedRule mfr = AgentInitializer.InitializeActionRule(John, FixedRule.Factory, w1, MemoryGroup_SupportCalculator);

                        mfr.GeneralizedCondition.Add(p, true);
                        mfr.GeneralizedCondition.Add(w, true);
                        John.Commit(mfr);
                    }
                    goto default;
                case Groups.SIMPLE_RULE:
                    for (double i = 0; i < 12; i++)
                    {
                        FixedRule sfr = AgentInitializer.InitializeActionRule(John, FixedRule.Factory, World.GetActionChunk(i), SimpleRule_SupportCalculator);
                        John.Commit(sfr);
                    }
                    goto default;
                default:
                    idn.Parameters.LEARNING_RATE = .05;
                    idn.Parameters.DISCOUNT = .95;
                    John.ACS.Parameters.SELECTION_TEMPERATURE = .09;
                    idn.Parameters.POSITIVE_MATCH_THRESHOLD = 1;
                    RefineableActionRule.GlobalParameters.GENERALIZATION_THRESHOLD_1 = 2;
                    RefineableActionRule.GlobalParameters.SPECIALIZATION_THRESHOLD_1 = 1.2;
                    RefineableActionRule.GlobalParameters.POSITIVE_MATCH_THRESHOLD = 1;
                    threshold_4 = .2;
                    break;
            }

            RefineableActionRule.GlobalParameters.INFORMATION_GAIN_OPTION = RefineableActionRule.IGOptions.PERFECT;

            John.Commit(idn);
        }

        public void GenerateIRLRuleSet(IRL_Rule_Sets ruleSet)
        {
            switch (ruleSet)
            {
                case IRL_Rule_Sets.ONE:
                    var i = (from a in As
                             select from b in Bs
                                    select
                                        new
                                        {
                                            A = World.GetDimensionValuePair("A", a),
                                            B = World.GetDimensionValuePair("B", b)
                                        }).SelectMany(t => t);
                    for (double w = 0; w < 12; w++)
                    {
                        foreach (var cond in i)
                        {
                            IRLRule ir = AgentInitializer.InitializeActionRule(John, IRLRule.Factory, World.GetActionChunk(w),
                                    IRLSet1_SupportCalculator, IRL_DeletionChecker);

                            ir.GeneralizedCondition.Add(cond.A, true);
                            ir.GeneralizedCondition.Add(cond.B, true);
                            John.Commit(ir);
                        }
                    }
                    break;
                case IRL_Rule_Sets.TWO:
                    var i2 = (from a in As
                              select from b in Bs
                                     select from c in Cs
                                            select
                                                new
                                                {
                                                    A = World.GetDimensionValuePair("A", a),
                                                    B = World.GetDimensionValuePair("B", b),
                                                    C = World.GetDimensionValuePair("C", c)
                                                }).SelectMany(t => t).SelectMany(t => t);
                    for (double w = 0; w < 12; w++)
                    {
                        foreach (var cond in i2)
                        {
                            IRLRule ir = AgentInitializer.InitializeActionRule(John, IRLRule.Factory, World.GetActionChunk(w),
                                IRLSet2_SupportCalculator, IRL_DeletionChecker);

                            ir.GeneralizedCondition.Add(cond.A, true);
                            ir.GeneralizedCondition.Add(cond.B, true);
                            ir.GeneralizedCondition.Add(cond.C, true);
                            John.Commit(ir);
                        }
                    }
                    break;
            }
        }

        public void Run()
        {
            foreach (Tasks t in Enum.GetValues(typeof(Tasks)))
            {
                int max_i = ((t == Tasks.PERSON) ? 2 * numTestTrials : numTestTrials);
                foreach (Groups g in Enum.GetValues(typeof(Groups)))
                {
                    Console.WriteLine("Running Group " + g + " through task " + t);
                    for (int r = 0; r < numRepeats; r++)
                    {
                        Console.Write("Participant #" + r + " is performing the task          ");
                        double currentW = rand.Next(12);
                        double lastP = rand.Next(12);
                        Initialize(g);

                        ActivationCollection irlSI = ImplicitComponentInitializer.NewDataSet();
                        var irlVars = (from a in As
                                       select from b in Bs
                                              select from c in Cs
                                                     select
                                                         new
                                                         {
                                                             A = World.GetDimensionValuePair("A", a),
                                                             B = World.GetDimensionValuePair("B", b),
                                                             C = World.GetDimensionValuePair("C", c)
                                                         }).SelectMany(k => k).SelectMany(k => k);
                        foreach (var k in irlVars)
                        {
                            irlSI.Add(k.A, 1);
                            irlSI.Add(k.B, 1);
                            irlSI.Add(k.C, 1);
                        }

                        DimensionValuePair targetDV = World.GetDimensionValuePair("Target P", target);

                        GenerateIRLRuleSet(IRL_Rule_Sets.ONE);
                        SensoryInformation si = null;
                        SensoryInformation prevSI;

                        for (int i = 0; i < max_i; i++)
                        {
                            int shift = 10 - (int)Math.Round(10 * ((double)i / (double)max_i));
                            Console.CursorLeft -= shift;
                            Console.Write(".");
                            for (int s = 0; s < shift - 1; s++)
                                Console.Write(" ");
                            if ((from a in John.GetInternals(Agent.InternalContainers.ACTION_RULES) where a is IRLRule select a).Count() == 0)
                                GenerateIRLRuleSet(IRL_Rule_Sets.TWO);

                            prevSI = si;

                            si = World.NewSensoryInformation(John);

                            foreach (var s in irlSI)
                                si.Add(s);

                            si.Add(targetDV, 1);
                            si.Add(World.GetActionChunk(currentW), 1);
                            lastP = FactoryOutput(lastP, currentW);
                            si.Add(World.GetDimensionValuePair("Current P", lastP), 1);

                            if (Math.Abs(lastP - target) < double.Epsilon)
                            {
                                if ((t != Tasks.PERSON || (t == Tasks.PERSON && i >= numTestTrials)))
                                    results[(int)t, (int)g, r]++;

                                if (prevSI != null)
                                    John.ReceiveFeedback(prevSI, 1);
                            }
                            else
                            {
                                if (prevSI != null)
                                    John.ReceiveFeedback(prevSI, 0);
                            }

                            John.Perceive(si);

                            currentW = (double)John.GetChosenExternalAction(si).LabelAsIComparable;
                        }
                        Console.WriteLine();
                        Console.WriteLine("Participant #" + r + " is finished performing the task and hit the target " +
                            results[(int)t, (int)g, r] + " times out of " + max_i);

                        Console.WriteLine("At the end of the task, the participant had the following rules: ");
                        foreach (var ar in John.GetInternals(Agent.InternalContainers.ACTION_RULES))
                        {
                            Console.WriteLine(ar);
                        }
                        John.Die();
                        World.Remove(John);
                    }
                }
                Console.WriteLine("Tabular results for the " + t + " task:");
                Console.WriteLine("Group\tParticipant\tHits");
                foreach (Groups g in Enum.GetValues(typeof(Groups)))
                {
                    for (int i = 0; i < numRepeats; i++)
                    {
                        Console.WriteLine(g + "\t" + i + "\t" + results[(int)t, (int)g, i]);
                    }
                }
            }
        }

        public double FactoryOutput(double lastP, double currentW)
        {
            double result = (2 * currentW) - lastP + NoiseOptions[rand.Next(3)];
            if (result > 11)
                return 11;
            else if (result < 0)
                return 0;
            else
                return result;
        }

        #region IRL Delegates

        public double CalculateSupport_IRLSet1(ActivationCollection si, Rule r = null)
        {
            double t = (from i in si
                        where i.WORLD_OBJECT.AsDimensionValuePair.Dimension.ToString() == "Target P" && 
                        Math.Abs(i.ACTIVATION - John.Parameters.MAX_ACTIVATION) < double.Epsilon
                        select (double)i.WORLD_OBJECT.AsDimensionValuePair.Value.AsIComparable).First();

            double A = (double)(from a in r.GeneralizedCondition
                                where a.Dimension.ToString() == "A" && r.GeneralizedCondition[a]
                                select a.AsDimensionValuePair.Value.AsIComparable).First();
            double B = (double)(from b in r.GeneralizedCondition
                                where b.AsDimensionValuePair.Dimension.ToString() == "B" && r.GeneralizedCondition[b]
                                select b.AsDimensionValuePair.Value.AsIComparable).First();

            return (Math.Abs((Math.Round((t - B) / A) - (double)((ActionRule)r).Action.LabelAsIComparable)) < double.Epsilon) ? 1 : 0;
        }

        public double CalculateSupport_IRLSet2(ActivationCollection si, Rule r = null)
        {
            double t = (from i in si
                        where i.WORLD_OBJECT.AsDimensionValuePair.Dimension.ToString() == "Target P" &&
                        Math.Abs(i.ACTIVATION - John.Parameters.MAX_ACTIVATION) < double.Epsilon
                        select (double)i.WORLD_OBJECT.AsDimensionValuePair.Value.AsIComparable).First();

            double p = (from i in si
                        where i.WORLD_OBJECT.AsDimensionValuePair.Dimension.ToString() == "Current P" &&
                        Math.Abs(i.ACTIVATION - John.Parameters.MAX_ACTIVATION) < double.Epsilon
                        select (double)i.WORLD_OBJECT.AsDimensionValuePair.Value.AsIComparable).First();

            double A = (double)(from a in r.GeneralizedCondition
                                where a.AsDimensionValuePair.Dimension.ToString() == "A" && r.GeneralizedCondition[a]
                                select a.AsDimensionValuePair.Value.AsIComparable).First();
            double B = (double)(from b in r.GeneralizedCondition
                                where b.AsDimensionValuePair.Dimension.ToString() == "B" && r.GeneralizedCondition[b] 
                                select b.AsDimensionValuePair.Value.AsIComparable).First();
            double C = (double)(from c in r.GeneralizedCondition
                                where c.AsDimensionValuePair.Dimension.ToString() == "C" && r.GeneralizedCondition[c] 
                                select c.AsDimensionValuePair.Value.AsIComparable).First();

            return (Math.Abs((Math.Round((t - B - (C * p)) / A) - (double)((ActionRule)r).Action.LabelAsIComparable)) < double.Epsilon) ? 1 : 0;
        }

        public bool CheckDeletion_IRL(long timeStamp, IDeletable r)
        {
            return ((IRLRule)r).CalculateInformationGain((IRLRule)r) < threshold_4;
        }

        public SupportCalculator IRLSet1_SupportCalculator { get { return CalculateSupport_IRLSet1; } }

        public SupportCalculator IRLSet2_SupportCalculator { get { return CalculateSupport_IRLSet2; } }

        public DeletionChecker IRL_DeletionChecker { get { return CheckDeletion_IRL; } }

        #endregion

        #region Memory Group FR Delegates

        public double CalculateSupport_MemoryGroup(ActivationCollection si, Rule r = null)
        {
            DimensionValuePair currentP = (from t in si
                                           where t.WORLD_OBJECT.AsDimensionValuePair.Dimension.ToString() == "Current P" && 
                                           r.GeneralizedCondition.Contains(t.WORLD_OBJECT, true)
                                           select t.WORLD_OBJECT.AsDimensionValuePair).FirstOrDefault();

            DimensionValuePair previousW = (from t in si
                                            where t.WORLD_OBJECT is ExternalActionChunk && 
                                            r.GeneralizedCondition.Contains(t.WORLD_OBJECT, true)
                                            select t.WORLD_OBJECT.AsDimensionValuePair).FirstOrDefault();

            if (currentP == null || previousW == null)
                return 0;
            else
                return (si[currentP] == John.Parameters.MAX_ACTIVATION && si[previousW] == John.Parameters.MAX_ACTIVATION) ? 1 : 0;
        }

        public SupportCalculator MemoryGroup_SupportCalculator { get { return CalculateSupport_MemoryGroup; } }

        #endregion

        #region Simple Rule FR Delegates

        public double CalculateSupport_SimpleRule(ActivationCollection si, Rule r = null)
        {
            double t = (from i in si
                        where i.WORLD_OBJECT.AsDimensionValuePair.Dimension.ToString() == "Target P" &&
                        Math.Abs(i.ACTIVATION - John.Parameters.MAX_ACTIVATION) < double.Epsilon
                        select (double)i.WORLD_OBJECT.AsDimensionValuePair.Value.AsIComparable).First();

            double c = (from i in si
                        where i.WORLD_OBJECT.AsDimensionValuePair.Dimension.ToString() == "Current P" &&
                        Math.Abs(i.ACTIVATION - John.Parameters.MAX_ACTIVATION) < double.Epsilon
                        select (double)i.WORLD_OBJECT.AsDimensionValuePair.Value.AsIComparable).First();

            double result = Math.Round((Math.Abs(t - c) / 2));
            if (c < t)
                result += c;
            else if (c > t)
                result -= c;

            return (Math.Abs(result - (double)((ActionRule)r).Action.LabelAsIComparable) < double.Epsilon) ? 1 : 0;
        }

        public SupportCalculator SimpleRule_SupportCalculator { get { return CalculateSupport_SimpleRule; } }

        #endregion
    }
}