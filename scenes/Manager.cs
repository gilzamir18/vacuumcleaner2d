using ai4u;
using Godot;
using System;

public partial class Manager : Node2D
{


    private RLAgent agent;
    private VaccumCleaner vaccumCleaner;

    private Label stepLabel;
    private Label endMessageLabel;
    private Label powerLabel;
    
    public override void _Ready()
    {
        agent = GetNode("../Agent").GetNode<RLAgent>("RLAgent");
        vaccumCleaner = GetNode<VaccumCleaner>("../Agent");
        stepLabel = GetNode<Label>("../CanvasLayer/StepLabel");
        endMessageLabel = GetNode<Label>("../CanvasLayer/EndMessageLabel");
        powerLabel = GetNode<Label>("../CanvasLayer/PowerLabel");
        
        agent.OnStepEnd += (RLAgent agent) =>
        {
            stepLabel.Text = "Steps: " + agent.NSteps;
            powerLabel.Text = $"Power: {vaccumCleaner.power:F1}";
        };

        agent.OnEpisodeEnd += (RLAgent agent) =>
        {
            endMessageLabel.Text = "Episode ended.";
        };

        agent.OnReset += () =>
        {
            endMessageLabel.Text = "";
            stepLabel.Text = "Steps: 0";
        };
    }
}
