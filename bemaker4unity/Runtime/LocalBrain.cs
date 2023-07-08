using UnityEngine;
namespace bemaker
{
    public class LocalBrain : Brain
    {
        private Controller Controller {get;}

        public LocalBrain(Controller ctrl)
        {
            this.Controller = ctrl;
        }
        
        public override void Setup(Agent agent)
        {
            this.agent = agent;
            this.Controller.Setup(agent);
        }

        public string SendMessage(string[] desc, byte[] tipo, string[] valor)
        {
            Controller.ReceiveState(desc, tipo, valor);
            return Controller.GetAction();
        }
    }
}
