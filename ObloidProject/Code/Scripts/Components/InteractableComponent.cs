using Godot;

public partial class InteractableComponent : Node {
    [Export]
    public string NpcId { get; set; } = "";

    [Export]
    public int Index { get; set; } = 0;
}