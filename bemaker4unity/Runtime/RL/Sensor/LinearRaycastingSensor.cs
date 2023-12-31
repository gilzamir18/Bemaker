using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker
{
    public class LinearRaycastingSensor : Sensor
    {
        [Tooltip("'noObjectCode' is the code produced when a ray does not hit any object.")]
        public int noObjectCode;
        [Tooltip("The 'eye' property refers to the position and forward direction of the casting rays.")]
        public GameObject eye;
        [Tooltip("The 'fieldOfView' property represents the angle between the two extreme rays that are cast from the position specified by the 'eye' property. This angle determines the extent of the field of view for the associated camera or sensor, and indicates the angular range within which objects can be detected or observed.")]
        public float fieldOfView = 180.0f;
        [Tooltip("A float value that determines the vertical shift of the casting rays.")]
        public float verticalShift = 0;
        [Tooltip("A float value that determines the horizontal shift of the casting rays.")]
        public float horizontalShift = 0;
        [Tooltip("A float value that determines the maximum distance that the casting rays can travel before being stopped.")]
        public float maxDistance = 500f;
        [Tooltip("A boolean value that indicates whether or not the sensor should return depth information.")]
        public bool returnDepthMatrix = false;
        [Tooltip("An integer value that determines how many rays will be cast from the sensor.")]
        public int numberOfRays = 5;
        [Tooltip("A boolean value that indicates whether or not the sensor should automatically map detected objects to integer codes based on their tags.")]
        public bool automaticTagMapping = false;
        [Tooltip("The list of tags relevant to the agent’s decision.")]
        public List<string> tags;
        [Tooltip("An integer value that is used to calculate the integer code for objects detected by the sensor when automaticTagMapping is enabled.")]
        public int tagCodeGap = 10;
        public int tagCodeShift = 0;
        [Tooltip("An array of ObjectMapping structs that define mappings between object tags and integer codes.")]
        public  ObjectMapping[] objectMapping;
        [Tooltip(" A boolean value that indicates whether or not debug information should be displayed while the sensor is active.")]
        public bool debugMode = false;

        private HistoryStack<float> stack;
        private int depth = 1;
        private float angleStep;
        private Dictionary<string, int> mapping;
        private float halfFOV = 90;
        
        public override void OnSetup(Agent agent)
        {
            depth = 1;
            type = SensorType.sfloatarray;
            mapping = new Dictionary<string, int>();
            if (returnDepthMatrix)
            {
                depth = 2;
            }

            shape = new int[1]{numberOfRays * depth};            
            stack = new HistoryStack<float>(stackedObservations * shape[0]);

            if (automaticTagMapping)
            {
                int i = 0;
                foreach (string tag in tags)
                {
                        mapping[tag] = tagCodeShift + i * tagCodeGap;
                        i++;
                }
            }
            else
            {
                foreach(ObjectMapping obj in objectMapping)
                {
                    mapping[obj.tag] = obj.code;
                }
            }

            agent.AddResetListener(this);
        }

        public override void OnReset(Agent agent) 
        {
            stack = new HistoryStack<float>(stackedObservations * shape[0]);
            mapping = new Dictionary<string, int>();
            foreach(ObjectMapping obj in objectMapping)
            {
                mapping[obj.tag] = obj.code;
            }
            RayCasting();
        }

        void OnDrawGizmos()
        {

            if (numberOfRays > 1)
            {
                angleStep = fieldOfView/(numberOfRays - 1);
            }
            else
            {
                angleStep = 0;
            }

            halfFOV = fieldOfView/2;
            if (shape == null || stack == null)
            {
                type = SensorType.sfloatarray;
                shape = new int[1]{numberOfRays};
                stack = new HistoryStack<float>(stackedObservations * numberOfRays);
                mapping = new Dictionary<string, int>();
                foreach(ObjectMapping obj in objectMapping)
                {
                    mapping[obj.tag] = obj.code;
                }
            }
            RayCasting();
        }


        void RayCasting()
        {
            Vector3 pos = eye.transform.position;
            Vector3 fwd = eye.transform.forward;

            if (numberOfRays > 1)
            {
                angleStep = fieldOfView/(numberOfRays - 1);
            }
            else
            {
                angleStep = 0;
            }

            halfFOV = fieldOfView/2;
            
            for (uint i = 0; i < numberOfRays; i++)
            {
                float angle =  0;
                
                if (numberOfRays > 1)
                {
                    angle = i * angleStep - halfFOV;
                }
                
                Vector3 direction = Quaternion.Euler(verticalShift, angle + horizontalShift, 0) * fwd;
                RaycastHit hitinfo;
                if (Physics.Raycast(pos, direction, out hitinfo, maxDistance))
                {
                    GameObject gobj = hitinfo.collider.gameObject;
                    string objtag = gobj.tag;
                    if (mapping.ContainsKey(objtag))
                    {
                        int code = mapping[objtag];
                        stack.Push(code);
                        if (returnDepthMatrix)
                        {
                            stack.Push(hitinfo.distance);
                        }
                    }
                    else 
                    {
                        stack.Push(noObjectCode);
                        if (returnDepthMatrix)
                        {
                            stack.Push(hitinfo.distance);
                        }
                    }
                    if (debugMode)
                    {
                        Debug.DrawRay(pos, direction * hitinfo.distance, Color.red);
                    }
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.DrawRay(pos, direction * hitinfo.distance, Color.yellow);
                    }
                    stack.Push(noObjectCode);
                    if (returnDepthMatrix)
                    {
                        stack.Push(-1);
                    }
                }
            }
        }

        public override float[] GetFloatArrayValue()
        {
            RayCasting();
            return stack.Values;
        }
    }
}
