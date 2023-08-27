from .agents import BasicAgent
from .utils import stepfv
from threading import Thread
from .bemaker2unity import create_server
from .workers import BMWorker
import time
from .utils import get_float_from

def startasdaemon(ids, controllers_classes=None, config=None):
    if config is None:
        config = {}
    agents = [BasicAgent] * len(ids)
    queues = []
    t = Thread(target=lambda:create_server(agents, ids, config))
    t.daemon = True
    t.start()
    waittime = get_float_from(config, 'waittime_on_startup', 0.0)
    time.sleep( waittime )
    print("starting bemaker2unity...", end='\r')
    while (BMWorker.count_agents() < len(ids)):
        time.sleep( waittime )
    agents = BMWorker.get_agents()
    for i in range(len(ids)):
        agents[i].controller = controllers_classes[i]()
        agents[i].controller.agent = agents[i]
    print("bemaker2unity started...")
    return [agent.controller for agent in agents]

