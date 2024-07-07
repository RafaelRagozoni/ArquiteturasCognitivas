using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Core;
using Clarion.Framework.Templates;
using ClarionApp.Model;
using ClarionApp;
using System.Threading;
using Gtk;

namespace ClarionApp
{
    /// <summary>
    /// Public enum that represents all possibilities of agent actions
    /// </summary>
    public enum CreatureActions
    {
        DO_NOTHING,
        ROTATE_CLOCKWISE,
        GO_AHEAD,
        GO_JEWEL,
        GO_FOOD,
        GO_DELIVER,
        GET_JEWEL,
        EAT_FOOD,
        DELIVER_LEAFLET,
        STOP
    }

    public class ClarionAgent
    {
        #region Constants
        /// <summary>
        /// Constant that represents the Visual Sensor
        /// </summary>
        private String SENSOR_VISUAL_DIMENSION = "VisualSensor";
        /// <summary>
        /// Constant that represents that there is at least one wall ahead
        /// </summary>
        private String DIMENSION_WALL_AHEAD = "WallAhead";
        
        private String DIMENSION_JEWEL_AHEAD = "JewelAhead";
        private String DIMENSION_FOOD_AHEAD = "FoodAhead";
        private String DIMENSION_JEWEL = "Jewel";
        private String DIMENSION_FOOD = "Food";
        private String DIMENSION_LEAFLET = "Leaflet";
        private String DIMENSION_DELIVER_SPOT_AHEAD = "DeliverSpotAhead";
        private String DIMENSION_DELIVER_SPOT = "DeliverSpot";
        private String DIMENSION_STOP = "STOP";



		double prad = 0;
        #endregion

        #region Properties
		public MindViewer mind;
		String creatureId = String.Empty;
		String creatureName = String.Empty;
        String foodName = String.Empty;
		String jewelName = String.Empty;
		String deliverName = String.Empty;
		String leafletId = String.Empty;
		int leafletNumber = -1;
		Thing closestFood = null;
		Thing closestJewel = null;
		Thing deliverSpot = null;
        Dictionary<string, bool> deliverableLeaflets = new Dictionary<string, bool>();
        bool stopped = false;
        #region Simulation
        /// <summary>
        /// If this value is greater than zero, the agent will have a finite number of cognitive cycle. Otherwise, it will have infinite cycles.
        /// </summary>
        public double MaxNumberOfCognitiveCycles = -1;
        /// <summary>
        /// Current cognitive cycle number
        /// </summary>
        private double CurrentCognitiveCycle = 0;
        /// <summary>
        /// Time between cognitive cycle in miliseconds
        /// </summary>
        public Int32 TimeBetweenCognitiveCycles = 0;
        /// <summary>
        /// A thread Class that will handle the simulation process
        /// </summary>
        private Thread runThread;
        #endregion

        #region Agent
		private WSProxy worldServer;
        /// <summary>
        /// The agent 
        /// </summary>
        private Clarion.Framework.Agent CurrentAgent;
        #endregion

        #region Perception Input
        /// <summary>
        /// Perception input to indicates a wall ahead
        /// </summary>
		private DimensionValuePair inputWallAhead;
        private DimensionValuePair inputJewelAhead;
        private DimensionValuePair inputFoodAhead;
		private DimensionValuePair inputDeliverSpotAhead;
        private DimensionValuePair inputJewel;
		private DimensionValuePair inputFood;
		private DimensionValuePair inputDeliverSpot;
		private DimensionValuePair inputStop;

        #endregion

        #region Action Output
        /// <summary>
        /// Output action that makes the agent to rotate clockwise
        /// </summary>
		private ExternalActionChunk outputRotateClockwise;
        /// <summary>
        /// Output action that makes the agent go ahead
        /// </summary>
		private ExternalActionChunk outputGoAhead;

		private ExternalActionChunk outputGoJewel;
		private ExternalActionChunk outputGoFood;
		private ExternalActionChunk outputGoDeliver;
		private ExternalActionChunk outputGetJewel;
		private ExternalActionChunk outputEatFood;
		private ExternalActionChunk outputDeliverLeaflet;
		private ExternalActionChunk outputStop;

        #endregion

        #endregion

