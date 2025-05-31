using System;
using Godot;
using static ObloidGame;

public static class UI {
    public static void UIProcedure(Clock clockUI, RootsCounter rootsCounter, DialogueBox dialogueBox, DonationUI DonationUI, double delta) {
        if (clockUI != null) {
            Vector2 startPosition = new Vector2(142, 0);
            float timeFraction = (ObloidGame.currentMinute - 0) / (ObloidGame.MAXIMUM_MINUTES - 0);
            clockUI.DayDial.Position = startPosition + (new Vector2(1080, 0) - startPosition) * timeFraction;
        }

        if (rootsCounter != null) {
            rootsCounter.RootsLabel.Text = ObloidGame.Roots.ToString();
        }

        if (dialogueBox != null) {
            UpdateDialogueBox(dialogueBox, delta);
        }

        if (DonationUI != null) {
            if (Input.IsMouseButtonPressed(MouseButton.Left)) {
                GD.Print("Nice");
            } else { GD.Print("bad!"); }
            //Input.IsMouseButtonPressed(MouseButton.Left)
            //DonationUI.Increase.GetGlobalRect().HasPoint(CurrentScene.GetViewport().GetMousePosition())
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