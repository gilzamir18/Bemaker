using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bemaker;

namespace bemaker
{

    public class AgentInitilizer: MonoBehaviour
    {
        public virtual void Initilize(BasicAgent agent)
        {
            
        }
    }
    
    /// <summary>Basic Agent - A basic reinforcement learning agent structure. </summary>
    public class BasicAgent : Agent
    {
        public delegate void AgentEpisodeHandler(BasicAgent agent);
        public event AgentEpisodeHandler beforeTheResetEvent;
        public event AgentEpisodeHandler endOfEpisodeEvent;
        public event AgentEpisodeHandler beginOfEpisodeEvent;
        public event AgentEpisodeHandler endOfStepEvent;
        public event AgentEpisodeHandler beginOfStepEvent;
        public event AgentEpisodeHandler beginOfUpdateStateEvent;
        public event AgentEpisodeHandler endOfUpdateStateEvent;
        public event AgentEpisodeHandler beginOfApplyActionEvent;
        public event AgentEpisodeHandler endOfApplyActionEvent;  

        ///<summary> <code>doneAtNegativeReward</code> ends the simulation whenever the agent receives a negative reward.</summary>
        public bool doneAtNegativeReward = true;
        ///<summary> <code>doneAtPositiveReward</code> ends the simulation whenever the agent receives a positive reward.</summary>
        public bool doneAtPositiveReward = false;
        ///<summary>The maximum number of steps per episode.</summary>
        public int MaxStepsPerEpisode = 0;
        public float rewardScale = 1.0f;
        public List<RewardFunc> rewards;
        public List<AgentInitilizer> initilizers;
        public GameObject body;
        
        //Agent's ridid body
        private bool done;
        private bool truncated;
        protected float reward;
        private Dictionary<string, bool> firstTouch;
        private Dictionary<string, ISensor> sensorsMap;
        private List<Actuator> actuatorList;
        private List<ISensor> sensorList;

        private int numberOfSensors = 0;
        private int numberOfActuators = 0;
        private ModelMetadataLoader metadataLoader;


        public override void SetupAgent()
        {
            foreach(AgentInitilizer a in initilizers)
            {
                a.Initilize(this);
            }

            var controlConfig = GetComponent<ControlConfiguration>();
            if (controlConfig != null)
            {
                ControlInfo.skipFrame = controlConfig.skipFrame;
                ControlInfo.repeatAction = controlConfig.repeatAction;
            }

            if (remote)
            {   
                RemoteBrain r = new RemoteBrain();
                SetBrain(r);
                var config = GetComponent<RemoteConfiguration>();
                if (config != null)
                {
                    r.Port = config.port;
                    r.Host = config.host;
                    r.Managed = config.managed;
                    r.ReceiveTimeout = config.receiveTimeout;
                    r.ReceiveBufferSize = config.receiveBufferSize;
                    r.SendBufferSize = config.sendBufferSize;
                }

                brain.Setup(this);
                BMInitialize();
            }
            else 
            {
                Controller ctrl = GetComponent<Controller>();
                
                if (ctrl != null)
                {
                    SetBrain(new LocalBrain(ctrl));
                    brain.Setup(this);
                    BMInitialize();
                }
                else
                {   
                    Debug.LogWarning("Invalid agent configuration: Controller do not found for non-remote agent!");
                }
            }
        }


        public bool Done
        {
            get
            {
                return done;
            }

            set
            {
                bool pd = done;
                done = value;
                if (!pd && done)
                {
                    EndOfEpisode();
                }
            }
        }

        public bool Truncated
        {
            get
            {
                return truncated;
            }
        }

        public override void EndOfEpisode()
        {
            if (endOfEpisodeEvent != null)
            {
                endOfEpisodeEvent(this);
            }
        }

        public bool TryGetSensor(string key, out ISensor s)
        {
            return sensorsMap.TryGetValue(key, out s);
        }

        public virtual void RequestDoneFrom(RewardFunc rf) {
            Done = true;
        }

