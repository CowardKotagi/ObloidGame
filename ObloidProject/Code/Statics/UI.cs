using System;
using Godot;

public static class UI {
    public static void HandleUI(Label clockLabel, Label melonsLabel) {
        clockLabel.Text = "Day: " + ObloidGame.currentDay + "\nHour: " + ObloidGame.currentMinute;
        melonsLabel.Text = "Roots: " + ObloidGame.Roots;
    }
}