using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker 
{
    public class IDSensor : AbstractSensor
    {

        public IDSensor()
        {
            SetKey("id");
            SetIsResetable(true);
            SetIsActive(true);
            SetIsInput(false);
            SetStackedObservations(1);
            SetSensorType(SensorType.sstring);
            SetShape(new int[1]{1});
         
        }

        public override void OnSetup(Agent agent)
        {
            SetAgent((BasicAgent)agent);
        }

        public override string GetStringValue()
        {
            return GetAgent().ID;
        }
    }
}