        #region Constructor
		public ClarionAgent(WSProxy nws, String creature_ID, String creature_Name)
        {
			worldServer = nws;
			// Initialize the agent
            CurrentAgent = World.NewAgent("Current Agent");
			mind = new MindViewer();
			mind.Show ();
			creatureId = creature_ID;
			creatureName = creature_Name;

            // Initialize Input Information
            inputWallAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_WALL_AHEAD);
            inputJewelAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_JEWEL_AHEAD);
			inputFoodAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_FOOD_AHEAD);
			inputJewel = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_JEWEL);
			inputFood = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_FOOD);
			inputDeliverSpotAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_DELIVER_SPOT_AHEAD);
			inputDeliverSpot = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_DELIVER_SPOT);
			inputStop = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_STOP);

            // Initialize Output actions
            outputRotateClockwise = World.NewExternalActionChunk(CreatureActions.ROTATE_CLOCKWISE.ToString());
            outputGoAhead = World.NewExternalActionChunk(CreatureActions.GO_AHEAD.ToString());
            outputGoJewel = World.NewExternalActionChunk(CreatureActions.GO_JEWEL.ToString());
            outputGoFood = World.NewExternalActionChunk(CreatureActions.GO_FOOD.ToString());
            outputGoDeliver = World.NewExternalActionChunk(CreatureActions.GO_DELIVER.ToString());
            outputGetJewel = World.NewExternalActionChunk(CreatureActions.GET_JEWEL.ToString());
            outputEatFood = World.NewExternalActionChunk(CreatureActions.EAT_FOOD.ToString());
            outputDeliverLeaflet =  World.NewExternalActionChunk(CreatureActions.DELIVER_LEAFLET.ToString());
            outputStop = World.NewExternalActionChunk(CreatureActions.STOP.ToString());
            //Create thread to simulation
            runThread = new Thread(CognitiveCycle);
			Console.WriteLine("Agent started");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Run the Simulation in World Server 3d Environment
        /// </summary>
        public void Run()
        {                
			Console.WriteLine ("Running ...");
            // Setup Agent to run
            if (runThread != null && !runThread.IsAlive)
            {
                SetupAgentInfraStructure();
				// Start Simulation Thread                
                runThread.Start(null);
            }
        }

        /// <summary>
        /// Abort the current Simulation
        /// </summary>
        /// <param name="deleteAgent">If true beyond abort the current simulation it will die the agent.</param>
        public void Abort(Boolean deleteAgent)
        {   Console.WriteLine ("Aborting ...");
            if (runThread != null && runThread.IsAlive)
            {
                runThread.Abort();
            }

            if (CurrentAgent != null && deleteAgent)
            {
                CurrentAgent.Die();
            }
        }

		IList<Thing> processSensoryInformation()
		{
			IList<Thing> response = null;

			if (worldServer != null && worldServer.IsConnected)
			{
				response = worldServer.SendGetCreatureState(creatureName);
				prad = (Math.PI / 180) * response.First().Pitch;
				while (prad > Math.PI) prad -= 2 * Math.PI;
				while (prad < - Math.PI) prad += 2 * Math.PI;
				Sack s = worldServer.SendGetSack("0");
				mind.setBag(s);
			}
            Console.WriteLine(response);
            Console.WriteLine("response");
			return response;
		}

		void processSelectedAction(CreatureActions externalAction)
		{   Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			if (worldServer != null && worldServer.IsConnected)
			{
				switch (externalAction)
				{
				case CreatureActions.DO_NOTHING:
					// Do nothing as the own value says
					break;
				case CreatureActions.ROTATE_CLOCKWISE:
					worldServer.SendSetAngle(creatureId, 2, -2, 2);
					break;
				case CreatureActions.GO_AHEAD:
					worldServer.SendSetAngle(creatureId, 1, 1, prad);
					break;
                case CreatureActions.GO_JEWEL:
                    worldServer.SendSetGoTo(creatureId,1, 1, closestJewel.comX, closestJewel.comY);
					break;
                case CreatureActions.GO_FOOD:
                    worldServer.SendSetGoTo(creatureId,1, 1, closestFood.comX, closestFood.comY);
					break;
                case CreatureActions.GO_DELIVER:
                    worldServer.SendSetGoTo(creatureId,1, 1, deliverSpot.comX, deliverSpot.comY);
					break;
                case CreatureActions.GET_JEWEL:
					worldServer.SendSackIt(creatureId, jewelName);
					break;
                case CreatureActions.EAT_FOOD:
					worldServer.SendEatIt(creatureId, foodName);
					break;
                case CreatureActions.DELIVER_LEAFLET:
                    worldServer.SendSetDeliverLeaflet(creatureId, leafletId);
                    mind.leafletsDelivered[leafletNumber] = true;
                    mind.leafletsCompletion[leafletNumber] = false;
                    if(mind.leafletsDelivered[0] && mind.leafletsDelivered[1] && mind.leafletsDelivered[2])
                        Abort(true);
					break;
                case CreatureActions.STOP:
					worldServer.SendStopCreature(creatureId);
					break;
				default:
					break;
				}
			}
		}

        #endregion

        #region Setup Agent Methods
        /// <summary>
        /// Setup agent infra structure (ACS, NACS, MS and MCS)
        /// </summary>
        private void SetupAgentInfraStructure()
        {
            // Setup the ACS Subsystem
            SetupACS();                    
        }

        private void SetupMS()
        {            
            //RichDrive
        }

        /// <summary>
        /// Setup the ACS subsystem
        /// </summary>
        private void SetupACS()
        {
            // Create Rule to avoid collision with wall
            SupportCalculator avoidCollisionWallSupportCalculator = FixedRuleToAvoidCollisionWall;
            FixedRule ruleAvoidCollisionWall = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputRotateClockwise, avoidCollisionWallSupportCalculator);

            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleAvoidCollisionWall);

            // Create Colission To Go Ahead
            SupportCalculator goAheadSupportCalculator = FixedRuleToGoAhead;
            FixedRule ruleGoAhead = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGoAhead, goAheadSupportCalculator);
            
            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleGoAhead);

            // Create Rule to Go Jewel
			SupportCalculator goJewelSupportCalculator = FixedRuleToGoJewel;
			FixedRule ruleGoJewel = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGoJewel, goJewelSupportCalculator);
			ruleGoJewel.Parameters.WEIGHT = 0.7;
			ruleGoJewel.Parameters.PARTIAL_MATCH_ON = true;
            CurrentAgent.Commit(ruleGoJewel);

            // Create Rule to Go Food
			SupportCalculator goFoodSupportCalculator = FixedRuleToGoFood;
			FixedRule ruleGoFood = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGoFood, goFoodSupportCalculator);
			ruleGoFood.Parameters.WEIGHT = 0.5;
			ruleGoFood.Parameters.PARTIAL_MATCH_ON = true;
            CurrentAgent.Commit(ruleGoFood);

            // Create Rule to Go Deliver
			SupportCalculator goDeliverSupportCalculator = FixedRuleToGoDeliver;
			FixedRule ruleGoDeliver = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGoDeliver, goDeliverSupportCalculator);
			ruleGoDeliver.Parameters.WEIGHT = 0.9;
			ruleGoDeliver.Parameters.PARTIAL_MATCH_ON = true;
            CurrentAgent.Commit(ruleGoDeliver);

            // Create Rule to Get Jewel
			SupportCalculator getJewelSupportCalculator = FixedRuleToGetJewel;
			FixedRule ruleGetJewel = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputGetJewel, getJewelSupportCalculator);
			ruleGetJewel.Parameters.WEIGHT = 0.8;
			ruleGetJewel.Parameters.PARTIAL_MATCH_ON = true;
            CurrentAgent.Commit(ruleGetJewel);

            // Create Rule to Eat Food
			SupportCalculator eatFoodSupportCalculator = FixedRuleToEatFood;
			FixedRule ruleEatFood = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputEatFood, eatFoodSupportCalculator);
			ruleEatFood.Parameters.WEIGHT = 0.6;
			ruleEatFood.Parameters.PARTIAL_MATCH_ON = true;
            CurrentAgent.Commit(ruleEatFood);

            // Create Rule to Deliver Leaflet
			SupportCalculator deliverLeafletSupportCalculator = FixedRuleToDeliverLeaflet;
			FixedRule ruleDeliverLeaflet = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputDeliverLeaflet, deliverLeafletSupportCalculator);
			ruleDeliverLeaflet.Parameters.WEIGHT = 0.9;
			ruleDeliverLeaflet.Parameters.PARTIAL_MATCH_ON = true;
            CurrentAgent.Commit(ruleDeliverLeaflet);

            // Create Rule to Stop
			SupportCalculator stopSupportCalculator = FixedRuleToStop;
			FixedRule ruleStop = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputStop, stopSupportCalculator);
			ruleStop.Parameters.WEIGHT = 1;
			ruleStop.Parameters.PARTIAL_MATCH_ON = true;
            CurrentAgent.Commit(ruleStop);


            // Disable Rule Refinement
            CurrentAgent.ACS.Parameters.PERFORM_RER_REFINEMENT = false;

            // The selection type will be probabilistic
            CurrentAgent.ACS.Parameters.LEVEL_SELECTION_METHOD = ActionCenteredSubsystem.LevelSelectionMethods.STOCHASTIC;

            // The action selection will be fixed (not variable) i.e. only the statement defined above.
            CurrentAgent.ACS.Parameters.LEVEL_SELECTION_OPTION = ActionCenteredSubsystem.LevelSelectionOptions.FIXED;

            // Define Probabilistic values
            CurrentAgent.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 1;
            CurrentAgent.ACS.Parameters.FIXED_IRL_LEVEL_SELECTION_MEASURE = 0;
            CurrentAgent.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 0;
            CurrentAgent.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 0;
        }

        /// <summary>
        /// Make the agent perception. In other words, translate the information that came from sensors to a new type that the agent can understand
        /// </summary>
        /// <param name="sensorialInformation">The information that came from server</param>
        /// <returns>The perceived information</returns>
		private SensoryInformation prepareSensoryInformation(IList<Thing> listOfThings)
        {   
            SensoryInformation si = World.NewSensoryInformation(CurrentAgent);

            Creature c = (Creature) listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_CREATURE)).First();

			int n = 0;
            int reds_needed = 0;
            int greens_needed = 0; 
            int blues_needed = 0; 
            int yellows_needed = 0; 
            int magentas_needed = 0; 
            int whites_needed = 0; 
            Console.WriteLine($"{mind.leafletsDelivered[0]} {mind.leafletsDelivered[01]} {mind.leafletsDelivered[2]}");
			foreach(Leaflet l in c.getLeaflets()) {
                // Console.WriteLine(n);
                // Console.WriteLine(l.situation);
                // Console.WriteLine(mind.leafletsDelivered[n]);
                if (mind.leafletsDelivered[n]){
				    n++;
                    continue;
                } 
				mind.updateLeaflet(n,l);
                mind.updateLeafletCompletion(n);
                reds_needed += l.getRequired("Red");
                greens_needed += l.getRequired("Green"); 
                blues_needed += l.getRequired("Blue"); 
                yellows_needed += l.getRequired("Yellow"); 
                magentas_needed += l.getRequired("Magenta"); 
                whites_needed += l.getRequired("White"); 
                if (l.situation){
                    leafletId = l.leafletID.ToString();
                    leafletNumber = n;
                    break;
                }
				n++;
			}


            //leaflets
            reds_needed = reds_needed - mind.red;
            // Console.WriteLine("reds_needed:");
            // Console.WriteLine(reds_needed);

            greens_needed = greens_needed - mind.green;
            // Console.WriteLine("greens_needed:");
            // Console.WriteLine(greens_needed);

            blues_needed = blues_needed - mind.blue;
            // Console.WriteLine("blues_needed:");
            // Console.WriteLine(blues_needed);

            yellows_needed = yellows_needed - mind.yellow;
            // Console.WriteLine("yellows_needed:");
            // Console.WriteLine(yellows_needed);

            magentas_needed = magentas_needed - mind.magenta;
            // Console.WriteLine("magentas_needed:");
            // Console.WriteLine(magentas_needed);

            whites_needed = whites_needed - mind.white;
            // Console.WriteLine("whites_needed:");
            // Console.WriteLine(whites_needed);

            Boolean stopBool = false;

            // Detect if we have a wall ahead
            IEnumerable<Thing>  wallsAhead = listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_BRICK && item.DistanceToCreature <= 61));
            Boolean wallAhead = wallsAhead.Any();

            // Detect if we have a jewel ahead
            IEnumerable<Thing> jewelsAhead = listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_JEWEL && item.DistanceToCreature <= 30));
            Boolean jewelAhead = jewelsAhead.Any();

            // Detect if we have a food ahead
            IEnumerable<Thing> foodsAhead = listOfThings.Where(item => ((item.CategoryId == Thing.CATEGORY_FOOD || item.CategoryId == Thing.categoryPFOOD || item.CategoryId == Thing.CATEGORY_NPFOOD) && item.DistanceToCreature <= 30));
            Boolean foodAhead = foodsAhead.Any();

            // Detect if we have a deliverSpot ahead
            IEnumerable<Thing> deliverSpotsAhead = listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_DeliverySPOT && item.DistanceToCreature <= 30));
            Boolean deliverSpotAhead = deliverSpotsAhead.Any();

            // Detect if we have a jewel só pegar das cores necessárias
            IEnumerable<Thing> jewels = listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_JEWEL));
            Boolean jewel = jewels.Any();

            // Detect if we have a food 
            IEnumerable<Thing> foods = listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_FOOD));
            Boolean food = foods.Any();

            // Detect if we have a deliverSpot 
            IEnumerable<Thing> deliverSpots = listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_DeliverySPOT));
            Boolean deliverSpotBool = deliverSpots.Any();

            
            //information
            if(jewelAhead){
                closestJewel = jewelsAhead.First();
                jewelName = closestJewel.Name;
            }
            
            if(foodAhead) {
                closestFood = foodsAhead.First();
                foodName = closestFood.Name;
            }
                
            if(deliverSpotAhead) {
                deliverSpot = deliverSpotsAhead.First();
                deliverName = deliverSpot.Name;
                deliverSpotAhead = mind.canDeliverSomething();
            }

            if(jewel) {
                closestJewel = jewels.OrderBy(item => item.DistanceToCreature).First();
                String jewelColor = closestJewel.Material.Color;
                switch(jewelColor){
                    case "Red":
                        jewel = reds_needed > 0;
                        break;
                    case "Green":
                        jewel = greens_needed > 0;
                        break;
                    case "Blue":
                        jewel = blues_needed > 0;
                        break;
                    case "Yellow":
                        jewel = yellows_needed > 0;
                        break;
                    case "Magenta":
                        jewel = magentas_needed > 0;
                        break;
                    case "White":
                        jewel = whites_needed > 0;
                        break;
                    default: break;
                }
            }

            if(food) {
                closestFood = foods.OrderBy(item => item.DistanceToCreature).First();
                food = c.Fuel < 400;
            }

            if(deliverSpotBool) {
                deliverSpot = deliverSpots.OrderBy(item => item.DistanceToCreature).First();
                deliverSpotBool = mind.canDeliverSomething();
            }
            Console.WriteLine($"DeliverSpotAhead :");
            Console.WriteLine(deliverSpotAhead);
            Console.WriteLine("DeliverSpotLonge: ");
            Console.WriteLine(deliverSpotBool);


            //activations


            //normal
            double wallAheadActivationValue = wallAhead ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
            double jewelAheadActivationValue = jewelAhead ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
            double foodAheadActivationValue = foodAhead ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
            double deliverSpotAheadActivationValue = deliverSpotAhead ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
            double jewelActivationValue = jewel ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
            double foodActivationValue = food ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
            double deliverSpotActivationValue = deliverSpotBool ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
            double stopActivationValue = stopBool ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;


            // Pode entregar mas não tá vendo o ponto: PROCURAR PONTO
            if(mind.canDeliverSomething() && !deliverSpotBool){
                wallAheadActivationValue = CurrentAgent.Parameters.MAX_ACTIVATION; // trata como se tudo fosse parede
                jewelAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                foodAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                deliverSpotAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                jewelActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                foodActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                deliverSpotActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                stopActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
            }

            // Pode entregar e está vendo o ponto: ENTREGAR
            if(mind.canDeliverSomething() && deliverSpotAhead){
                wallAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION; 
                jewelAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                foodAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                deliverSpotAheadActivationValue = CurrentAgent.Parameters.MAX_ACTIVATION;
                jewelActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                foodActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                deliverSpotActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                stopActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
            }

            // Está no ponto mas não pode entregar: RODEAR
            if(!mind.canDeliverSomething() && deliverSpotAhead){
                wallAheadActivationValue = CurrentAgent.Parameters.MAX_ACTIVATION; 
                jewelAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                foodAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                deliverSpotAheadActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                jewelActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                foodActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                deliverSpotActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
                stopActivationValue = CurrentAgent.Parameters.MIN_ACTIVATION;
            }
            

            //add to agent sensory information
            si.Add(inputWallAhead, wallAheadActivationValue);
            si.Add(inputJewelAhead, jewelAheadActivationValue);
            si.Add(inputFoodAhead, foodAheadActivationValue);
            si.Add(inputDeliverSpotAhead, deliverSpotAheadActivationValue);
            si.Add(inputJewel, jewelActivationValue);
            si.Add(inputFood, foodActivationValue);
            si.Add(inputDeliverSpot, deliverSpotActivationValue);
            si.Add(inputStop, stopActivationValue);
            
            return si;
        }
        #endregion

        #region Fixed Rules
        private double FixedRuleToAvoidCollisionWall(ActivationCollection currentInput, Rule target)
        {
            // See partial match threshold to verify what are the rules available for action selection
            return ((currentInput.Contains(inputWallAhead, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }

        private double FixedRuleToGoAhead(ActivationCollection currentInput, Rule target)
        {
            // Here we will make the logic to go ahead
            return ((currentInput.Contains(inputWallAhead, CurrentAgent.Parameters.MIN_ACTIVATION))) ? 1.0 : 0.0;
        }

        private double FixedRuleToGoJewel(ActivationCollection currentInput, Rule target)
        {
            return ((currentInput.Contains(inputJewel, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }
        
        private double FixedRuleToGoFood(ActivationCollection currentInput, Rule target)
        {
            return ((currentInput.Contains(inputFood, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }
        
        private double FixedRuleToGoDeliver(ActivationCollection currentInput, Rule target)
        {
            return ((currentInput.Contains(inputDeliverSpot, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }
        
        private double FixedRuleToGetJewel(ActivationCollection currentInput, Rule target)
        {
            return ((currentInput.Contains(inputJewelAhead, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }
        
        private double FixedRuleToEatFood(ActivationCollection currentInput, Rule target)
        {
            return ((currentInput.Contains(inputFoodAhead, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }
        
        private double FixedRuleToDeliverLeaflet(ActivationCollection currentInput, Rule target)
        {
            return ((currentInput.Contains(inputDeliverSpotAhead, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }

        private double FixedRuleToStop(ActivationCollection currentInput, Rule target)
        {
            return ((currentInput.Contains(inputStop, CurrentAgent.Parameters.MAX_ACTIVATION))) ? 1.0 : 0.0;
        }


        #endregion

        #region Run Thread Method
        private void CognitiveCycle(object obj)
        {

			Console.WriteLine("Starting Cognitive Cycle ... press CTRL-C to finish !");
            // Cognitive Cycle starts here getting sensorial information
            while (CurrentCognitiveCycle != MaxNumberOfCognitiveCycles)
            {   
				// Get current sensory information                    
				IList<Thing> currentSceneInWS3D = processSensoryInformation();
                Console.WriteLine(currentSceneInWS3D);

                // Make the perception
                SensoryInformation si = prepareSensoryInformation(currentSceneInWS3D);

                //Perceive the sensory information
                CurrentAgent.Perceive(si);

                //Choose an action
                ExternalActionChunk chosen = CurrentAgent.GetChosenExternalAction(si);

                // Get the selected action
                String actionLabel = chosen.LabelAsIComparable.ToString();
                CreatureActions actionType = (CreatureActions)Enum.Parse(typeof(CreatureActions), actionLabel, true);

                // Call the output event handler
				processSelectedAction(actionType);

                // Increment the number of cognitive cycles
                CurrentCognitiveCycle++;

                //Wait to the agent accomplish his job
                if (TimeBetweenCognitiveCycles > 0)
                {
                    Thread.Sleep(TimeBetweenCognitiveCycles);
                }
			}
        }
        #endregion

    }
}
