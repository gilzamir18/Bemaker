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
        #print("Begin Reseting....")
        self.initialState = None
        while self.initialState is None:
            self.agent.request_newepisode(self.resetid, args)
            time.sleep(self.waitforinitialstate)
        self.resetid += 1
        self.done = False
        info = self.initialState
        self.initialState = None
        #print("Reseting Ending... ", info)
        self.paused = False
        return self.reset_behavior(info)

    def handleNewEpisode(self, info):
        pass

    def handleEndOfEpisode(self, info):
        pass

    def _endOfEpisode(self, info):
        self.handleEndOfEpisode(self.lastinfo)
        self.lastinfo = info
        self.nextstate = info
        self.initialState = None
        self.done = True
    
    def stop(self):
        self.agent.request_stop()

    def pause(self):
        self.agent.request_pause()
    
    def resume(self):
        self.agent.request_resume()

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
        
    def request_step(self, action, gymformat=True, truncated=False):
        """
        This method change environment state under action named 'action'.
        Never override this method. If you want change step behavior,
        implements step_behavior method.
        """
        self.step_behavior(action)
        self.newaction = True
        self.nextstate = None
        while ( (not self.done) and 
                (self.nextstate is None) and 
                (not self.agent.paused) and (not self.agent.waitingCommand) ):
            time.sleep(self.waitfornextstate)
        
        #check if environment is waiting a command
        if self.agent.waitingCommand:
            self.lastinfo['envmode'] = True
            self.lastinfo['done'] = True
            self.done = True
            self.lastinfo['reward'] = 0
            if gymformat:
                return self.lastinfo, self.lastinfo['reward'], True, truncated, {}
            else:
                return self.lastinfo

        if self.agent.paused:
            #resume if current action is resume and paused.
            if self.actionName == '__resume__':
                self.resume()
                self.lastinfo['paused'] = False
            else:
                self.lastinfo['paused'] = True
            #return current last state
            if gymformat:
                return self.lastinfo, self.lastinfo['reward'], True, truncated, {}
            else:
                return self.lastinfo

        state = self.nextstate

        if not state:
            state = self.agent.lastinfo
        else:
            self.nextstate = None

        if gymformat:
            return state, state['reward'], state['done'], truncated, {} 
        else:
            return state

    def getaction(self, info):
        """
        Return a new action based in new info 'info'.
        """
        if self.newaction:
            self.newaction = False
            self.lastinfo = info.copy()
            if (self.actionName == '__pause__') and not self.agent.paused:
                self.actionName = self.defaultAction
                return self.agent._pause() #environment is in action mode
            elif (self.actionName == '__resume__') and self.agent.paused:
                self.actionName = self.defaultAction
                return self.agent._resume() #environment is in envcontrolmode 
            elif (self.actionName == '__stop__'):
                self.actionName = self.defaultAction
                return self.agent._stop()
            elif (self.actionName == "__restart__"):
                self.actionName = self.defaultAction
                r = self.agent._restart()
                self.resetid += 1
                return r
            else:
                action = steps(self.actionName,  self.actionArgs, self.fields)
                self.actionName = self.defaultAction
                self.actionArgs = self.defaultActionArgs.copy()
                self.agent.hasNextState = True
                return action
        else:
            return step("__waitnewaction__")

