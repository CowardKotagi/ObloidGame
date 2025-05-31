using Godot;
using System;

public partial class DonationUI : Control {
    [Export] public Label SpeechLabel;
    [Export] public TextureButton Increase;
    [Export] public TextureButton Decrease;
    [Export] public Label GiveAmount;
    [Export] public Button DoneButton;
}
