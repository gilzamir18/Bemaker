from bemaker.onnxutils import sac_export_to
from bemaker.controllers import BasicGymController
import BMEnv
import gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy

env = gym.make("BMEnv-v0")

print('''
bemaker Client Controller
=======================
This example controll a movable character in game (unity or godot).
''')


model = SAC.load("sac_bemaker")

sac_export_to("sac_bemaker", metadatamodel=env.controller.metadataobj)

obs = env.reset()

reward_sum = 0
while True:
    action, _states = model.predict(obs, deterministic=True)
    obs, reward, done, info = env.step(action)
    reward_sum += reward
    env.render()
    if done:
      print("Testing Reward: ", reward_sum)
      reward_sum = 0
      obs = env.reset()