class BasicAgent:
    rl_env_control = {
        'max_steps': 1000,
        'agent_id': 0
    }

    def __init__(self):
        self.max_step = 0
        self.id = 0
        self.steps = 0
        self.createANewEpisode = False
        self.newStopCommand = False
        self.newPauseCommand = False
        self.newResumeCommand = False
        self.newRestartCommand = False
        self.controller = BasicController()
        self.newInfo = True
        self.lastinfo = None
        self.stopped = False
        self.paused = False
        self.hasNextState = False
        self.waitingCommand = True
        self.resetargs = None
        self.envcontrol = False
        self.waitingnewepisode = True
        self.envresetid = 0

    def request_stop(self):
        self.newStopCommand = True
    
    def request_pause(self):
        if not self.stopped:
            self.newPauseCommand = True

    def request_newepisode(self, resetid, cmds=None):
        self.resetargs = cmds
        self.resetid = resetid
        self.createANewEpisode = True
    
    def request_envcontrol(self, cmds=None):
        self.envcontrolargs = cmds
        self.envcontrol = True

    def request_resume(self):
        if self.paused and not self.stopped:
            self.newResumeCommand = True

    def _stop(self): 
        """
        Stop agent simulation in Unity.
        """
        self.stopped = True
        self.paused = False
        self.hasNextState = False
        return step("__stop__")

    def _envcontrol(self): 
        """
        Stop agent simulation in Unity.
        """
        self.stopped = True
        self.paused = False
        self.hasNextState = False
        return step("__envcontrol__")

    def _restart(self):
        """
        Restart agent simulation in Unity.
        """
        self.stopped = False
        self.paused = False
        self.newInfo = True
        self.hasNextState = False
        if self.resetargs is None:
            return step("__restart__", [self.resetid])
        else:
            args = self.resetargs
            self.resetargs = None
            return steps("__restart__", [self.resetid], args)
    
    def _restartfromenv(self):
        """
        Restart agent simulation in Unity.
        """
        self.stopped = False
        self.paused = False
        self.newInfo = True
        self.hasNextState = False
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

    def _pause(self):
        """
        Pause agent simulation in Unity.
        """
        self.paused = True
        return step("__pause__")
    
    def _resume(self):
        """
        Resume agent simulation in Unity.
        """
        self.paused = False
        return step("__resume__")

    def _step(self, info):
        assert info['id'] == self.id, "Error: inconsistent agent identification!"       
        return self.__get_controller().getaction(info)

    def act(self, info):
        self.waitingCommand = False
        if self.hasNextState:
            self.__get_controller().nextstate = info.copy()
            self.hasNextState = False
        
        if self.newInfo:
            self.newInfo = False
            ctrl = self.__get_controller()
            ctrl.initialState = info
            ctrl.handleNewEpisode(info)
            
        if self.createANewEpisode:
            #print(f"==================> CREATENEWEPSCMD {self.id}")
            self.waitingnewepisode = False
            ctrl = self.__get_controller()
            self.createANewEpisode = False
            self.newInfo = True
            return self._restart()
        
        if self.newPauseCommand:
            #print(f"==================> NEWPAUSECMD {self.id}")
            self.newPauseCommand = False
            self.lastinfo = info
            return self._pause()

        if  self.newStopCommand:
            #print(f"==================> NEWSTOPCMD {self.id}")
            self.newStopCommand = False
            self.waitingnewepisode = True
            return self._stop()

        if self.envcontrol:
            ctrl = self.__get_controller()
            self.newInfo = False
            self.paused = False
            self.stopped = False
            self.envcontrol = False
            r = self._envcontrol()
            return r

        if info['done'] and not self.waitingnewepisode:
            self.waitingnewepisode = True
            self.lastinfo = info
            self.__get_controller()._endOfEpisode(info)
            return self._stop()
        self.steps = self.steps + 1
        return self._step(info)

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
            #print("waiting command from unity...")
            self.waitingCommand = True
            if self.createANewEpisode:
                self.waitingnewepisode = False
                self.createANewEpisode = False
                self.newInfo = True
                self.waitingCommand = False
                return self._restartfromenv()
            if self.newResumeCommand:
                self.waitingCommand = False
                return self._resume()
            elif self.newRestartCommand:
                self.waitingnewepisode = False
                self.newRestartCommand = False
                self.waitingCommand = False
                self.newInfo = True
                self.paused = False
                return self._restartfromenv()
            elif self.newStopCommand:
                self.newStopCommand = False
                self.waitingnewepisode = True
                return self._stop()
        return stepfv('__noop__', [0])

    def __get_controller(self):
        if (self.controller):
            return self.controller
        else:
            print("ERROR(agents.py): agent without controller!")
            sys.exit(0)
