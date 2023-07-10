# BeMaker

BeMaker  is a protocol and an Application Programming Interface (API) that allow control Unity Game Objects designed with BeMaker's agent Abstraction Framework (BAFF). A Unity game object is an agents if associated with components of the type *BasicAgent*. 

The environment and agent creation pipeline is:

* First create a project in Unity.
* Install BeMaker in the created project and start modeling the agent's environment using the BAAF.
* Connect the environment and the agent with python scripts to, for example, train the neural networks that control the agent.

To control your agent through the Python language, you need to install BeMaker4Py. To do this, enter the directory [bemaker4py](/bemaker) and run the command:

    pip install -e .

# Game Engine Support

Bemaker supports scenes designed in Unity game engine. The BeMaker Agent Abstraction Framework provides a set of abstractions for creating agents, with sensors and actuators being the main abstractions. Agents utilize sensors to gather information about the environment and themselves, sending this information to a decision-making model such as a neural network. For instance, a set of sensors can capture the agent's position and orientation, while another set can capture the position of a target. Actuators, on the other hand, represent actions that modify both the environment and the agent itself. All the information is concatenated and sent to an agent controller, which can utilize a neural network to make decisions on actions to perform. The available actions depend on the type of actuator present in the agent. For example, a motion actuator can receive parameters such as forward motion force and torque, which are then applied to the agent's center of mass in the physical object representation.

There are separate implementations of BAAF for Unity. Each implementation has its own unique characteristics and limitations. Consequently, BeMaker acts as the component that connects a external controller (usually python) to  Unity.


# Prerequisites
BeMaker have been tested on three different operating systems: Windows 11, Windows 10, and Ubuntu 20.04. In all systems, it is necessary to have a Python environment installed, preferably a version higher than 3.7. Additionally, the current code has been designed to run on Gymnasio and Gym with a version higher than 0.28.1 or above.

If you are using the stable-baselines3 framework for deep reinforcement learning, bemaker only supports the alpha version, which should be installed according to the issue [#1327](https://github.com/DLR-RM/stable-baselines3/pull/1327):

```shell
pip install "sb3_contrib>=2.0.0a1" --upgrade    
```

or

```shell
pip install "stable_baselines3>=2.0.0a1" --upgrade
```
.

It is essential to upgrade to the latest version of the setuptools package:

```shell
pip install setuptools --upgrade
```
