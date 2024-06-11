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
using Clarion.Framework.Templates;

namespace Clarion.Samples
{
    class SerializationDemo
    {
        static bool load = false;
        static int numTrials = 2;

        static string worldLoadFile = "SerializationDemo - World.xml";
        static string agentLoadFile = "SerializationDemo - Agent.xml";
        static string driveLoadFile = "SerializationDemo - Drive.xml";
        static string componentLoadFile = "SerializationDemo - DriveComponent.xml";

        public static void Main()
        {
            char repeat;
            do
            {
                Console.Write("Load Serialization Data (y/n)?");
                load = (Console.ReadKey().KeyChar != 'n');
                Console.WriteLine();

                Console.WriteLine("World Serialization/Pre-Training Demonstration");
                SerializeWorld();

                World.Destroy();
                World.Initialize();
                ImplicitComponentInitializer.ClearRanges();
                 

                Console.WriteLine("Agent Serialization/Pre-Training Demonstration");
                SerializeAgent();

                World.Destroy();
                World.Initialize();
                ImplicitComponentInitializer.ClearRanges();

                Console.WriteLine("Drive Serialization/Pre-Training Demonstration");
                SerializeDrive();

                World.Destroy();
                World.Initialize();
                ImplicitComponentInitializer.ClearRanges();

                Console.WriteLine("Drive Component Serialization/Pre-Training Demonstration");
                SerializeDriveComponent();

                World.Destroy();
                World.Initialize();
                ImplicitComponentInitializer.ClearRanges();

                Console.Write("Continue (y/n)?");
                repeat = Console.ReadKey().KeyChar;
                Console.WriteLine();
            } while (repeat != 'n');
        }

        static void SerializeWorld()
        {
            Agent John;
            BPNetwork net;
            FoodDrive foodDr;
            if (load && File.Exists(worldLoadFile))
            {
                Console.WriteLine("Deserializing the world");
                SerializationPlugin.DeserializeWorld(worldLoadFile);
                John = World.GetAgent("John");
                foodDr = (FoodDrive)John.GetInternals(Agent.InternalContainers.DRIVES).First();
                net = (BPNetwork)foodDr.DriveComponent;
            }
            else
            {
                Console.WriteLine("Initializing the world");
                World.LoggingLevel = TraceLevel.Warning;
                John = World.NewAgent("John");

                foodDr = AgentInitializer.InitializeDrive(John, FoodDrive.Factory, .5);

                net = AgentInitializer.InitializeDriveComponent(foodDr, BPNetwork.Factory);
                net.Input.AddRange(Drive.GenerateTypicalInputs(foodDr));

                net.Parameters.LEARNING_RATE = .2;
                net.Parameters.MOMENTUM = .05;
                foodDr.Commit(net);
                John.Commit(foodDr);
            }

            DoTraining(net, foodDr);

            Console.WriteLine("Serializing the world");
            SerializationPlugin.SerializeWorld(worldLoadFile);

            John.Die();
        }

        static void SerializeAgent()
        {
            World.LoggingLevel = TraceLevel.Warning;
            Agent John;
            BPNetwork net;
            FoodDrive foodDr;

            if (load && File.Exists(agentLoadFile))
            {
                Console.WriteLine("Deserializing John");
                SerializationPlugin.DeserializeWorldObject(agentLoadFile, out John);
                foodDr = (FoodDrive)John.GetInternals(Agent.InternalContainers.DRIVES).First();
                net = (BPNetwork)foodDr.DriveComponent;
            }
            else
            {
                Console.WriteLine("Initializing John");
                John = World.NewAgent("John");

                foodDr = AgentInitializer.InitializeDrive(John, FoodDrive.Factory, .5);

                net = AgentInitializer.InitializeDriveComponent(foodDr, BPNetwork.Factory);
                net.Input.AddRange(Drive.GenerateTypicalInputs(foodDr));

                net.Parameters.LEARNING_RATE = .2;
                net.Parameters.MOMENTUM = .05;
                foodDr.Commit(net);
                John.Commit(foodDr);
            }

            DoTraining(net, foodDr);

            Console.WriteLine("Serializing John");
            SerializationPlugin.Serialize(John, agentLoadFile);

            John.Die();
        }

        static void SerializeDrive()
        {
            World.LoggingLevel = TraceLevel.Warning;
            Agent John = World.NewAgent("John");
            BPNetwork net;
            FoodDrive foodDr;

            if (load && File.Exists(driveLoadFile))
            {
                Console.WriteLine("Deserializing the drive");
                SerializationPlugin.DeserializeDrive(John, driveLoadFile, out foodDr);
                net = (BPNetwork)foodDr.DriveComponent;
            }
            else
            {
                Console.WriteLine("Initializing the drive");
                foodDr = AgentInitializer.InitializeDrive(John, FoodDrive.Factory, .5);

                net = AgentInitializer.InitializeDriveComponent(foodDr, BPNetwork.Factory);
                net.Input.AddRange(Drive.GenerateTypicalInputs(foodDr));

                net.Parameters.LEARNING_RATE = .2;
                net.Parameters.MOMENTUM = .05;
                foodDr.Commit(net);
                John.Commit(foodDr);
            }

            DoTraining(net, foodDr);

            Console.WriteLine("Serializing the drive");
            SerializationPlugin.Serialize(foodDr, driveLoadFile);

            John.Die();
        }

        static void SerializeDriveComponent()
        {
            World.LoggingLevel = TraceLevel.Warning;
            Agent John = World.NewAgent("John");
            BPNetwork net;
            FoodDrive foodDr = AgentInitializer.InitializeDrive(John, FoodDrive.Factory, .5);;

            if (load && File.Exists(componentLoadFile))
            {
                Console.WriteLine("Deserializing the drive component");
                SerializationPlugin.DeserializeDriveComponent(foodDr, componentLoadFile, out net);
            }
            else
            {
                Console.WriteLine("Initializing the drive component");
                net = AgentInitializer.InitializeDriveComponent(foodDr, BPNetwork.Factory);
                net.Input.AddRange(Drive.GenerateTypicalInputs(foodDr));

                net.Parameters.LEARNING_RATE = .2;
                net.Parameters.MOMENTUM = .05;
                foodDr.Commit(net);
            }
            
            John.Commit(foodDr);

            DoTraining(net, foodDr);

            Console.WriteLine("Serializing the drive component");
            SerializationPlugin.Serialize(net, componentLoadFile);

            John.Die();
        }

        static void DoTraining(BPNetwork target, FoodDrive foodDr)
        {
            DriveEquation trainer = ImplicitComponentInitializer.InitializeTrainer(DriveEquation.Factory, foodDr);

            trainer.Commit();

            List<ActivationCollection> data = new List<ActivationCollection>();
            data.Add(ImplicitComponentInitializer.NewDataSet());

            foreach (var i in foodDr.Input)
            {
                ImplicitComponentInitializer.AddRange(i.WORLD_OBJECT, 0, 1, .25);
                data[0].Add(i);
            }

            Console.WriteLine("Performing Pre-Training (see the trace log for results)"); 

            ImplicitComponentInitializer.Train(target, trainer, data, ImplicitComponentInitializer.TrainingTerminationConditions.BOTH, numTrials);
        }
    }
}
