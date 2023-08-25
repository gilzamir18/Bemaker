from .utils import step, stepfv, steps
import random
import time
import sys

class BasicController:    
    def __init__(self):
        self.initialState = None
        self.actionName = "__waitnewaction__"
        self.actionArgs = [0, 0, 0, 0]
        self.defaultAction = "__waitnewaction__"
        self.defaultActionArgs = [0, 0, 0, 0]
        self.lastinfo = None
        self.waitfornextstate = 0.001
        self.waitforinitialstate = 0.01
        self.done = False
        self.agent = None
        self.id = 0
        self.max_steps = 0
        self.newaction = False
        self.nextstate = None
        self.fields = None
        self.resetid = 0

    def reset_behavior(self, info):
        self.actionArgs = [0, 0, 0, 0]
        return info

    def close(self):
        pass #release resources here

    def request_reset(self, args=None):
        self.initialState = None
        self.agent.reset = True
        while self.initialState is None:
            self.agent.request_newepisode(self.resetid, args)
            time.sleep(self.waitforinitialstate)
        info = self.initialState
        self.initialState = None
        self.resetid += 1
        self.done = False
        self.paused = False
        return self.reset_behavior(info)

    def handleNewEpisode(self, info):
        pass

    def handleEndOfEpisode(self, info):
        pass
    
    def stop(self):
        self.agent.request_stop()

    def pause(self):
        self.agent.request_pause()
    
    def resume(self):
        self.agent.request_resume()
    
    def transform_state(self, info):
        return info

    def handleConfiguration(self, id, max_step, metadatamodel):
        self.id = id
        self.max_steps = max_step
        self.metadatamodel = metadatamodel

    def step_behavior(self, action):
        """
        Override this method to change step behavior. Never change
        step method directly.
        """
        self.actionName = "move"
        self.actionArgs = [random.choice([0, 500]), 0, random.choice([0, 500])]
        
    def request_step(self, action):
        """
        This method change environment state under action named 'action'.
        Never override this method. If you want change step behavior,
        implements step_behavior method.
        """
        self.step_behavior(action)
        self.nextstate = None
        self.agent.newaction = True
        while ( self.nextstate is None and self.agent.check_inepisode(self.agent.info)):
            time.sleep(self.waitfornextstate)
        if self.nextstate is None:
            self.nextstate = self.agent.info
        state = self.nextstate
        self.nextstate = None
        return state

    def restoreDefaultAction(self):
        self.actionName = "__waitnewaction__"
        self.actionArgs = [0]
        self.fields = None


class BasicAgent:
    rl_env_control = {
        'max_steps': 1000,
        'agent_id': 0
    }

    def __init__(self):
        self.max_step = 0
        self.id = 0
        self.steps = 0
        self.controller = BasicController()
        self.newaction = False
        self.resume = False
        self.reset = False
        self.resetargs = None
        self.resetid = 0
        self.envresetid = 0
        self.info = None
        self.stop = False
      
    def check_outofepisode(self, info):
        return info['__ctrl_stopped__'] or info['done'] or info['__ctrl_paused__']

    def check_inepisode(self, info):
        return not self.check_outofepisode(info)
        
    def check_firststep(self, info):
        return info['steps'] == 0
 
    def __step(self, actionName, actionArgs, fields, info):
        assert info['id'] == self.id, "Error: inconsistent agent identification!"
        if fields is None:
            return step(actionName, actionArgs)
        else:
            return steps(actionName, actionArgs, fields)


    def __stop(self): 
        """
        Stop agent simulation in Unity.
        """
        return step("__stop__")

    def __resume(self):
        """
        Resume agent simulation in Unity.
        """
        return step("__resume__")

    def __reset(self):
        """
        Restart agent simulation in Unity.
        """
        self.reset = False
        if self.resetargs is None:
            return step("__restart__", [self.resetid])
        else:
            args = self.resetargs
            self.resetargs = None
            return steps("__restart__", [self.resetid], args)
        
    def __pause(self):
        """
        Pause agent simulation in Unity.
        """
        return step("__pause__")
    
    def __resume(self):
        """
        Resume agent simulation in Unity.
        """
        return step("__resume__")

    def __resetfromenv(self):
        """
        Restart agent simulation in Unity.
        """
        if self.resetargs is None:
            cid = self.envresetid
            self.envresetid += 1
            return step("__restart__", [cid])
        else:
            args = self.resetargs
            self.resetargs = None
            cid = self.envresetid
            self.envresetid += 1
            return steps("__restart__", [cid], args)

    def request_newepisode(self, resetid, cmds=None):
        self.resetargs = cmds
        self.resetid = resetid
        self.reset = True

    def act(self, info):
        actionName = self.__get_controller().actionName
        actionArgs = self.__get_controller().actionArgs
        fields = self.__get_controller().fields
        self.__get_controller().restoreDefaultAction()
        self.info = info

        if info['done'] and not info['__ctrl_stopped__']:
            self.__get_controller().nextstate = info
            self.__get_controller().handleEndOfEpisode(info)
            return self.__stop()

        if actionName == "__restart__" or self.reset:
            self.reset = False
            return self.__reset()
        if actionName == "__stop__" or self.stop:
            self.stop = False
            return self.__stop()
        if actionName == "__resume__" and info['__ctrl_paused__']:
            self.resume = False
            return self.__resume()

        if self.check_inepisode(info):
            if self.check_firststep(info) and self.__get_controller().initialState  is None:
                self.__get_controller().initialState = info
                self.__get_controller().handleNewEpisode(info)
            if self.newaction:
                self.__get_controller().nextstate = info
                self.newaction = False
                self.steps += 1
                return self.__step(actionName, actionArgs, fields, info)
            else:
                return step("__waitnewaction__", [0])
        return step("__waitnewaction__", [0])

    def handleEnvCtrl(self, a):
        if 'config' in a:
            self.max_steps = a['max_steps']
            self.id = a['id']
            self.modelmetadata = a['modelmetadata']
            control = []
            control.append(stepfv('max_steps', [self.max_steps]))
            control.append(stepfv('id', [self.id]))
            self.__get_controller().handleConfiguration(self.id, self.max_step, self.modelmetadata)
            return ("@".join(control))
        if 'wait_command' in a:
            if self.reset:
                return self.__resetfromenv()
        return stepfv('__noop__', [0])

    def __get_controller(self):
        if (self.controller):
            return self.controller
        else:
            print("ERROR(agents.py): agent without controller!")
            sys.exit(0)
