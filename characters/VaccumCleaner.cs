using ai4u;
using Godot;
using System;
using System.Threading.Tasks.Sources;

public partial class VaccumCleaner : Node2D
{
    [Export] Texture2D[] dirtyTexture;
    [Export] private Marker2D powerStation;

    internal TileMapLayer[] layers;
    internal const float scorePerClean = 0.1f;
    internal float power = 1000;
    internal const float maxPower = 1000;
    internal const float minPower = 0;

    private int posX = 28;
    private int posY = 17;

    private bool[,] dirties = new bool[29, 18];
    private Sprite2D[,] dirtySprite = new Sprite2D[29, 18];

    private RLAgent agent;
    private AnimatedSprite2D animatedSprite;

    public override void _Ready()
    {

        layers = new TileMapLayer[3];
        layers[0] = GetTree().Root.GetNode("main").GetNode<TileMapLayer>("LayerFloor");
        layers[1] = GetTree().Root.GetNode("main").GetNode<TileMapLayer>("LayerObjects");
        layers[2] = GetTree().Root.GetNode("main").GetNode<TileMapLayer>("LayerDecorations");

        //Supondo que "tilemap" seja o nó TileMap
        SetPos(layers[0], posX, posY);
        agent = (RLAgent)GetNode("RLAgent");


        agent.OnStepEnd += OnStepEnd;


        animatedSprite = (AnimatedSprite2D)GetNode("AnimatedSprite2D");
        for (int i = 0; i < 29; i++)
        {
            for (int j = 0; j < 18; j++)
            {
                dirties[i, j] = GD.RandRange(0, 1) == 1;
            }
        }
    }


    private void OnStepEnd(RLAgent agent)
    {
        if (GlobalPosition.DistanceTo(powerStation.GlobalPosition) < 1)
        {
            power = Mathf.Clamp(power + 10, 0, 1000);
        }

        if (power < 1)
        {
            agent.AddReward(-1.0f, true);
            return;
        }
        SendPerception();
    }


    private bool IsDirty(int x, int y)
    {
        return this.dirties[x, y];
    }

    private void SendPerception()
    {
        int idx = 0;
        for (int i = 0; i < 29; i++)
        {
            for (int j = 0; j < 18; j++)
            {
                int tipo = GetCellType(i, j);
                if (tipo == 0)
                {
                    if (IsDirty(i, j))
                    {
                        agent.ArraySensor.SetValue(idx, 2);
                    }
                    else
                    {
                        agent.ArraySensor.SetValue(idx, 0);
                    }
                }
                else
                {
                    agent.ArraySensor.SetValue(idx, tipo);
                }
                idx++;
            }
        }
        agent.ArraySensor.SetValue(agent.ArraySensor.shape[0] - 3, (int)power);
        agent.ArraySensor.SetValue(agent.ArraySensor.shape[0] - 2, posX);
        agent.ArraySensor.SetValue(agent.ArraySensor.shape[0] - 1, posY);
        //GD.Print($"Pos X, Y: {posX},{posY} - Power: {power:F1}");
    }

    public int  GetCellType(int x, int y)
    {
        var tipo = -1;
        //Obtendo o índice do tile na célula (5, 3)
        for (var i = 0; i < layers.Length; i++)
        {
            var tileData = layers[i].GetCellTileData(new Vector2I(x, y));
            if (tileData != null)
            {
                var customData = (int)tileData.GetCustomData("tipo");
                if (customData != 0)
                {
                    tipo = customData;
                    break;
                }
                else
                {
                    tipo = customData;
                }
            }
        }
        return tipo;
    }

    public void SetAction(int action)
    {
        if (action == 1)
        {
            if (GetCellType(posX + 1, posY) == 0)
            {
                SetPos(layers[0], posX + 1, posY);
                posX = posX + 1;
            }
        }
        else if (action == 2)
        {
            if (GetCellType(posX - 1, posY) == 0)
            {
                SetPos(layers[0], posX - 1, posY);
                posX = posX - 1;
            }
        }
        else if (action == 3)
        {
            if (GetCellType(posX, posY - 1) == 0)
            {
                SetPos(layers[0], posX, posY - 1);
                posY = posY - 1;
            }
        }
        else if (action == 4)
        {
            if (GetCellType(posX, posY + 1) == 0)
            {
                SetPos(layers[0], posX, posY + 1);
                posY = posY + 1;
            }
        }
        else if (action == 5)
        {
            if (dirties[posX, posY])
            {
                RemoveDeco(posX, posY);
                dirties[posX, posY] = false;
                agent.AddReward(scorePerClean);
            }
        }
        CheckCharger();
    }

    private void CheckCharger()
    {
        string ani = animatedSprite.Animation;
        if (powerStation.GlobalPosition.DistanceTo( animatedSprite.Position ) < 1)
        {
            animatedSprite.Animation = "suck";
            power = Mathf.Clamp(power + 5, minPower, maxPower);
        }
        else
        {
            this.power = this.power * 0.995f;
            animatedSprite.Animation = ani;
        }
    }

    public void Reset()
    {
        power = 1000;
        while (true)
        {
            posX = GD.RandRange(0, 20);
            posY = GD.RandRange(0, 17);
            if (GetCellType(posX, posY)==0)
            {
                break;
            }
        }
        SetPos(layers[0], posX, posY);
        for (int i = 0; i < 29; i++)
        {
            for (int j = 0; j < 18; j++)
            {
                if (GetCellType(i, j) == 0 &&  (i != posX || j != posY))
                {
                    dirties[i, j] = GD.RandRange(0, 4) == 1 ? true : false;
                    if (dirtySprite[i, j] != null)
                    {
                        RemoveDeco(i, j);
                    }
                    if (dirties[i, j])
                    {
                        SetDeco(layers[0], i, j);
                    }
                }
                else
                {
                    dirties[i, j] = false;
                }
            }
        }
    }

    public void SetPos(TileMapLayer layer, int x, int y)
    {
        // Assumindo que 'tilemap' é o nó do seu TileMap e 'player' o nó do seu personagem.
        var tileCoords = new Vector2I(x, y); // Coordenadas de tile para onde o objeto deve ir
        var localPosition = layer.MapToLocal(tileCoords);//tilemap.map_to_local(tile_coords);
        GlobalPosition = localPosition;
    }

    public void SetDeco(TileMapLayer layer, int x, int y)
    {
        var tileCoords = new Vector2I(x, y);

        //Cria uma nova instância do Sprite2D
        var sprite = new Sprite2D();

        //Define a textura do sprite
        if (dirtyTexture.Length > 0)
        {
            sprite.Texture = dirtyTexture[GD.RandRange(0, dirtyTexture.Length - 1)];
        }

        //Converte as coordenadas do tile para a posição global no mundo
        // e centraliza o sprite no meio do tile.
        sprite.Position = layer.MapToLocal(tileCoords);

        //Adiciona o sprite como um filho do TileMap (ou da sua cena principal)
        GetTree().Root.GetNode("main").AddChild(sprite);
        GetTree().Root.GetNode("main").MoveChild(sprite, 4);
        //Para garantir que a sobreposição apareça por cima do TileMap,
        //você pode ajustar o Z Index do sprite.
        sprite.ZIndex = 0;
        dirtySprite[x, y] = sprite;
    }

    public void RemoveDeco(int x, int y)
    {
        if (dirtySprite[x, y] != null)
        {
            dirtySprite[x, y].QueueFree();
            dirtySprite[x, y] = null;
        }
    }
}
