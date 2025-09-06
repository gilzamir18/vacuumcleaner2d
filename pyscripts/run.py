import ai4u
import AI4UEnv
import gymnasium as gym
import numpy as np
import time
import sys

env = gym.make("AI4UEnv-v0", rid='0', config=dict(server_IP='127.0.0.1', server_port=8080, buffer_size=81900))

obs, info = env.reset()
reward_sum = 0
while True:
    action = np.random.choice(6)
    obs, reward, done, truncate, info = env.step(action)
    
    #print(obs)
    reward_sum += reward
    if done or truncate:
      print("Testing Reward: ", reward_sum)
      reward_sum = 0
      obs, truncate = env.reset()
      done = False
      time.sleep(1)
