using System;
using static ObloidGame;
using Godot;

public struct InteractionData {
    public string npcName;
    public Action[] Effects;
}

public static class Interactions {
    public static readonly InteractionData[] Data = new InteractionData[] {
        new InteractionData {
            npcName = "NPCDonate",
            Effects = new Action[] {
                () => {
                    GD.Print("thanks for ur Obloids i am now going to set ur ObloidGame.Obloids publicly accessible integer variable to 0");
                },
                () => {
                    GD.Print("come back when u have more Obloids and look i can say multiple lines");
                    ObloidGame.Donations += ObloidGame.Donations;
                    ObloidGame.Roots -= ObloidGame.Roots;
                    GD.Print("You donated all your Obloids! Orphans fed: " + ObloidGame.Donations);
                }
            }
        },
        new InteractionData {
            npcName = "ExitDungeonLadder",
            Effects = new Action[] {
                () => {
                    GD.Print("You are free! Only if you press me again..");
                },
                () => {
                    ObloidGame.ChangeScene(ObloidGame.CurrentScene, ObloidGame.CurrentScene.GetNode<ColorRect>("UI/BlackFade"), "res://Scenes/Levels/Church.tscn");
                }
            }
        },
        new InteractionData {
            npcName = "DungeonEntrance",
            Effects = new Action[] {
                () => {
                    GD.Print("Are you sure you want to enter the dungeon? It is a dangerous place.");
                },
                () => {
                    ObloidGame.ChangeScene(ObloidGame.CurrentScene, ObloidGame.CurrentScene.GetNode<ColorRect>("UI/BlackFade"), "res://Scenes/Levels/DungeonTest.tscn");
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