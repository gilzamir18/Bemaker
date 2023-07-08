using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bemaker;

namespace bemaker
{
    public class LinearRaycastingSensorWrapper : Sensor
    {
    
        public SharedLinearRaycasting sensor;
        
        public override void OnSetup(Agent agent)
        {
            sensor.initialize();
            type = sensor.type;
            shape = sensor.shape;
        }

        public override float[] GetFloatArrayValue()
        {
            return sensor.History;    
        }
    }
}