        public void RegisterRewardFunc(RewardFunc f)
        {
            if (rewards == null)
            {
                rewards = new List<RewardFunc>();
            }
            rewards.Add(f);
        }

        public bool UnregisterRewardFunc(RewardFunc f)
        {
            return rewards.Remove(f);
        }

        public void BMInitialize()
        {
            if (body == null)
            {
                body = gameObject;
            }
            if (ControlRequestor == null)
            {
                throw new System.Exception("ControlRequestor is mandatory to BasicAgent!");
            }
            setupIsDone = false;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            actuatorList = new List<Actuator>();
            sensorList = new List<ISensor>();
            sensorsMap = new Dictionary<string, ISensor>();
            if (rewards == null)
            {
                rewards = new List<RewardFunc>();
            }
            DoneSensor doneSensor = new DoneSensor();
            doneSensor.SetIsInput(false);
            doneSensor.SetAgent(this);
            sensorList.Add(doneSensor);
            sensorsMap[doneSensor.GetKey()] = doneSensor;

            AgentRewardSensor rewardSensor = new AgentRewardSensor();
            rewardSensor.SetRewardScale(rewardScale);
            rewardSensor.SetIsInput(false);
            rewardSensor.SetAgent(this);
            sensorList.Add(rewardSensor);
            sensorsMap[rewardSensor.GetKey()] = rewardSensor;

            IDSensor idSensor = new IDSensor();
            idSensor.SetIsInput(false);
            idSensor.SetAgent(this);
            sensorList.Add(idSensor);
            sensorsMap[idSensor.GetKey()] = idSensor;

            StepSensor stepSensor = new StepSensor();
            stepSensor.SetIsInput(false);
            stepSensor.SetAgent(this);
            sensorList.Add(stepSensor);
            sensorsMap[stepSensor.GetKey()] = stepSensor;

            AgentTruncatedSensor truncatedSensor = new AgentTruncatedSensor();
            truncatedSensor.SetIsInput(false);
            truncatedSensor.SetAgent(this);
            sensorList.Add(truncatedSensor);
            sensorsMap[truncatedSensor.GetKey()] = truncatedSensor;

            numberOfSensors = 5;

            for (int i = 0; i < transform.childCount; i++) 
            {
                GameObject obj = transform.GetChild(i).gameObject;
                Sensor s = obj.GetComponent<Sensor>();
                if (s != null && s.isActive)
                {
                    if (!sensorsMap.ContainsKey(s.perceptionKey))
                    {
                        sensorList.Add(s);
                        numberOfSensors++;
                        sensorsMap[s.perceptionKey] = s;
                    }
                    else
                    {
                        Debug.LogWarning($"A sensor with the name {s.perceptionKey} already exists on agent {name}. Only the first sensor added with the same name is added to agent.");
                    }
                }
                else
                {
                    if (obj.name == "sensors")
                    {
                        for (int j = 0; j < obj.transform.childCount; j++)
                        {
                            GameObject sobj = obj.transform.GetChild(j).gameObject;
                            Sensor s2 = sobj.GetComponent<Sensor>();
                            if (s2 != null && !sensorsMap.ContainsKey(s2.perceptionKey))
                            {
                                sensorList.Add(s2);
                                numberOfSensors++;
                                sensorsMap[s2.perceptionKey] = s2;
                            } else if (s2 != null)
                            {
                                Debug.LogWarning($"A sensor with the name {s2.perceptionKey} already " +
                                                                    $"exists on agent {name}. Only the first sensor added " +
                                                                    "with the same name is added to agent.");
                            }
                        }
                    }
                }

                Actuator a = obj.GetComponent<Actuator>();
                if (a != null)
                {
                    actuatorList.Add(a);
                    numberOfActuators++;
                }
                else
                {
                    if (obj.name == "actuators")
                    {
                        for (int j = 0; j < obj.transform.childCount; j++)
                        {
                            GameObject aobj = obj.transform.GetChild(j).gameObject;
                            Actuator a2 = aobj.GetComponent<Actuator>();
                            if (a2 != null)
                            {
                                actuatorList.Add(a2);
                                numberOfActuators++;
                            }
                        }
                    }
                }

                RewardFunc r = obj.GetComponent<RewardFunc>();
                if ( r != null)
                {
                    rewards.Add(r);
                }
                else
                {
                    if (obj.name == "rewards")
                    {
                        for (int j = 0; j < obj.transform.childCount; j++)
                        {
                            GameObject robj = obj.transform.GetChild(j).gameObject;
                            RewardFunc r2 = robj.GetComponent<RewardFunc>();
                            if (r2 != null)
                            {
                                rewards.Add(r2);
                            }
                        }
                    }
                }
            }

            if (sensorList.Count == 0) {
                Debug.LogWarning("Agent without sensors. Add at least one sensor for this agent to be able to perceive the world! GameObject: " + gameObject.name);
            }

            if (actuatorList.Count == 0) {
                Debug.LogWarning("Agent without actuators. Add at least one actuator for this agent to be able to change the world! GameObject: " + gameObject.name);
            }

            int totalNumberOfSensors = sensorList.Count;

            desc = new string[totalNumberOfSensors];
            types = new byte[totalNumberOfSensors];
            values = new string[totalNumberOfSensors];

            foreach (RewardFunc r in rewards)
            {
                r.OnSetup(this);
            }

            foreach (ISensor sensor in sensorList)
            {
                if (sensor.IsResetable())
                {
                    AddResetListener(sensor);
                }
                sensor.OnSetup(this);
            }

            foreach(Actuator a in actuatorList)
            {
                a.OnSetup(this);
            }

            metadataLoader = new ModelMetadataLoader(this);
            string metadatastr = metadataLoader.toJson();

            InitializeDataFromSensor();
            RequestCommand request = new RequestCommand(5);
            request.SetMessage(0, "__target__", bemaker.Brain.STR, "envcontrol");
            request.SetMessage(1, "max_steps", bemaker.Brain.INT, MaxStepsPerEpisode);
            request.SetMessage(2, "id", bemaker.Brain.STR, ID);
            request.SetMessage(3, "modelmetadata", bemaker.Brain.STR, metadatastr);
			request.SetMessage(4, "config", bemaker.Brain.INT, 1);
            var cmds = ControlRequestor.RequestEnvControl(this, request);
            if (cmds == null)
            {
                throw new System.Exception("bemaker2unity connection error on agent id: " + ID);
            }
    
            setupIsDone = true;
        }

