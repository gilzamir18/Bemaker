from .agents import BasicAgent
from .utils import stepfv
from threading import Thread
from .bemaker2unity import create_server
from .workers import BMWorker
import time

def startasdaemon(ids, controllers_classes=None, server_IP="127.0.0.1", server_port=8080, buffer_size=8192, waitfor=0.1):
    agents = [BasicAgent] * len(ids)
    queues = []
    t = Thread(target=lambda:create_server(agents, ids, server_IP, server_port, buffer_size, waitfor))
    t.daemon = True
    t.start()
    time.sleep(waitfor)
    print("starting bemaker2unity...", end='\r')
    while (BMWorker.count_agents() < len(ids)):
        time.sleep(waitfor)
    agents = BMWorker.get_agents()
    for i in range(len(ids)):
        agents[i].controller = controllers_classes[i]()
        agents[i].controller.agent = agents[i]
    print("bemaker2unity started...")
    return [agent.controller for agent in agents]

