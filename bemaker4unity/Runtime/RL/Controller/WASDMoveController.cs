using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bemaker;

namespace  bemaker
{
    public class WASDMoveController : Controller
    {
        public string actuatorName = "move";
        public float speed = 10.0f;
        public bool startOnSetup = true;
        private float reward_sum = 0;

        private bool waitRestart = false;

        override public void OnSetup()
        {
            waitRestart = false;
        }

        override public string GetAction()
        {
            if (waitRestart)
            {
                return bemaker.Utils.ParseAction("__restart__");
            }

            float[] actionValue = new float[4];
            string actionName = actuatorName;

            if (Input.GetKey(KeyCode.W))
            {
                actionValue[0] = speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                actionValue[0] = -speed;
            }

            if (Input.GetKey(KeyCode.U))
            {
                actionValue[2] = speed;
            }

            if (Input.GetKey(KeyCode.J))
            {
                actionValue[2] = -speed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                actionValue[1] = -speed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                actionValue[1] = speed;
            }

            if (Input.GetKey(KeyCode.R))
            {
                actionName = "__restart__";
            }

            if (actionName != "__restart__")
            {
                //Debug.Log(ai4u.Utils.ParseAction(actionName, actionValue));
                return bemaker.Utils.ParseAction(actionName, actionValue);
            } else
            {
                return bemaker.Utils.ParseAction("__restart__");
            }
        }

        override public void NewStateEvent()
        {
            int n = GetStateSize();
            bool checkWaitRestart = false;
            for (int i = 0; i < n; i++)
            {
                if (GetStateName(i) == "wait_command")
                {
                    if (GetStateAsString(i).Contains("restart"))
                    {
                        checkWaitRestart = true;
                        waitRestart = true;
                    }
                    else
                    {
                        waitRestart = false;
                    }
                }
                if (GetStateName(i) == "reward" || GetStateName(i) == "score")
                {
                    float r = GetStateAsFloat(i);
                    reward_sum += r;
                }
                if (GetStateName(i) == "done" && GetStateAsFloat() > 0)
                {
                    Debug.Log("Reward Episode: " + reward_sum);
                    reward_sum = 0;
                }
            }
            if (!checkWaitRestart)
            {
                waitRestart = false;
            }
        }
    }
}
