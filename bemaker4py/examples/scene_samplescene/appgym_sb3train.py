import bemaker
from bemaker.controllers import BasicGymController
import bemakerEnv
import gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy

env = gym.make("bemakerEnv-v0")

model = SAC(MultiInputPolicy, env, verbose=1, tensorboard_log="SAC")
print("Training....")
model.learn(total_timesteps=20000, log_interval=4,  tb_log_name='SAC')
model.save("sac_bemaker")
print("Trained...")
del model # remove to demonstrate saving and loading
print("Train finished!!!")

