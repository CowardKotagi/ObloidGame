using System;
using static ObloidGame;
using Godot;

public struct InteractionData {
    public string npcName;
    public string[] dialogueLines;
    public Action[] Effects;
}

public static class Interactions  {
    public static readonly InteractionData[] Data = new InteractionData[] {
        new InteractionData {
            npcName = "NPCDonate",
            dialogueLines = new[] {
                "thanks for ur Obloids i am now going to set ur ObloidGame.Obloids publicly accessible integer variable to 0",
                "come back when u have more Obloids and look i can say multiple lines"
            },
            Effects = new Action[] {
                null,
                () => {
                    ObloidGame.orphansFed += ObloidGame.orphansFed;
                    ObloidGame.Mandrakes -= ObloidGame.Mandrakes;
                    GD.Print("You donated all your Obloids! Orphans fed: " + ObloidGame.orphansFed);
                }
            }
        },
        new InteractionData {
            npcName = "ExitDungeonLadder",
            dialogueLines = new[] {
                "You are free! Only if you press me again..",
                ""
            },
            Effects = new Action[] {
                null,
                () => {
                    ObloidGame.CurrentScene.GetTree().ChangeSceneToFile("res://Scenes/Levels/Church.tscn");
                }
            }
        },
        new InteractionData {
            npcName = "DungeonEntrance",
            dialogueLines = new[] {
                "Are you sure you want to enter the dungeon? It is a dangerous place.",
                ""
            },
            Effects = new Action[] {
                null,
                () => {
                    ObloidGame.CurrentScene.GetTree().ChangeSceneToFile("res://Scenes/Levels/DungeonTest.tscn");
                }
            }
        },
    };

    public static InteractionData? GetInteraction(string npcName) {
        foreach (var entry in Data)
            if (entry.npcName == npcName)
                return entry;
        return null;
    }
}