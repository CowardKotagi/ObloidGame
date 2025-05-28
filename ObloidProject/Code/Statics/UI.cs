using System;
using Godot;

public static class UI {
    public static void HandleUI(Label clockLabel, Label rootsLabel, DialogueBox dialogueBox) {
        clockLabel.Text = "Day: " + ObloidGame.currentDay + "\nHour: " + ObloidGame.currentMinute;
        rootsLabel.Text = "Roots: " + ObloidGame.Roots;
    }
}