using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bemaker;

namespace bemaker
{
    public class DoneSensor : AbstractSensor
    {
        public DoneSensor()
        {
            SetKey("done");
            SetIsResetable(true);
            SetIsActive(true);
            SetIsInput(false);
            SetStackedObservations(1);
            SetSensorType(SensorType.sbool);
            SetShape(new int[0]);
        }

        public override void OnSetup(Agent agent)
        {
            SetAgent((BasicAgent) agent);
        }

        public override bool GetBoolValue()
        {
            return GetAgent().Done;    
        }
    }
}
