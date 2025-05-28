using Godot;
using System;

public partial class DialogueBox : Control {
    [Export] public ColorRect Background;
    [Export] public Label Speaker;
    [Export] public RichTextLabel Dialogue;
	public float Timer;
}