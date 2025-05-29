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

    public DirectionalLight3D Sun;

    public float ProjectileSpeedModifier = 2f;
    public DialogueBox dialogueBox;
    public Label clockLabel;
    public Label rootsLabel;

    /* We may need to use _EnterTree() when _Ready() is not fast enough.
       That is to say, Ready executes in children first, then parents. So this "Ready" is the last thing to be ready.
       But Entertree executes before ready */

    public override void _Ready(){
        Engine.MaxFps = 240;
        EnvironmentNode = this.HasNode("Environment") ? GetNode("Environment") : null;
        EntitiesNode = this.HasNode("Entities") ? GetNode("Entities") : null;
        UINode = this.HasNode("UI") ? GetNode("UI") : null;
        Sun = this.HasNode("Environment/DirectionalLight3D") ? GetNode<DirectionalLight3D>("Environment/DirectionalLight3D") : null;
        clockLabel = this.HasNode("UI/Clock") ? GetNode<Label>("UI/Clock") : null;
        rootsLabel = this.HasNode("UI/Roots") ? GetNode<Label>("UI/Roots") : null;
        dialogueBox = this.HasNode("UI/DialogueBox") ? GetNode<DialogueBox>("UI/DialogueBox") : null;

        ObloidGame.CurrentScene = this;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        if (dialogueBox != null) { dialogueBox.Visible = false; }

        Entities = GetEntitiesOfType<Node3D>(this);
        PackedScene packedScene = GD.Load<PackedScene>("res://Scenes/Enemy/ObloidMandrake.tscn");
        if (this.HasNode("Entities/Spawner")) { SpawnEnemies(GetNode("Entities"), GetNode("Entities/Spawner"), packedScene); }

        Enemies = GetEntitiesOfType<ObloidMandrake>(this);

        ObloidGame.Fade(GetNode<ColorRect>("UI/BlackFade"), 1, 0, ObloidGame.FADE_DURATION);
        Players = GetEntitiesOfType<Player>(this);
    }

    public override void _PhysicsProcess(double delta) {
        bool isUIValid = UINode != null;
        bool canPlayerInput = canInput == true && Players != null && Players.Length > 0;
        if (isUIValid) {
            HandleTime(delta, GetTree());
            UIProcedure(clockLabel, rootsLabel, dialogueBox, delta);
        }
        if (canPlayerInput) {
            HandleInput(Players[0]);
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
    public void HandleInput(CharacterBody3D Player) {
        //TODO: move input handling out of the player class. Ideally remove the player class
        if (Input.IsActionJustPressed("Menu")) {
            GD.Print("Menu");
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }
        if (Input.IsActionJustPressed("Debug")) {
            GD.Print("Debug");
            Sun.LightEnergy = Sun.LightEnergy == 0f ? 1f : 0f;
        }
        if (Input.IsActionJustPressed("Interact") && dialogueBox.Visible == false) {
            Node3D[] Entities = GetNode("Entities").GetChildren().OfType<Node3D>().ToArray();
            Node3D[] potentialTargets = GetObjectsWithinArea(Entities, Players[0].GlobalPosition - Players[0].GlobalTransform.Basis.Z * 2f, 4f, 4f);
            int count = 0;
            for (int i = 0; i < potentialTargets.Length; i++)
                if (potentialTargets[i].FindChild("InteractableComponent") != null)
                    potentialTargets[count++] = potentialTargets[i];
            Array.Resize(ref potentialTargets, count);

            Node3D Closest = null;
            float minimumDistance = float.MaxValue;
            for (int i = 0; i < potentialTargets.Length; i++) {
                float Distance = (potentialTargets[i].GlobalPosition - Players[0].GlobalPosition).Length();
                if (Distance < minimumDistance) { minimumDistance = Distance; Closest = potentialTargets[i]; }
            }
            GD.Print("Closest interactable: ", Closest?.Name ?? "None");
            if (Closest == null) { return; }
            Node interactableNode = Closest.FindChild("InteractableComponent");
            if (interactableNode is not InteractableComponent Interactable) { return; }
                InteractionData? Interaction = Interactions.GetInteraction(Interactable.NpcId);
                if (Interaction.HasValue) {
                    int Index = Interactable.Index;
                    Action[] Effects = Interaction.Value.Effects;
                    if (Index >= 0 && Index < Effects.Length) {
                        if (Effects != null && Index < Effects.Length && Effects[Index] != null) {
                            Effects[Index]?.Invoke();
                        }
                        if (Index == Effects.Length - 1) {
                            Interactable.Index = Effects.Length - 1;
                        } else {
                            Interactable.Index++;
                        }
                    }
            }
        }
    }
    private static ObloidMandrake[] SpawnEnemies(Node entitiesNode, Node inputSpawner, PackedScene packedScene) {
        var spawnerChildren = inputSpawner.GetChildren().OfType<Marker3D>().ToArray();
        var tempEnemies = new ObloidMandrake[spawnerChildren.Length];
        int count = 0;
        for (int i = 0; i < spawnerChildren.Length; i++) {
            if (GD.Randf() < 0.5f) { continue; }
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
    public void ShowDialogue(string speaker, string dialogue, float visibleTime) {
        dialogueBox.Speaker.Text = speaker;
        dialogueBox.Dialogue.Text = dialogue;
        dialogueBox.Visible = true;
    }
}
