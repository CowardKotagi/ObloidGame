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

    public static void InitializeUI (Clock ClockUI, RootsCounter RootsCounter, DialogueBox DialogueBox, DonationUI DonationUI) {
        if (ClockUI != null) {
            ClockUI.DayCount.Text = currentDay.ToString();
        }

        if (RootsCounter != null) {
        }

        if (DialogueBox != null) {
            DialogueBox.Visible = false; 
        }

        if (DonationUI != null) {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            DonationUI.RootsAmount.Text = Roots.ToString();
        } else {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public static void UpdateDonationUI(DonationUI DonationUI) {
        // This is hack because godot doesn't give us a "IsPressed" bool :(
        if (!Input.IsActionJustPressed("Select")) { return; }
        Vector2 mousePosition = CurrentScene.GetViewport().GetMousePosition();
        if (DonationUI.Increase.GetGlobalRect().HasPoint(mousePosition)) {
            DonationUI.GiveAmount.Text = Math.Min(DonationUI.GiveAmount.Text.ToInt() + 1, ObloidGame.Roots).ToString();
        }
        if (DonationUI.Decrease.GetGlobalRect().HasPoint(mousePosition)) {
            DonationUI.GiveAmount.Text = Math.Max(DonationUI.GiveAmount.Text.ToInt() - 1, 0).ToString();
        }
        if (DonationUI.DoneButton.GetGlobalRect().HasPoint(mousePosition)) {
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