using Godot;
using static ObloidGame;
using static Projectile;
using static UI;
using System;
using System.Linq;
using System.Net;

public partial class GameModeDungeon : Node3D {
    public Node EnvironmentNode;
    public Node EntitiesNode;
    public Node UINode;

    [Export] public bool timeAdvance = true;
    public Player[] Players;
    public ObloidMandrake[] Enemies;
    public Node3D[] Entities;

    DirectionalLight3D Sun;

    float ProjectileSpeedModifier = 2f;
    DialogueBox dialogueBox;
    Label clockLabel;
    Label rootsLabel;

    /* We use _EnterTree() when _Ready() is not fast enough.
       That is to say, Ready executes in children first, then parents. So this "Ready" is the last thing to be ready.
       but Entertree executes before ready */

    public override void _EnterTree() {
        Engine.MaxFps = 240;
        GD.Print("GameModeDungeon EnterTree");
        Players = GetEntitiesOfType<Player>(this);
    }

    public override void _Ready() {
        EnvironmentNode = this.HasNode("Environment") ? GetNode("Environment") : null;
        EntitiesNode = this.HasNode("Entities") ? GetNode("Entities") : null;
        UINode = this.HasNode("UI") ? GetNode("UI") : null;
        this.Sun = GetNode<DirectionalLight3D>("Environment/DirectionalLight3D");
        clockLabel = GetNode<Label>("UI/Clock");
        rootsLabel = GetNode<Label>("UI/Roots");
        dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");

        ObloidGame.CurrentScene = this;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        dialogueBox.Visible = false;

        Entities = GetEntitiesOfType<Node3D>(this);
        PackedScene packedScene = GD.Load<PackedScene>("res://Scenes/Enemy/ObloidMandrake.tscn");
        if (this.HasNode("Entities/Spawner")) { SpawnEnemies(GetNode("Entities"), GetNode("Entities/Spawner"), packedScene); }

        Enemies = GetEntitiesOfType<ObloidMandrake>(this);

        ObloidGame.Fade(GetNode<ColorRect>("UI/BlackFade"), 1, 0, ObloidGame.FADE_DURATION);
    }

    public override void _PhysicsProcess(double delta) {
        HandleTime(delta, GetTree());
        if (UINode != null) {
            HandleUI(clockLabel, rootsLabel, dialogueBox);
        }
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
                        var Effects = interaction.Value.Effects;
                        if (Index >= 0 && Index < Effects.Length) {
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
        for (int i = 0; i < Players.Length; i++) {
            if (Players[i] == null || !Players[i].IsInsideTree()) { continue; }
            Player Player = Players[i];
            if (Player.IsInsideTree() && Player.GlobalPosition.Length() > 1000f) {
                GD.Print("RESET");
                Player.GlobalPosition = new Vector3(0, 0, 0);
                Player.Velocity = Vector3.Zero;
            }
        }
        if (Enemies != null && Enemies.Length != 0) {
            for (int i = 0; i < Enemies.Length; i++) {
                if (Enemies[i] == null || !Enemies[i].IsInsideTree()) { continue; }
                ObloidMandrake enemy = Enemies[i];
                if (enemy == null || !enemy.IsInsideTree()) { continue; }
                if ((enemy.GlobalPosition - Players[0].GlobalPosition).Length() > 3f) { continue; }
                if (enemy.movementState != ObloidMandrake.MovementState.Die) { continue; }
                Enemies = Enemies.Where(thisEnemy => thisEnemy != enemy).ToArray();
                enemy.QueueFree();
                if (Players[0] is Player) {
                    Roots++;
                }
                if (enemy.IsInsideTree() && enemy.GlobalPosition.Length() > 1000f) {
                    GD.Print("ENEMY OUT OF BOUNDS");
                    Roots++;
                    Enemies = Enemies.Where(existingEnemy => existingEnemy != enemy).ToArray();
                    enemy.QueueFree();
                }
            }
        }
    }
    private static ObloidMandrake[] SpawnEnemies(Node entitiesNode, Node inputSpawner, PackedScene packedScene) {
        var spawnerChildren = inputSpawner.GetChildren().OfType<Marker3D>().ToArray();
        var tempEnemies = new ObloidMandrake[spawnerChildren.Length];
        int count = 0;
        for (int i = 0; i < spawnerChildren.Length; i++) {
            if (GD.Randf() < 0.5f) continue;
            var enemy = packedScene.Instantiate<ObloidMandrake>();
            entitiesNode.AddChild(enemy);
            enemy.GlobalPosition = spawnerChildren[i].GlobalPosition;
            tempEnemies[count++] = enemy;
        }
        inputSpawner.QueueFree();
        var result = new ObloidMandrake[count];
        Array.Copy(tempEnemies, result, count);
        return result;
    }
}
