import ai4u
import AI4UEnv
import gymnasium as gym
import numpy as np
import time
from agentdata import * #provides the get_state function to get the grid and agent state
from searchutils import * #provides class Node to generate the search tree
from collections import deque

''' Search Agent using BFS to find the nearest dirty cell 
With the path found, the agent will move to the cell and clean it.
The use of the function get_state from agentdata.py is necessary to get the grid and agent state.
The grid data is a 2D array where each cell can be empty, dirty or a collider.
The agent state is a tuple with the power, x and y position of the agent.
The search_path function implements the BFS algorithm to find the nearest dirty cell.
'''

env = gym.make("AI4UEnv-v0", rid='0', config=dict(server_IP='127.0.0.1', server_port=8080, buffer_size=81900))

def search_path(grid, agent_state):
  frontier = deque()
  current_pos = (agent_state[NGD_COLUMN], agent_state[NGD_LINE])
  frontier.append(Node(current_pos))
  print("Initial pos: ", current_pos)
  while len(frontier)>0:
    node = frontier.popleft()
    pos = node.position
    print("state ", grid[pos[0], pos[1]])
    if grid[pos[0], pos[1]] == GD_SBDIRTY:
      path = []
      while node is not None:
        if (node.action is not None):
          path.append(node.action)
        node = node.parent
      if (len(path) == 0):
        return ["suck"]
      return path[::-1]  # Return reversed path
    actions = {(1 , 0): "right", (-1, 0): "left", (0, -1): "up", (0, 1): "down"}  # Up, Down, Left, Right
    for move in [(-1, 0), (1, 0), (0, -1), (0, 1)]:  # Up, Down, Left, Right
      neighbor = ( int(node.position[0] + move[0]), int(node.position[1] + move[1]) )
      if (0 <= neighbor[0] < GRID_WIDTH and
          0 <= neighbor[1] < GRID_HEIGHT):
        
        if grid[neighbor[0], neighbor[1]] != GD_SBCOLIDER:
          frontier.append(Node(neighbor, node, actions[move]))
          print("Valid Neighbor: ", neighbor)
        else:
          print("Invalid Neighbor: ", neighbor)
  print("Path not found")
  return ["no_op"]  # No path found

obs, info = env.reset()
reward_sum = 0
path = None
action_map = {"right": 1, "left": 2, "up": 3, "down": 4, "suck": 5, "no_op": 0} #mapa de ações
while True:
    action = None
    if path is None or len(path) == 0:
      grid, agent_state = get_state(obs)
      path = search_path(grid, agent_state)
      print("Action Path: ", path)
      if grid[agent_state[NGD_COLUMN], agent_state[NGD_LINE]] == GD_SBDIRTY:
          action = action_map["suck"] if grid[agent_state[NGD_COLUMN], agent_state[NGD_LINE]] == GD_SBDIRTY else action_map["no_op"]
      else:
        action = action_map[path.pop(0)] if path else action_map["no_op"]
      
    else:
      action = action_map[path.pop(0)] if path else action_map["no_op"]
    print("Action: ", action)
  
    obs, reward, done, truncate, info = env.step(action)
    reward_sum += reward
    if done or truncate:
      print("Testing Reward: ", reward_sum)
      reward_sum = 0
      obs, truncate = env.reset()
      done = False
      time.sleep(1)
