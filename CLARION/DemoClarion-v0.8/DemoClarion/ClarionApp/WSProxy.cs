using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using ClarionApp.Exceptions;
using ClarionApp.Model;

namespace ClarionApp
{
    public class WSProxy:IDisposable
    {
        #region Properties

        /// <summary>
        /// The Server Host Name Address
        /// </summary>
        private String hostName = null;
        
        /// <summary>
        /// The Server Host Port 
        /// </summary>
        private Int32 port;

        /// <summary>
        /// The .NET Socket Class
        /// </summary>
        private TcpClient clientSocket = null;

        /// <summary>
        /// Max Read Buffer Size in bytes
        /// </summary>
        private const Int32 MAX_READ_BYTES_SIZE = 1024 * 5;

        /// <summary>
        /// Check the connection with World Server is up
        /// </summary>
        public Boolean IsConnected
        {
            get 
            {
                Boolean isConnected = false;

                if (clientSocket != null && clientSocket.Connected)
                {
                    isConnected = true;
                }

                return isConnected; 
            }            
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">The Server Host Name Address</param>
        /// <param name="port">The Server Host Port</param>
        /// <exception cref="WorldServerInvalidArgument">Throws this exception if some argument is invalid</exception>
        public WSProxy(String hostName, String port)
        {                                   
            Int32 portInt = 0;

            if (!String.IsNullOrWhiteSpace(hostName))
            {
                this.hostName = hostName.Trim();
            }
            else
            {
                throw new WorldServerInvalidArgument("Invalid argument for Host Name");
            }

            if (!String.IsNullOrWhiteSpace(port) && Int32.TryParse(port,out portInt) && portInt > 0)
            {
                this.port = portInt;
            }
            else
            {
                throw new WorldServerInvalidArgument("Invalid argument for Port Number");
            }

            clientSocket = new TcpClient();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">The Server Host Name Address</param>
        /// <param name="port">The Server Host Port</param>
        /// <exception cref="WorldServerInvalidArgument">Throws this exception if some argument is invalid</exception>
        public WSProxy(String hostName, Int32 port)
        {
            if (!String.IsNullOrWhiteSpace(hostName))
            {
                this.hostName = hostName.Trim();
            }
            else
            {
                throw new WorldServerInvalidArgument("Invalid argument for Host Name");
            }

            if (port <= 0)
            {
                throw new WorldServerInvalidArgument("Invalid argument for Port Number");
            }
            
            this.port = port;
            clientSocket = new TcpClient();
        }

        #endregion

        #region Connect Methods

        /// <summary>
        /// Connect to the Host Name 
        /// </summary>
        /// <returns>The message returned when a successful connection is made</returns>
        /// <exception cref="WorldServerConnectionError">Throws this exception if it was not possible to accomplish the task</exception>
        public string Connect()
        {
            String messageReturned = String.Empty;
            
            try
            {
                if (!IsConnected && clientSocket != null)
                {
                    clientSocket.Connect(hostName, port);
                    messageReturned = ReadMessage();
                }
            }
            catch (Exception e)
            {
                throw new WorldServerConnectionError("Error while attempting to connect on World Server", e);
            }

            return messageReturned;
        }

        #endregion

        #region Creature Command Methods

        /// <summary>
        /// Send Control Diff Comand
        /// </summary>
        /// <param name="creatureId">The creature ID</param>
        /// <param name="vr">The right wheel velocity</param>
        /// <param name="vl">The left wheel velocity</param>
        /// <param name="x">The X destination position</param>
        /// <param name="y">THe Y destination position</param>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerSendError">Send Exception</exception>
        /// <returns>The messages received from World Server</returns>
        public string SendSetGoTo(string creatureId, double vr, double vl, double x, double y)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("setGoTo ");
                builder.Append(creatureId);
                builder.Append(" ");
                builder.Append(vr);
                builder.Append(" ");
                builder.Append(vl);
                builder.Append(" ");
                builder.Append(x);
                builder.Append(" ");
                builder.Append(y);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return response;
        }

        /// <summary>
        /// Send Control Difft Comand
        /// </summary>
        /// <param name="creatureId">The creature ID</param>
        /// <param name="v">The velocity</param>
        /// <param name="vr">The right wheel velocity</param>
        /// <param name="vl">The left wheel velocity</param>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerSendError">Send Exception</exception>
        /// <returns>The messages received from World Server</returns>
        public string SendSetTurn(string creatureId, double v, double vr, double vl)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("setTurn ");
                builder.Append(creatureId);
                builder.Append(" ");
                builder.Append(v);
                builder.Append(" ");
                builder.Append(vr);
                builder.Append(" ");
                builder.Append(vl);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return response;
        }


