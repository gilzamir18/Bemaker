using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker
{
    public class AgentTruncatedSensor : AbstractSensor
    {
        public AgentTruncatedSensor()
        {
            SetKey("truncated");
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
            return GetAgent().Truncated;
        }
    }
}
