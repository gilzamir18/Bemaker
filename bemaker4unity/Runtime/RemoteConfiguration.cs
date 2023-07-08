using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bemaker
{
    public class RemoteConfiguration : MonoBehaviour
    {
        ///<summary>If true, the remote brain will be 
        ///managed manually. Thus, in this case, command 
        ///line arguments do not alter the properties of 
        ///the remote brain.</summary>
        public bool managed = false;
        ///<summary>The IP of the bemaker2unity training server.</summary>
        public string host = "127.0.0.1";
        ///<summary>The server port of the bemaker2unity training server.</summary>
        public int port = 8080;
        public int receiveTimeout = 2000;
        public int receiveBufferSize = 8192;
        public int sendBufferSize = 8192;
    }
}
