using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace bemaker
{

    public class Command  
    {
        public string name;
        public string[] args;
        public Command(string n, string[] args)
        {
            this.name = n;
            this.args = args;
        }
    }


    internal  class AgentComparer : IComparer<Agent>
    {
        public int Compare(Agent x, Agent y)
        {
            if (x.priority > y.priority)
            {
                return -1;
            } else if (x.priority < y.priority)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class ControlRequestor : MonoBehaviour
    {
        public float defaultTimeScale = 1.0f; 
        public bool physicsMode = true;
        public int skipFrame = 8;
        public bool repeatAction = false;
        public List<Agent> agents;

        void Awake()
        {
            Agent current = GetComponent<Agent>();
            if (current != null && !agents.Contains(current))
            {
                agents.Add(current);
            }
            agents.Sort(new AgentComparer());
            for (int i = 0; i < agents.Count; i++)
            {
                Agent a = agents[i];
                a.ControlInfo = new AgentControlInfo();
                a.ControlInfo.repeatAction = repeatAction;
                a.ControlInfo.skipFrame = skipFrame;
                a.ControlRequestor = this;
                a.SetupAgent();
            }

            Time.timeScale = defaultTimeScale;
        }

        public Command[] RequestEnvControl(Agent agent, RequestCommand request)
        {
            string cmdstr = null;
            if (agent.Brain is LocalBrain)
            {
                cmdstr = ((LocalBrain) (agent.Brain)).SendMessage(request.Command, request.Type, request.Value);
            }
            else
            {
                cmdstr = SendMessageFrom((RemoteBrain)agent.Brain, request.Command, request.Type, request.Value);
            }
            
            if (cmdstr != null)
            {
                Command[] cmds = UpdateActionData(cmdstr);
                return cmds;
            }
            
            return null;
        }


        public Command[] RequestControl(Agent agent)
        {
            agent.UpdateState();
            string cmdstr = null;
            if (agent.Brain is LocalBrain)
            {
                cmdstr = ((LocalBrain) (agent.Brain)).SendMessage(agent.MessageID, agent.MessageType, agent.MessageValue);
            }
            else
            {

                cmdstr = SendMessageFrom((RemoteBrain)agent.Brain, agent.MessageID, agent.MessageType, agent.MessageValue);
            }

            if (cmdstr != null)
            {
                Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
                Command[] cmds = UpdateActionData(cmdstr);
                if (cmds.Length > 0)
                {
                    agent.Brain.SetReceivedCommandName(cmds[0].name);
                    agent.Brain.SetReceivedCommandArgs(cmds[0].args);
                }
                for (int i = 1; i < cmds.Length; i++)
                {
                    fields[cmds[i].name] = cmds[i].args; 
                }
                agent.Brain.SetCommandFields(fields);
                return cmds;
            }

            return null;
        }

        private Command[] UpdateActionData(string cmd)
        {   
            string[] cmdTokens = cmd.Trim().Split('@');
            int nCmds = cmdTokens.Length;
            Command[] res = new Command[nCmds];
            int c = 0;
            foreach(string cmdToken in cmdTokens)
            { 
                //Debug.Log(cmdToken);
                string[] tokens = cmdToken.Trim().Split(';');
                if (tokens.Length < 2)
                {
                    string msg = "Invalid command exception: number of arguments is less then two : " + cmd;
                    throw new System.Exception(msg);
                }
                
                string cmdname = tokens[0].Trim();
                //Debug.Log(cmdname);
                int nargs = int.Parse(tokens[1].Trim());
                string[] args = new string[nargs];

                for (int i = 0; i < nargs; i++)
                {
                    args[i] = tokens[i+2];
                }

                res[c] = new Command(cmdname, args);
                c++;
            }
            return res;
        }

        private bool CheckCmd(Command[] cmds, string cmd)
        {
            if (cmds != null && cmds.Length > 0)
            {
                return cmds[0].name == cmd;
            }
            return false;
        }


        void FixedUpdate()
        {
            if (physicsMode)
            {
                bemakerUpdate();
            }
        }

        void Update()
        {
            if (!physicsMode)
            {
                bemakerUpdate();
            }
        }

        void bemakerUpdate()
        {
            foreach(var agent in agents)
            {
                AgentUpdate(agent);
            }
        }

        private void AgentUpdate(Agent agent)
        {
            if (agent == null || !agent.SetupIsDone)
            {
                if (agent == null)
                {
                    Debug.LogWarning("ControlRequest update loop called with null agent!");
                }
                return;
            }
            AgentControlInfo ctrl = agent.ControlInfo;
            if (!ctrl.envmode)
            {
                if (!ctrl.applyingAction)
                {
                    var cmd = RequestControl(agent);
                    if (CheckCmd(cmd, "__stop__") && !ctrl.stopped)
                    {
                        ctrl.stopped = true;
                        ctrl.applyingAction = false;
                        ctrl.frameCounter = 0;
                        agent.NSteps = 0;
                        //Debug.Log($"STOP::CMD{agent.ID}: {cmd[0].name}");
                    }
                    else if (CheckCmd(cmd, "__restart__"))
                    {
                        ctrl.lastResetId = cmd[0].args[0];
                        ctrl.frameCounter = 0;
                        agent.NSteps = 0;
                        ctrl.applyingAction = false;
                        ctrl.paused = false;
                        ctrl.stopped = false;
                        agent.AgentReset();
                        //Debug.Log($"RESTART::CMD{agent.ID}: {cmd[0].name}");
                    }
                    else if (CheckCmd(cmd, "__pause__"))
                    {
                        ctrl.applyingAction = false;
                        ctrl.paused = true;
                    }
                    else if (CheckCmd(cmd, "__resume__"))
                    {
                        ctrl.paused = false;
                    }
                    else if (CheckCmd(cmd, "__envcontrol__"))
                    {
                        ctrl.envmode = true;
                        ctrl.paused = true;
                    }
                    else if (!CheckCmd(cmd, "__waitnewaction__") && !(ctrl.paused || ctrl.stopped))
                    {
                        ctrl.applyingAction = true;
                        ctrl.frameCounter = 1;
                        agent.ResetReward();
                        agent.BeginOfStep();
                        agent.ApplyAction();
                        if (!agent.Alive())
                        {
                            ctrl.stopped = true;
                            ctrl.applyingAction = false;
                            agent.UpdateReward();
                            ctrl.paused = false;
                            ctrl.frameCounter = 0;
                            agent.NSteps = 0;
                        }
                    }
                }
                else if (!ctrl.stopped && !ctrl.paused)
                {
                    if (ctrl.frameCounter >= ctrl.skipFrame)
                    {
                        agent.UpdateReward();
                        ctrl.frameCounter = 0;
                        ctrl.applyingAction = false;
                        agent.NSteps = agent.NSteps + 1;
                    }
                    else
                    {
                        if (ctrl.repeatAction)
                        {
                            agent.ApplyAction();
                        } 
                        
                        if (!agent.Alive())
                        {
                            ctrl.stopped = true;
                            ctrl.applyingAction = false;
                            agent.UpdateReward();
                            ctrl.paused = false;
                            ctrl.frameCounter = 0;
                            agent.NSteps = 0;
                        }
                        else
                        {                        
                            ctrl.frameCounter ++;
                        }
                    }
                }
            } else
            {
                RequestCommand request = new RequestCommand(3);
                request.SetMessage(0, "__target__", bemaker.Brain.STR, "envcontrol");
                request.SetMessage(1, "wait_command", bemaker.Brain.STR, "restart, resume");
                request.SetMessage(2, "id", bemaker.Brain.STR, agent.ID);
                var cmds = RequestEnvControl(agent, request);

                if (cmds == null)
                {
                    throw new System.Exception($"bemaker2unity connection error! Agent ID: {agent.ID}.");
                }
                if (CheckCmd(cmds, "__restart__"))
                {
                    ctrl.lastResetId = cmds[0].args[0];
                    ctrl.frameCounter = -1;
                    agent.NSteps = 0;
                    Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
                    for (int i = 0; i < cmds.Length; i++)
                    {
                        fields[cmds[i].name] = cmds[i].args;
                    }
                    agent.Brain.SetCommandFields(fields);
                    ctrl.paused = false;
                    ctrl.stopped = false;
                    ctrl.applyingAction = false;
                    ctrl.envmode = false;
                    agent.AgentRestart();
                }
            }
        }

        /// <summary>
        /// Sends a message to the customer in the following format:
        /// [numberofields] [[descsize] [desc] [type] [valorsize] [value]] +
        /// where desc is a description of the message, type is the type of the message given as an integer such that:
        /// 0 = float
        /// 1 = int
        /// 2 = boolean
        /// 3 = string
        /// 4 = byte array
        /// e value is the value of the information sent.
        /// </summary>
        public string SendMessageFrom(RemoteBrain rbrain, string[] desc, byte[] tipo, string[] valor)
        {
            StringBuilder sb = new StringBuilder();
            int numberoffields = desc.Length;
            sb.Append(numberoffields.ToString().PadLeft(4,' ').PadRight(4, ' '));

            for (int i = 0; i < desc.Length; i++)
            {
                StringBuilder field = new StringBuilder();
                int descsize = Encoding.UTF8.GetByteCount(desc[i]);
                field.Append(descsize.ToString().PadLeft(4, ' ').PadRight(4,' '));
                field.Append(desc[i]);
                field.Append((int)tipo[i]);
                field.Append(Encoding.UTF8.GetByteCount(valor[i]).ToString().PadLeft(8, ' ').PadRight(8, ' '));
                field.Append(valor[i]);
                string fstr = field.ToString();
                sb.Append(fstr);
            }
            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] received = new byte[1000];
            int total = 0;
            if (rbrain.sendData(b, out total, received))
            {
                return Encoding.UTF8.GetString(received);
            }
            return null;
        }
    }
}
