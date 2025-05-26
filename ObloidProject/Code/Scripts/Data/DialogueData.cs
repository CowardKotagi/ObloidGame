using Godot;

namespace Obloid.Code.Scripts.Data;

[GlobalClass]
public partial class DialogueData : Resource
{
        [Export] public string Id { get; set; }
        [Export] public string Text { get; set; }
        [Export] public string Speaker { get; set; }
        [Export] public DialogueChoice[] Choices { get; set; }
        [Export] public string NextId { get; set; }
        [Export] public bool End { get; set; }
}

[GlobalClass]
public partial class DialogueChoice : Resource
{
        [Export] public string Text { get; set; }
        [Export] public string NextId { get; set; }
        [Export] public string RequiredItem { get; set; }
}