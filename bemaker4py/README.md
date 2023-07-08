# BeMaker

BeMaker  is a protocol and an Application Programming Interface (API) that allow control Unity Game Objects designed with BeMaker's agent Abstraction Framework (BAFF). A Unity game object is an agents if associated with components of the type *BasicAgent*. 

The environment and agent creation pipeline is:

* First create a project in Unity.
* Install BeMaker in the created project and start modeling the agent's environment using the BAAF.
* Connect the environment and the agent with python scripts to, for example, train the neural networks that control the agent.

To control your agent through the Python language, you need to install BeMaker4Py. To do this, enter the directory [bemaker4py](/bemaker) and run the command:

    pip install -e .

After installing bemaker4py, enter the [example](/examples/scene_samplescene) directory and run the command:

    python appgym_sbtest.py

Then run the SampleScene scene in Unity and run the game. This script will execute an already trained neural network that will control the capsule with arrow that represents the agent. The agent's objective is to reach the red cube without falling off the platform (green plane in figure bellow).

[See here for full documentation on bemaker](./doc/) 

![IMAGEM](/doc/img/bemakerge_samplescene.png)


In the next image, we show the tensorboard training logs generated when training the SampleScene scene agent. To do this, we use the stable-baselines3 project.

![IMAGE](/doc/img/sample_scene.png)


A full example of the game Pong using BeMaker4Unity:

![](https://public.by.files.1drv.com/y4mE4z_1xivtrP8mdLnopcJSoad1Vs70jnclJtfQrK5GTBCXjnVVfavVBvTgizC0ytDV4acsbPokboN_tnW8iIppCDHZs1OP1ZJ0_NRh5f2T5DTDSrXSIauYIhPOalXStNutHBQ3StqPYfHcseiwq6kqFQasiuaDN_ozHHnkRkIPDOo3Wn2JTat0XamQo0JxU7jlxYSiUzP4TECDSZDGXWh2KHbKBYOtlXXLhjKQNE5ziw)


The training logs show that:

![Gráfico](https://by3301files.storage.live.com/y4mBFID5H01I_Z5o5VdQ_dAYnAP-eh_MsDKZpWCywqhqx-BMvzHbtD23roz99QqsdmE5BncH0c59wy6OEkVyE7TsblGg-In_CY29MQ81MRzXmrIOwO2Q2XhSy9kcHFSLGneVhOlDB7KYvCsKF0nXYTkWbmihxz_1IeKyBR7qlk_lAFA6dFtbISekGqKNlmFkC110-E6CXpkIqsMYRLzoJKbRjjnpPcziXRnpTU6WkJu7c0?encodeFailures=1&width=384&height=311). 

Pong project is [here](https://github.com/gilzamir18/BMPongDemo).


# Game Engine Support

Bemaker supports scenes designed in Unity game engine. The BeMaker Agent Abstraction Framework provides a set of abstractions for creating agents, with sensors and actuators being the main abstractions. Agents utilize sensors to gather information about the environment and themselves, sending this information to a decision-making model such as a neural network. For instance, a set of sensors can capture the agent's position and orientation, while another set can capture the position of a target. Actuators, on the other hand, represent actions that modify both the environment and the agent itself. All the information is concatenated and sent to an agent controller, which can utilize a neural network to make decisions on actions to perform. The available actions depend on the type of actuator present in the agent. For example, a motion actuator can receive parameters such as forward motion force and torque, which are then applied to the agent's center of mass in the physical object representation.

There are separate implementations of BAAF for Unity. Each implementation has its own unique characteristics and limitations. Consequently, BeMaker acts as the component that connects a external controller (usually python) to  Unity.


# Prerequisites
BeMaker have been tested on three different operating systems: Windows 11, Windows 10, and Ubuntu 20.04. In all systems, it is necessary to have a Python environment installed, preferably a version higher than 3.7. Additionally, the current code has been designed to run on Gymnasio and Gym with a version higher than 0.28.1 or above.

If you are using the stable-baselines3 framework for deep reinforcement learning, bemaker only supports the alpha version, which should be installed according to the issue [#1327](https://github.com/DLR-RM/stable-baselines3/pull/1327):

```shell
pip install "sb3_contrib>=2.0.0a1" --upgrade    
pip install "stable_baselines3>=2.0.0a1" --upgrade
```