		public string SendSetAngle(string creatureId, double vr, double vl, double w)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("setAngle ");
                builder.Append(creatureId);
                builder.Append(" ");
                builder.Append(vr);
                builder.Append(" ");
                builder.Append(vl);
                builder.Append(" ");
                builder.Append(w);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }
            return response;
        }
        /// <summary>
        /// Send Start Creature Camera
        /// </summary>
        /// <param name="creatureId">The creature ID</param>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerSendError">Send Exception</exception>
        /// <returns>The messages received from World Server</returns>
        public string SendStartCamera(string creatureId)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("camera ");
                builder.Append(creatureId);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return response;
        }

        /// <summary>
        /// Send Start Creature Command
        /// </summary>
        /// <param name="creatureId">The creature ID</param>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerSendError">Send Exception</exception>
        /// <returns>The messages received from World Server</returns>
        public string SendStartCreature(string creatureId)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("start ");
                builder.Append(creatureId);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return response;
        }

        /// <summary>
        /// Send Stop Creature Command
        /// </summary>
        /// <param name="creatureId">The creature ID</param>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerSendError">Send Exception</exception>
        /// <returns>The messages received from World Server</returns>
        public string SendStopCreature(string creatureId)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("stop ");
                builder.Append(creatureId);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return response;
        }
		
		

        /// <summary>
        /// Send Eat Command
        /// </summary>
        /// <param name="creatureId">The creature ID</param>
        /// <param name="thingName">The thing that will be eaten</param>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerSendError">Send Exception</exception>
        /// <returns>The messages received from World Server</returns>
        public string SendEatIt(string creatureId, String thingName)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("eatit ");
                builder.Append(creatureId);
                builder.Append(" ");
                builder.Append(thingName);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return response;
        }

        /// <summary>
        /// Send Sack Command
        /// </summary>
        /// <param name="creatureId">The creature ID</param>
        /// <param name="thingName">The thing that will be sacked</param>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerSendError">Send Exception</exception>
        /// <returns>The messages received from World Server</returns>
        public string SendSackIt(string creatureId, String thingName)
        {
            String response = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("sackit ");
                builder.Append(creatureId);
                builder.Append(" ");
                builder.Append(thingName);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return response;
        }

		/// <summary>
		/// Send CreateLeaflet Command
		/// </summary>
		/// <exception cref="WorldServerConnectionError">Connection Exception</exception>
		/// <exception cref="WorldServerReadError">Read Exception</exception>
		/// <exception cref="WorldServerSendError">Send Exception</exception>
		/// <returns>The messages received from World Server</returns>
		public string SendCreateLeaflet()
		{
			String response = String.Empty;

			try
			{
				// Prepare the message
				StringBuilder builder = new StringBuilder();
				builder.Append("leaflet");

				// Send Message
				SendMessage(builder.ToString());

				// Read the response
				response = ReadMessage();
			}
			catch (WorldServerConnectionError connEx)
			{
				throw connEx;
			}
			catch (WorldServerSendError sendEx)
			{
				throw sendEx;
			}
			catch (WorldServerReadError readEx)
			{
				throw readEx;
			}
			catch (Exception e)
			{
				throw new WorldServerSendError("Error while sending message", e);
			}

			return response;
		}

		/// <summary>
		/// Send WorldReset Command
		/// </summary>
		/// <exception cref="WorldServerConnectionError">Connection Exception</exception>
		/// <exception cref="WorldServerReadError">Read Exception</exception>
		/// <exception cref="WorldServerSendError">Send Exception</exception>
		/// <returns>The messages received from World Server</returns>
		public string SendWorldReset()
		{
			String response = String.Empty;

			try
			{
				// Prepare the message
				StringBuilder builder = new StringBuilder();
				builder.Append("worldreset");

				// Send Message
				SendMessage(builder.ToString());

				// Read the response
				response = ReadMessage();
			}
			catch (WorldServerConnectionError connEx)
			{
				throw connEx;
			}
			catch (WorldServerSendError sendEx)
			{
				throw sendEx;
			}
			catch (WorldServerReadError readEx)
			{
				throw readEx;
			}
			catch (Exception e)
			{
				throw new WorldServerSendError("Error while sending message", e);
			}

			return response;
		}

        /// <summary>
        /// Send Full Status 3d command
        /// </summary>
        /// <param name="creatureId">The creature Id</param>
        /// <returns>A list of things that in the visual field of the creature</returns>
        public List<Thing> SendGetCreatureState(string creatureId)
        {
            List<Thing> returnDic = null;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("getcreaturestate ");
                builder.Append(creatureId);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                String response = ReadMessage();

                // Parse the response
                returnDic = ParseGetCreatureStateResponse(response, new String[]{"||"} );
            }
            catch (WorldServerConnectionError connEx)
            {
                throw connEx;
            }
            catch (WorldServerSendError sendEx)
            {
                throw sendEx;
            }
            catch (WorldServerReadError readEx)
            {
                throw readEx;
            }
            catch (Exception e)
            {
                throw new WorldServerSendError("Error while sending message", e);
            }

            return returnDic;
        }

		/// <summary>
		/// Send GetSack Command
		/// </summary>
		/// <exception cref="WorldServerConnectionError">Connection Exception</exception>
		/// <exception cref="WorldServerReadError">Read Exception</exception>
		/// <exception cref="WorldServerSendError">Send Exception</exception>
		/// <returns>The messages received from World Server</returns>
		public Sack SendGetSack(string creatureId)
		{

			Sack sack = new Sack();
			try
			{   

				// Prepare the message
				StringBuilder builder = new StringBuilder();
				builder.Append("getsack ");
				builder.Append(creatureId);

				// Send Message
				SendMessage(builder.ToString());

				// Read the response
				String response = ReadMessage();
				string[] tokens = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				var enumerator = tokens.GetEnumerator();

				// Parse the response

				SetAttribute(sack, "n_food", enumerator);
				SetAttribute(sack, "n_crystal", enumerator);
				SetAttribute(sack, "p_food", enumerator);
				SetAttribute(sack, "np_food", enumerator);
				SetAttribute(sack, "red_crystal", enumerator);
				SetAttribute(sack, "green_crystal", enumerator);
				SetAttribute(sack, "blue_crystal", enumerator);
				SetAttribute(sack, "yellow_crystal", enumerator);
				SetAttribute(sack, "magenta_crystal", enumerator);
				SetAttribute(sack, "white_crystal", enumerator);

				//returnDic = ParseGetCreatureStateResponse(response, new String[]{"||"} );
			}
			catch (WorldServerConnectionError connEx)
			{
				throw connEx;
			}
			catch (WorldServerSendError sendEx)
			{
				throw sendEx;
			}
			catch (WorldServerReadError readEx)
			{
				throw readEx;
			}
			catch (Exception e)
			{
				throw new WorldServerSendError("Error while sending message", e);
			}

			return sack;
		}

        #endregion
		
		#region New Methods
		
		/// <summary>
		/// Create a New Creature 
		/// </summary>
		/// <param name="x">Position X</param>
        /// <param name="y">Position Y</param>
		/// <param name="phi">Creature Angle</param>
        /// <param name="creatureId">The Creature Id</param>
        /// <param name="creatureName"> The Creature Name</param>
        public void NewCreature(Int32 x, Int32 y, Int32 phi, out String creatureId, out String creatureName)
		{
            String response = String.Empty;;
            creatureId = String.Empty;
            creatureName = String.Empty;

            try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("new ");
                builder.Append(x);
                builder.Append(" ");
				builder.Append(y);
                builder.Append(" ");
                builder.Append(phi);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();

                if (!String.IsNullOrWhiteSpace(response))
                {
                    string[] tokens = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    creatureId = (tokens != null && tokens.Length > 0) ? tokens[0] : null;
                    creatureName = (tokens != null && tokens.Length > 1) ? tokens[1] : null;
                }

            }
            catch (WorldServerConnectionError connEx)
            {throw connEx; }
            catch (WorldServerSendError sendEx)
            {throw sendEx; }
            catch (WorldServerReadError readEx)
            {throw readEx;}
            catch (Exception e)
            {throw new WorldServerSendError("Error while sending message", e);}
		}	
		
		/// <summary>
		/// Creature a new jewel
		/// </summary>
		/// <param name="type">Jewel Type</param>
		/// <param name="x">Jewel X Position</param>
		/// <param name="y">Jewel Y Position</param>
		/// <returns>Jewel's name</returns>
        public String NewJewel(Int32 type, Int32 x, Int32 y)
		{
            String response = String.Empty;
            String jewelName = String.Empty;

			try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("jewel ");
                builder.Append(type);
                builder.Append(" ");
				builder.Append(x);
                builder.Append(" ");
                builder.Append(y);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();

                if (!String.IsNullOrWhiteSpace(response))
                {
                    string[] tokens = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    jewelName = (tokens != null && tokens.Length > 1) ? tokens[1] : null;
                }

                return jewelName;
            }
            catch (WorldServerConnectionError connEx)
            {throw connEx; }
            catch (WorldServerSendError sendEx)
            {throw sendEx; }
            catch (WorldServerReadError readEx)
            {throw readEx;}
            catch (Exception e)
            {throw new WorldServerSendError("Error while sending message", e);}
		}	
		
		/// <summary>
		/// Create a new Food
		/// </summary>
		/// <param name="type">Food Type</param>
		/// <param name="x">X Position</param>
		/// <param name="y">Y Position</param>
		/// <returns>Food's name</returns>
        public String NewFood(Int32 type, Int32 x, Int32 y)
		{
            String response = String.Empty;
            String foodName = String.Empty;

			try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("food ");
                builder.Append(type);
                builder.Append(" ");
				builder.Append(x);
                builder.Append(" ");
                builder.Append(y);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();

                if (!String.IsNullOrWhiteSpace(response))
                {
                    string[] tokens = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foodName = (tokens != null && tokens.Length > 1) ? tokens[1] : null;
                }

                return foodName;
            }
            catch (WorldServerConnectionError connEx)
            {throw connEx; }
            catch (WorldServerSendError sendEx)
            {throw sendEx; }
            catch (WorldServerReadError readEx)
            {throw readEx;}
            catch (Exception e)
            {throw new WorldServerSendError("Error while sending message", e);}
		}	
		
		/// <summary>
		/// Create a new brick
		/// </summary>
		/// <param name="type">Brick type</param>
		/// <param name="x1">X1 Position</param>
        /// <param name="y1">Y1 Position</param>
        /// <param name="x2">X2 Position</param>
        /// <param name="y2">Y1 Position</param>
		/// <returns>Brick's name</returns>
		public string NewBrick(Int32 type, Int32 x1, Int32 y1, Int32 x2, Int32 y2)
		{
            String response = String.Empty;
            String brickName = String.Empty;

			try
            {
                // Prepare the message
                StringBuilder builder = new StringBuilder();
                builder.Append("brick ");
                builder.Append(type);
                builder.Append(" ");
				builder.Append(x1);
                builder.Append(" ");
                builder.Append(y1);
				builder.Append(" ");
				builder.Append(x2);
                builder.Append(" ");
                builder.Append(y2);

                // Send Message
                SendMessage(builder.ToString());

                // Read the response
                response = ReadMessage();

                if (!String.IsNullOrWhiteSpace(response))
                {
                    string[] tokens = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    brickName = (tokens != null && tokens.Length > 1) ? tokens[1] : null;
                }

                return brickName;
            }
            catch (WorldServerConnectionError connEx)
            {throw connEx; }
            catch (WorldServerSendError sendEx)
            {throw sendEx; }
            catch (WorldServerReadError readEx)
            {throw readEx;}
            catch (Exception e)
            {throw new WorldServerSendError("Error while sending message", e);}
		}	
		
		#endregion

        #region Disposable Methods

        /// <summary>
        /// Dispose Allocated Resources
        /// </summary>
        public void Dispose()
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket = null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Send a message to World Server
        /// </summary>
        /// <param name="message">The message that will be sent</param>
        /// <exception cref="WorldServerSendError">Send Message Exception</exception>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        private void SendMessage(String message)
        {
            if (IsConnected)
            {
                NetworkStream netStream = clientSocket.GetStream();

                if (netStream != null && netStream.CanWrite)
                {
                    // Translate the passed message into ASCII and store it as a Byte array.
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(message + Environment.NewLine);
                    netStream.Write(data, 0, data.Length);
                }
                else
                {
                    throw new WorldServerSendError("It is not possible to write in the socket");
                }
            }
            else
            {
                throw new WorldServerConnectionError("World Server is not connected");
            }
        }

        /// <summary>
        /// Read a message from World Server 
        /// </summary>
        /// <param name="dataLengthToRead">The data Length that must be read. Default is set to MAX_READ_BYTES_SIZE constant</param>
        /// <exception cref="WorldServerReadError">Read Exception</exception>
        /// <exception cref="WorldServerConnectionError">Connection Exception</exception>
        /// <returns>The message that was read</returns>
        private String ReadMessage(long dataLengthToRead = MAX_READ_BYTES_SIZE)
        {
            String response = String.Empty;

            if (IsConnected)
            {
                NetworkStream netStream = clientSocket.GetStream();

                if (netStream != null && netStream.CanRead)
                {
                    // read and display the response
                    Byte[] data = new Byte[dataLengthToRead];

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = netStream.Read(data, 0, data.Length);
                    response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                }
                else
                {
                    throw new WorldServerReadError("It is not possible to read in the socket");
                }
            }
            else
            {
                throw new WorldServerConnectionError("World Server is not connected");
            }

            return response;
        }

        /// <summary>
        /// Process the response and parse elements
        /// </summary>
        /// <param name="response">The response provided by the server.</param>
        /// <param name="ignoredTokens">The ignored tokens. They will not be processed.</param>
        /// <returns>A list of parsed things</returns>
        private List<Thing> ParseGetCreatureStateResponse(string response, string[] ignoredTokens)
        {
            List<Thing> returnValue = null;
			//Console.WriteLine (response);

            try
            {
                if (!String.IsNullOrWhiteSpace(response))
                {
                    string[] tokens = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] tokensProcessed = null;

                    // Remove unecessary tokens
                    if (ignoredTokens != null && ignoredTokens.Length > 0)
                    {
                        IList<String> processed = new List<String>();

                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (!ignoredTokens.Contains(tokens[i]))
                            {
                                processed.Add(tokens[i]);
                            }
                        }

                        tokensProcessed = processed.ToArray();
                    }
                    else
                    {
                        tokensProcessed = tokens;
                    }

                    // Parse tokens
                    if (tokensProcessed != null && tokensProcessed.Length > 0)
                    {
                        // Things list to return
                        returnValue = new List<Thing>();

                        // Get Current Enumerator
                        var enumerator = tokensProcessed.GetEnumerator();

                        // First Parse the creature   
                        Creature creature = new Creature();
                        creature.Material = new Material3d();

						//X Y size pitch motorSys wheel speed fuel color camera
                        
                        // Set Creature Attributes
                        SetAttribute(creature, "Name", enumerator);
						SetAttribute(creature, "index", enumerator);
                        SetAttribute(creature, "X1", enumerator);
                        SetAttribute(creature, "Y1", enumerator);
                        SetAttribute(creature, "Size", enumerator);
                        SetAttribute(creature, "Pitch", enumerator);
                        SetAttribute(creature, "MotorSystem", enumerator);
                        SetAttribute(creature, "Wheel", enumerator);
                        SetAttribute(creature, "Speed", enumerator);
                        SetAttribute(creature, "Fuel", enumerator);
						SetAttribute(creature, "serotonin", enumerator);
						SetAttribute(creature, "endorphine", enumerator);
						SetAttribute(creature, "score", enumerator);
						SetAttribute(creature, "XX1", enumerator);
						SetAttribute(creature, "YY1", enumerator);
						SetAttribute(creature, "XX2", enumerator);
						SetAttribute(creature, "YY2", enumerator);
                        SetAttribute(creature.Material, "Color", enumerator);
						SetAttribute(creature, "actionData", enumerator);
						SetAttribute(creature, "HasCollided", enumerator);
                        SetAttribute(creature, "HasLeaflet", enumerator);

						// parsing of leaflets
						if (creature.HasLeaflet) {
							creature.leaflets = new List<Leaflet>();
							SetAttribute(creature, "NumberOfLeaflets", enumerator);
							for (int i=0;i<creature.NumberOfLeaflets;i++) {
								Leaflet l = new Leaflet();
								SetAttribute(l, "leafletID", enumerator);
								SetAttribute(l, "NumberOfLeafletItems", enumerator);
								l.items = new List<LeafletItem>();
								for (int j=0;j< l.NumberOfLeafletItems;j++) {
								   LeafletItem li = new LeafletItem();
								   SetAttribute(li, "itemKey", enumerator);
								   SetAttribute(li, "totalNumber", enumerator);
								   SetAttribute(li, "collected", enumerator);
								   l.items.Add(li);
								}
								//28 payment int
								SetAttribute(l, "payment", enumerator);
								SetAttribute(l,"situation", enumerator);
								creature.leaflets.Add(l);
							}
						}
                        

                        

                        // Add Creature
                        returnValue.Add(creature);

                        // Get Things Quantity in visual sensor
                        Int32 thingsQuantity = 0;
                        if (enumerator != null && enumerator.MoveNext())
                        {
                            String currentToken = enumerator.Current as String;
                            thingsQuantity = Convert.ToInt32(currentToken, new CultureInfo("en-US"));
                        }
                        else
                        {
                            throw new WorldServerErrorProcessingResponse("Could not evaluate property: things quantity");
                        }

                        // Parse Objects
                        for (int i = 0; i < thingsQuantity; i++)
                        {
                            Thing thing = new Thing();
                            thing.Material = new Material3d();

                            SetAttribute(thing, "Name", enumerator);
                            SetAttribute(thing, "CategoryId", enumerator);
                            SetAttribute(thing, "IsOccluded", enumerator);
                            SetAttribute(thing, "X1", enumerator);
                            SetAttribute(thing, "X2", enumerator);
                            SetAttribute(thing, "Y1", enumerator);
                            SetAttribute(thing, "Y2", enumerator);
                            SetAttribute(thing, "Pitch", enumerator);
                            SetAttribute(thing.Material, "Hardness", enumerator);
                            SetAttribute(thing.Material, "Energy", enumerator);
                            SetAttribute(thing.Material, "Taste", enumerator);
                            SetAttribute(thing.Material, "Color", enumerator);
							SetAttribute(thing, "comX", enumerator);
							SetAttribute(thing, "comY", enumerator);

                            thing.DistanceToCreature = CalculateDistanceToCreature(thing, creature);

                            returnValue.Add(thing);
                        }
                    }
                    else
                    {
                        throw new WorldServerErrorProcessingResponse("Tokens from server are null or empty");
                    }
                }
            }
            catch (WorldServerErrorProcessingResponse exRes)
            {
                throw exRes;
            }
            catch (Exception ex)
            {
                throw new WorldServerErrorProcessingResponse("Error while parsing GetFullStatus3d", ex);
            }

            return returnValue;
        }

        /// <summary>
        /// Calculate thing distance to the creature
        /// </summary>
        /// <param name="thing">Thing object</param>
        /// <param name="creature">Creature object</param>
        /// <returns>Te distance between thing and creature</returns>
        private double CalculateDistanceToCreature(Thing thing, Creature creature)
        {
            Double distance = Double.PositiveInfinity;

            if (thing != null && creature != null)
            {
                distance = 0;
                double maxX = Math.Max(thing.X1, thing.X2);
                double minX = Math.Min(thing.X1, thing.X2);
                double maxY = Math.Max(thing.Y1, thing.Y2);
                double minY = Math.Min(thing.Y1, thing.Y2);

                if (creature.X1 > maxX)
                {
                    distance += (creature.X1 - maxX) * (creature.X1 - maxX);
                }
                else if (creature.X1 < minX)
                {
                    distance += (minX - creature.X1) * (minX - creature.X1);
                }

                if (creature.Y1 > maxY)
                {
                    distance += (creature.Y1 - maxY) * (creature.Y1 - maxY);
                }
                else if (creature.Y1 < minY)
                {
                    distance += (minY - creature.Y1) * (minY - creature.Y1);
                }

                return Math.Sqrt(distance);

            }

            return distance;
        }

        /// <summary>
        /// Set a given property in a model with the next value in enumeration using reflection
        /// </summary>
        /// <param name="model">The model object</param>
        /// <param name="propertyName">The property name of the model</param>
        /// <param name="enumerator">The enumerator</param>
        private void SetAttribute(object model, string propertyName, System.Collections.IEnumerator enumerator)
        {
            if (model != null && !String.IsNullOrWhiteSpace(propertyName) && enumerator != null && enumerator.MoveNext())
            {
                PropertyInfo property = model.GetType().GetProperty(propertyName);

                if (property != null)
                {
                    String currentToken = enumerator.Current as String;
                    CultureInfo cultureInfo = new CultureInfo("en-US"); // Use this culture to parse numbers like 3.2

                    if (property.PropertyType == typeof(String))
                    {
                        property.SetValue(model, currentToken, null);
                    }
                    else if (property.PropertyType == typeof(Double))
                    {
                        property.SetValue(model, Convert.ToDouble(currentToken, cultureInfo), null);
                    }
					else if (property.PropertyType == typeof(Int64))
					{
						property.SetValue(model, Convert.ToInt64(currentToken, cultureInfo), null);
					}
                    else if (property.PropertyType == typeof(Int32))
                    {
                        property.SetValue(model, Convert.ToInt32(currentToken, cultureInfo), null);
                    }
                    else if (property.PropertyType == typeof(Boolean))
                    {
                        //Int32 value = Convert.ToInt32(currentToken, cultureInfo);
						//Boolean valueConverted = (value == 1);
						Boolean valueConverted = false;
						if (currentToken.Equals("true")) valueConverted = true;
						if (currentToken.Equals("1")) valueConverted = true;
                        property.SetValue(model, valueConverted, null);
                    }
                    else if (property.PropertyType == typeof(MotorSystemType))
                    {
                        property.SetValue(model, Convert.ToInt32(currentToken, cultureInfo), null);
                    }
                    else
                    {
                        throw new WorldServerErrorProcessingResponse("Property Type not recognized in parser");
                    }
                }
                else
                {
                    throw new WorldServerErrorProcessingResponse("Property Name does not exist");
                }
            }
            else
            {
                throw new WorldServerErrorProcessingResponse("Could not evaluate property: " + propertyName);
            }
        }

        #endregion
    }
}
