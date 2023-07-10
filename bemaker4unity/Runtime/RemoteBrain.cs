using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEditor;

namespace bemaker
{
    ///
    /// <summary>This class defines a remote controller for an agent of type Agent of the bemaker.
    /// Estes agentes recebem comandos de um script por meio de uma interface de comunicação em rede.
    /// So, Brain is a generic controller, awhile RemoteBrain implements an agent's network controller.
    /// </summary>
    public class RemoteBrain : Brain
    {   
        ///<summary>If true, the remote brain will be 
        ///managed manually. Thus, in this case, command 
        ///line arguments do not alter the properties of 
        ///the remote brain.</summary>
        public bool Managed {get; set;} = false;
        ///<summary>The IP of the bemaker2unity training server.</summary>
        public string Host {get; set;} = "127.0.0.1";
        ///<summary>The server port of the bemaker2unity training server.</summary>
        public int Port {get; set;} = 8080;
        public int ReceiveTimeout {get; set;} = 2000;
        public int ReceiveBufferSize {get; set;} = 81920;
        public int SendBufferSize {get; set;} = 81920;

        private string cmdname; //It's more recently received command/action name.
        private string[] args; //It's more recently command/action arguments.
        private bool runFirstTime = false;

        private IPAddress serverAddr; //controller address
        private EndPoint endPoint; //controller endpoint
        private Socket sockToSend; //Socket to send async message.

        public override void Setup(Agent agent){
            this.agent = agent;
            //one time configuration
            sockToSend = TrySocket();
            if (!Managed && runFirstTime){
                runFirstTime =false;
                string[] args = System.Environment.GetCommandLineArgs ();
                int i = 0;
                while (i < args.Length){
                    switch (args[i]) {
                        case "--bemaker_port":
                            Port = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--bemaker_timescale":
                            agent.ControlRequestor.defaultTimeScale = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            Time.timeScale = agent.ControlRequestor.defaultTimeScale;
                            i += 2;
                            break;
                        case "--bemaker_host":
                            Host = args[i+1];
                            i += 2;
                            break;
                        case "--bemaker_targetframerate":
                            Application.targetFrameRate = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--bemaker_vsynccount":
                            QualitySettings.vSyncCount = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        default:
                            i+=1;
                            break;
                    }
                }
            }
        }


        public Socket TrySocket()
        {
            if (sockToSend == null)
            {
                    serverAddr = IPAddress.Parse(Host);
                    endPoint = new IPEndPoint(serverAddr, Port);
                    sockToSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }
            return sockToSend;
        }

        public override void Close()
        {
            if (sockToSend != null)
            {
                //Debug.Log("Socket is closed...");
                sockToSend.Close();
            }
        }

        public bool sendData(byte[] data, out int total, byte[] received)
        {
            TrySocket().ReceiveTimeout = ReceiveTimeout;
            TrySocket().ReceiveBufferSize = ReceiveBufferSize;
            TrySocket().SendBufferSize = SendBufferSize;
            total = 0;
            try 
            { 
                sockToSend.SendTo(data, endPoint);
                total = sockToSend.Receive(received);
                if (total == 0)
                {
                    Debug.LogWarning($"Script bemaker4py is not connected in agent with ID equals to {agent.ID}!");
                    Debug.LogWarning(@"
                    Check one of these options to try fix this problem:
                        1) Add a RemoteConfiguration component to the current agent and change the appropriate 
                            network settings for communication with the agent’s controller.
                    
                        2) Check agent if agent ID in BasicAgent component is the same that agent's ID in controller (bemaker2py).

                        3) Try another network configution using the RemoteConfigution component in agent game object.
                    ");
                }
                return true;
            }
            catch(System.Exception e)
            {
                Debug.LogWarning($"Script bemaker4py is not connected in agent {agent.ID}! Start the bemaker2unity script! Network error: {e.Message}");
                #if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
                return false;
            }
        }
    }
}
