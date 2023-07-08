from gym.envs.registration import register
from BMEnv.envs.env import GenericEnvironment

register (
    id='bemakerEnv-v0',
    entry_point='bemakerEnv.envs:GenericEnvironment',
)