        public List<Actuator> Actuators
        {
            get 
            {
                return actuatorList;
            }
        }

        public List<ISensor> Sensors 
        {
            get
            {
                return sensorList;
            }
        }

        public virtual void AddReward(float v, RewardFunc from = null){
            if (doneAtNegativeReward && v < 0) {
                Done = true;
            }

            if (doneAtPositiveReward && v > 0) {
                Done = true;
            }

            if (from != null)
            {
                if (from.causeEpisodeToEnd)
                {
                    Done = true;
                }
            }
            reward += v;
        }

        public void AddReward(float v, bool causeEpisodeToEnd){
            if (doneAtNegativeReward && v < 0) {
                Done = true;
            }

            if (doneAtPositiveReward && v > 0) {
                Done = true;
            }

            if (causeEpisodeToEnd)
            {
                Done = true;
            }

            reward += v;
        }

        public override void  ApplyAction()
        {

            if (beginOfApplyActionEvent != null)
            {
                beginOfApplyActionEvent(this);
            }

            if (MaxStepsPerEpisode > 0 && nSteps >= MaxStepsPerEpisode) {
                truncated = true;
                Done = true;
            }
            int n = actuatorList.Count;
            for (int i = 0; i < n; i++)
            {
                if (!Done)
                {
                    if (GetActionName() == actuatorList[i].actionName)
                    {
                        actuatorList[i].Act();
                    }
                }
            }
            if (!Done)
            {
                if (endOfApplyActionEvent != null)
                {
                    endOfApplyActionEvent(this);
                }
            }
        }

