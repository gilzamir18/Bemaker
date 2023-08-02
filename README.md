
# Bemaker for Unity

BeMaker provides an agent abstraction for modeling Non-Player Characters (NPCs). Agent Abstraction (AA) is an Artificial Intelligence (AI) concept that defines an agent living in an environment and interacting with it through sensors and actuators. As such, NPC specification is a type of agent specification. An agent’s components include sensors, actuators, events, reward functions, and a brain. Sensors and actuators serve as the interface between agents and their environments, with sensors providing data to the agent’s brain and actuators sending actions from the agent to the environment. The brain is a script that processes sensor data and makes decisions by selecting actions at each time step.

In the Unity architecture, we map agent components to prefabs or Unity scripts. These components are stored as game objects, which are game components that perform behaviors.


# Setup

The Python side of the BeMaker can be installed using local pip package. To install it,  enter the directory [bemaker4py](/bemaker) and run the command:


BeMaker has two components: the Unity side and the Python side. These components communicate with each other through the private BeMaker Communication Protocol, a lightweight application protocol for exchanging data between Unity and Python scripts.

The Unity component can be installed using the BeMaker package available through the Unity Package Manager. To do this, create a new 3D project in Unity and select “Package Manager” from the Window menu. In the Package Manager window, click on the “Add package from disk” option and then click “Install” to use BeMaker in your project.

The Python component of BeMaker can be installed using a local pip package. To install it, navigate to the bemaker4py directory and run the appropriate command:

```
    pip install -e . 
```

# BeMaker Features

* BeMaker is [Stable-baselines3](https://stable-baselines3.readthedocs.io/) friendly: BeMaker has a default configuration to support stable-baseline3 (SB3) features. We keep track of SB3 features and implement an appropriate compatibility layer.

* BeMaker supports many [HuggingFace](https://huggingface.co/) features. With this, you can provide characters with natural language capabilities. We provide a default sensor for main NLP models and a set of prompts to create characters with various peculiar personas.

* BeMaker supports ONNX models. As such, you can make your apps without python binding after agent training. For this, it is possible to load trained ONNX models in your Unity application.

* BeMaker supports 2D and 3D scenarios in Unity. We provide various made components that facilitate the development of agent-oriented games.

* BeMaker supports both classic game artificial intelligence (such as the A* algorithm) and new ways of doing pathfinding (such as reinforcement learning).

# System Requirements

BeMaker was tested on Ubuntu 22.04 and Windows 11. On Ubuntu 22.04, the best experience was achieved by installing Unity Hub beta and Unity Editor 2022.2.1f1. The optimal scenario is to use Unity and BeMaker on a Windows environment (10 or 11).


