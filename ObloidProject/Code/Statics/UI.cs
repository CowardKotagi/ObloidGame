using System;
using Godot;
using static ObloidGame;

public static class UI {
    public static void UIProcedure(Clock ClockUI, RootsCounter RootsCounter, DialogueBox DialogueBox, DonationUI DonationUI, double delta) {
        if (ClockUI != null) {
            Vector2 startPosition = new Vector2(142, 0);
            float timeFraction = (ObloidGame.currentMinute - 0) / (ObloidGame.MAXIMUM_MINUTES - 0);
            ClockUI.DayDial.Position = startPosition + (new Vector2(1080, 0) - startPosition) * timeFraction;
        }

        if (RootsCounter != null) {
            RootsCounter.RootsLabel.Text = ObloidGame.Roots.ToString();
        }

        if (DialogueBox != null) {
            UpdateDialogueBox(DialogueBox, delta);
        }

        if (DonationUI != null) {
            UpdateDonationUI(DonationUI);
        }
    }

    public static void UpdateDonationUI(DonationUI DonationUI) {
        if (Input.IsActionJustPressed("Select") && DonationUI.Increase.IsHovered()) {
            DonationUI.GiveAmount.Text = Math.Min(DonationUI.GiveAmount.Text.ToInt() + 1, ObloidGame.Roots).ToString();
        }
        if (Input.IsActionJustPressed("Select") && DonationUI.Decrease.IsHovered()) {
            DonationUI.GiveAmount.Text = Math.Max(DonationUI.GiveAmount.Text.ToInt() - 1, 0).ToString();
        }
        if (Input.IsActionJustPressed("Select") && DonationUI.DoneButton.IsHovered()) {
            if (DonationUI.GiveAmount.Text.ToInt() <= ObloidGame.Roots) {
                Donations += DonationUI.GiveAmount.Text.ToInt();
                ObloidGame.Roots -= DonationUI.GiveAmount.Text.ToInt();
                DonationUI.GiveAmount.Text = "0";
                ObloidGame.ChangeScene(ObloidGame.CurrentScene, ObloidGame.CurrentScene.GetNode<ColorRect>("UI/BlackFade"), "res://Scenes/Levels/LevelTest.tscn");
                currentDay += 1;
            }
        }
        return;
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
    public static void ShowDialogue(DialogueBox State, string Speaker, string Dialogue, float visibleTime) {
		State.Visible = true;
		State.Speaker.Text = Speaker;
		State.Dialogue.Text = Dialogue;
		State.Timer = visibleTime; 
	}
}