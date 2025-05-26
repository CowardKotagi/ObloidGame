using Godot;
using static ObloidGame;
using static Projectile;
using static UI;
using System;
using System.Linq;

public partial class GameModeOrphan : Node3D {
    public Player[] Players;
    public Node3D[] Entities;

    DirectionalLight3D Sun;

    float ProjectileSpeedModifier = 2f;

    Label clockLabel;
    Label melonsLabel;

    /* We use _EnterTree() when _Ready() is not fast enough.
       That is to say, Ready executes in children first, then parents. So this "Ready" is the last thing to be ready.
       but Entertree executes before ready */

    public override void _EnterTree() {
        Players = GetEntitiesOfType<Player>(this);
    }

    public override void _Ready() {
        ObloidGame.CurrentScene = this;
        Sun = GetNode<DirectionalLight3D>("Environment/DirectionalLight3D");
        clockLabel = GetNode<Label>("UI/Clock");
        melonsLabel = GetNode<Label>("UI/Melons");
        Entities = GetEntitiesOfType<Node3D>(this);
        Input.MouseMode = Input.MouseModeEnum.Captured;

        Players[0].Level = this;
    }
    
    public override void _PhysicsProcess(double delta) {
        HandleTime(delta, GetTree());
        HandleUI(clockLabel, melonsLabel);
        if (Input.IsActionJustPressed("Menu")) {
            GD.Print("Menu");
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }
        if (Input.IsActionJustPressed("Debug")) {
            GD.Print("Debug");
            Sun.LightEnergy = Sun.LightEnergy == 0f ? 1f : 0f;
        }
        if (Input.IsActionJustPressed("Interact")) {
            Node3D[] Entities = GetNode("Entities").GetChildren().OfType<Node3D>().ToArray();
            Node3D[] potentialTargets = GetObjectsWithinArea(Entities, Players[0].GlobalPosition - Players[0].GlobalTransform.Basis.Z * 2f, 4f, 4f);
            int count = 0;
            for (int i = 0; i < potentialTargets.Length; i++)
                if (potentialTargets[i].FindChild("InteractableComponent") != null)
                    potentialTargets[count++] = potentialTargets[i];
            Array.Resize(ref potentialTargets, count);

            Node3D Closest = null;
            float minimumDistance = float.MaxValue;
            foreach (var Target in potentialTargets) {
                float Distance = (Target.GlobalPosition - Players[0].GlobalPosition).Length();
                if (Distance < minimumDistance) { minimumDistance = Distance; Closest = Target; }
            }
            GD.Print("Closest interactable: ", Closest?.Name ?? "None");
            if (Closest != null) {
                var interactableNode = Closest.FindChild("InteractableComponent") as Node;
                if (interactableNode is InteractableComponent interactable) {
                    var interaction = Interactions.GetInteraction(interactable.NpcId);
                    if (interaction.HasValue) {
                        int Index = interactable.Index;
                        var Lines = interaction.Value.dialogueLines;
                        var Effects = interaction.Value.Effects;
                        if (Index >= 0 && Index < Lines.Length) {
                            string line = Lines[Index];
                            if (!string.IsNullOrWhiteSpace(line)) {
                                GD.Print(line);
                            }
                            if (Effects != null && Index < Effects.Length && Effects[Index] != null) {
                                Effects[Index]?.Invoke();
                            }
                            interactable.Index++;
                        }
                    }
                }
            }
        }
        Node3D[] projectiles = GetEntitiesOfType<Node3D>(this);
        for (int i = 0; i < projectiles.Length; i++) {
            if (projectiles[i].IsInGroup("Projectile")) {
                HandleProjectiles(projectiles[i], ProjectileSpeedModifier, this);
            }
        }

        if (Players[0].IsInsideTree() && Players[0].GlobalPosition.Length() > 1000f) {
            GD.Print("RESET");
            Players[0].GlobalPosition = new Vector3(0, 0, 0);
            Players[0].Velocity = Vector3.Zero;
        }
    }
}
