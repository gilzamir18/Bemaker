using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker
{
    public class BrainManager : MonoBehaviour
    {
        public RemoteBrain[] brainList;
        public bool autoConfiguration = true;
        public int startPort = 8080;
        public int stepPort = 1;
       
        public string host = "127.0.0.1";

        public bool acceptRemoteConfiguration = true;

        private bool runFirstTime = true;

        // Start is called before the first frame update
        void Awake()
        {
            if (runFirstTime && acceptRemoteConfiguration) {
                runFirstTime = false;
                string[] args = System.Environment.GetCommandLineArgs ();
                int i = 0;
                while (i < args.Length){
                    switch (args[i]) {
                        case "--bemaker_port":
                            startPort = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--bemaker_stepport":
                            stepPort = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--bemaker_timescale":
                            Time.timeScale = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            i += 2;
                            break;
                        case "--bemaker_host":
                            host = args[i+1];
                            i += 2;
                            break;
                        case "--bemaker_targetframerate":
                            Application.targetFrameRate = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--bemaker_vsynccount":
                            QualitySettings.vSyncCount = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        default:
                            i+=1;
                            break;
                    }
                }
            }

            if (autoConfiguration) {
                Configure();
            }
        }

        public void Configure() {
            int port = startPort;
            int step = stepPort;

            int p = 0;
            foreach (RemoteBrain rb in brainList)
            {
                rb.Host = host;
                rb.Port = startPort + p;
                p += step;
            }
        }
    }
}
