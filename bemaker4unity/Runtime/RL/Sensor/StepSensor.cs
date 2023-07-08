using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker 
{
    public class StepSensor : AbstractSensor
    {
        public StepSensor()
        {
            SetKey("steps");
            SetIsResetable(true);
            SetIsActive(true);
            SetIsInput(false);
            SetStackedObservations(1);
            SetSensorType(SensorType.sint);
            SetShape(new int[1]{1});
        }

        public override int GetIntValue()
        {
            return GetAgent().NSteps;    
        }
    }
}