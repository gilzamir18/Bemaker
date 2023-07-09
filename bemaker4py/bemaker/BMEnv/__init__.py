from gym.envs.registration import register
from BMEnv.envs.env import GenericEnvironment

register (
    id='BMEnv-v0',
    entry_point='BMEnv.envs:GenericEnvironment',
)
