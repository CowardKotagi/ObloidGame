using System;
using Godot;
using static ObloidGame;

public static class UI {
    public static void UIProcedure(Clock clockUI, Label rootsLabel, DialogueBox dialogueBox, DonationUI DonationUI, double delta) {
        if (clockUI != null) {
            //Vector2(0, 1080)
            Vector2 startPosition = new Vector2(142, 0);
            float timeFraction = (ObloidGame.currentMinute - 0) / (ObloidGame.MAXIMUM_MINUTES - 0);
            clockUI.DayDial.Position = startPosition + (new Vector2(1080, 0) - startPosition) * timeFraction;
        }

        if (rootsLabel != null) {
            rootsLabel.Text = "Roots: " + ObloidGame.Roots;
        }

        if (dialogueBox != null) {
            UpdateDialogueBox(dialogueBox, delta);
        }

        if (DonationUI != null) {
            GD.Print(DonationUI.Increase.TexturePressed);
        }
    }
    
    public static void ShowDialogue(DialogueBox State, string Speaker, string Dialogue, float visibleTime) {
		State.Visible = true;
		State.Speaker.Text = Speaker;
		State.Dialogue.Text = Dialogue;
		State.Timer = visibleTime;
	}

    public static void UpdateDialogueBox(DialogueBox State, double delta) {
        if (!State.Visible) { return; }
        State.Timer -= (float)delta;
        if (State.Timer <= 0 || Input.IsActionJustPressed("Interact")) {
            GD.Print("Become invisible");
            State.Visible = false;
            State.Timer = 0;
        }
        return;
    }
}