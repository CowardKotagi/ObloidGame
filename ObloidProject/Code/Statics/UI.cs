using System;
using Godot;
using static ObloidGame;

public static class UI {
    public static void UIProcedure(Label clockLabel, Label rootsLabel, DialogueBox dialogueBox, double delta) {
        clockLabel.Text = "Day: " + ObloidGame.currentDay + "\nHour: " + ObloidGame.currentMinute;
        rootsLabel.Text = "Roots: " + ObloidGame.Roots;
        UpdateDialogueBox(CurrentScene.dialogueBox, delta);
    }
    
    public static void ShowDialogue(DialogueBox State, string Speaker, string Dialogue, float visibleTime) {
		State.Visible = true;
		State.Speaker.Text = Speaker;
		State.Dialogue.Text = Dialogue;
		State.Timer = visibleTime;
	}

    public static void UpdateDialogueBox(DialogueBox state, double delta) {
        if (!state.Visible) { return; }
        state.Timer -= (float)delta;
        if (state.Timer <= 0 || Input.IsActionJustPressed("Interact")) {
            GD.Print("Become invisible");
            state.Visible = false;
            state.Timer = 0;
        }
        return;
    }
}