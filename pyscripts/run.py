import ai4u
import AI4UEnv
import gymnasium as gym
import numpy as np
import time
import sys

'''
O objeto env é criado a partir do ambiente AI4UEnv-v0, que simula um agente em um ambiente 2D.
O agente pode executar ações como mover-se em quatro direções (cima, baixo, esquerda, direita) e aspirar sujeira.
O ambiente retorna observações, recompensas e informações sobre o estado do agente.
'''
env = gym.make("AI4UEnv-v0", rid='0', config=dict(server_IP='127.0.0.1', server_port=8080, buffer_size=81900))

'''
A função reset() reinicia o ambiente e retorna a observação inicial e informações adicionais.
A função step(action) executa a ação fornecida e retorna a nova observação, a recompensa obtida,
um indicador de término (done), um indicador de truncamento (truncate) e informações adicionais.
'''
obs, info = env.reset()
reward_sum = 0
while True:
    action = np.random.choice(6) #aqui a ação é escolhida aleatoriamente entre 0 e 5
    obs, reward, done, truncate, info = env.step(action) #executa a ação no ambiente
    
    #print(obs)
    reward_sum += reward #acumula a recompensa
    if done or truncate: #se o episódio terminou ou foi truncado, reinicia o ambiente
      print("Testing Reward: ", reward_sum)
      reward_sum = 0
      obs, truncate = env.reset() #o ambiente é reiniciado para um estado inicial aleatório
      done = False
      time.sleep(1) #pausa de 1 segundo antes de iniciar o próximo episódio
