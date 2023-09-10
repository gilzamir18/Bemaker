using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker
{
    public class ActuatorHub :  Actuator
    {
        public List<Actuator> actuators;
        public override void Act()
        {
            foreach (var actuator in actuators)
            {
                actuator.Act();
            }
        }

        public override void OnSetup(Agent agent)
        {
            foreach (var actuator in actuators)
            {
                actuator.OnSetup(agent);
            }
        }

        public override void OnReset(Agent agent)
        {
            foreach (var actuator in actuators)
            {
                actuator.OnReset(agent);
            }
        }
    }
}
