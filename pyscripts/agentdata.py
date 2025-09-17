import numpy as np

CHARGER_POS = (19, 6)
GRID_WIDTH = 29
GRID_HEIGHT = 18
NGD_POWER = 0 #POSITION OF THE POWER VALUE ON NGD (NGD = Non-Grid Data)
NGD_LINE = 1 #POSITION OF THE AGENT (LINE) ON NGD
NGD_COLUMN = 2 #POSITION OF THE AGENT (COLUMN) ON NGD
GD_SBDIRTY = 2 #SYMBOL OF DIRTIES ON GRID DATA
GD_SBCOLIDER = 1 #SYMBOL OF COLIDERS ON GRID DATA

def get_grid_data(obs):
   print("obs shape:", obs[0].shape)
   dt = np.zeros((GRID_WIDTH, GRID_HEIGHT), dtype=np.int16)
   idx = 0
   for i in range(GRID_WIDTH):
      for j in range(GRID_HEIGHT):
         dt[i, j] = int(obs[0][idx])
         idx += 1
   return dt

def get_nogrid_data(obs):
   return obs[0][-3:]

def get_agent_state(non_grid_data):
   posx = non_grid_data[NGD_COLUMN]
   posy = non_grid_data[NGD_LINE]
   power = non_grid_data[NGD_POWER]
   return power, int(posx), int(posy)

def get_state(obs):
   return get_grid_data(obs), get_agent_state(get_nogrid_data(obs))
