using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker
{
    public class AgentRewardSensor : AbstractSensor
    {
        private float rewardScale;

        public AgentRewardSensor()
        {
            SetKey("reward");
            SetRewardScale(1.0f);
            SetIsResetable(true);
            SetIsActive(true);
            SetIsInput(false);
            SetStackedObservations(1);
            SetSensorType(SensorType.sfloat);
            SetShape(new int[1]{1});
        }

        public override void OnSetup(Agent agent)
        {
            SetAgent( (BasicAgent) agent );
        }

        public void SetRewardScale(float rs)
        {
            this.rewardScale = rs;
        }

        public override float GetFloatValue()
        {
            return GetAgent().Reward * rewardScale;
        }
    }
}