        public virtual void PreconditionFailListener(RewardFunc func, RewardFunc precondiction) { 
            if (Done) return;
            if (func is TouchRewardFunc) {
                if (!checkFirstTouch(func.gameObject.tag) && !((TouchRewardFunc)func).allowNext)
                {
                    this.RequestDoneFrom(func);
                }
            }
        }

        public virtual void touchListener(TouchRewardFunc origin)
        {
            //TODO add behavior here
        }

        public virtual void boxListener(BoxRewardFunc origin)
        {
            //TODO add behavior here
        }

        public override bool Alive()
        {
            return !Done;
        }

        public override void AgentReset() 
        {
            if (beforeTheResetEvent != null)
            {
                beforeTheResetEvent(this);
            }
            ResetPlayer();
            if (beginOfEpisodeEvent != null)
            {
                beginOfEpisodeEvent(this);
            }
        }

        private bool checkFirstTouch(string tag){
            if (firstTouch.ContainsKey(tag)) {
                return false;
            } else {
                firstTouch[tag] = false;
                return true;
            }
        }

        public void ResetReward()
        {
            reward = 0;
            if (beginOfStepEvent != null)
            {
                beginOfStepEvent(this);
            }
        }

        private void ResetPlayer()
        {
            nSteps = 0;
            reward = 0;
            truncated = false;
            Done = false;
            firstTouch = new Dictionary<string, bool>(); 
            
                    
            UpdateState();
            NotifyReset();
        }

        public float Reward
        {
            get
            {
                return reward;
            }
        }
        
        public void UpdateReward()
        {
            int n = rewards.Count;

            for (int i = 0; i < n; i++)
            {
                rewards[i].OnUpdate();
            }
            if (endOfStepEvent != null)
            {
                endOfStepEvent(this);
            }
        }

        public override void UpdateState()
        {

            if (beginOfUpdateStateEvent != null)
            {
                beginOfUpdateStateEvent(this);
            }

            InitializeDataFromSensor();


            if (endOfUpdateStateEvent != null)
            {
                endOfUpdateStateEvent(this);
            }
        }

        private void InitializeDataFromSensor()
        {
            int n = sensorList.Count;
            for (int i = 0; i < n; i++) {
                ISensor s = sensorList[i];
                switch(s.GetSensorType())
                {
                    case SensorType.sfloatarray:
                        var fv = s.GetFloatArrayValue();
                        if (fv == null)
                        {
                            Debug.LogWarning("Error: array of float sensor " + s.GetName() + " returning null value!");
                        }
                        SetStateAsFloatArray(i, s.GetKey(), fv);
                        break;
                    case SensorType.sfloat:
                        var fv2 = s.GetFloatValue();
                        SetStateAsFloat(i, s.GetKey(), fv2);
                        break;
                    case SensorType.sint:
                        var fv3 = s.GetIntValue();
                        SetStateAsInt(i, s.GetKey(), fv3);
                        break;
                    case SensorType.sstring:
                        var fv4 = s.GetStringValue();
                        if (fv4 == null)
                        {
                            Debug.LogWarning("Error: string sensor " + s.GetName() + " returning null value!");
                        }
                        SetStateAsString(i, s.GetKey(), fv4);
                        break;
                    case SensorType.sbool:
                        var fv5 = s.GetBoolValue();
                        SetStateAsBool(i, s.GetKey(), fv5);
                        break;
                    case SensorType.sbytearray:
                        var fv6 = s.GetByteArrayValue();
                        if (fv6 == null)
                        {
                            Debug.LogWarning("Error: byte array sensor " + s.GetName() + " returning null value!");
                        }
                        SetStateAsByteArray(i, s.GetKey(), fv6);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
