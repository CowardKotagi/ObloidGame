using System;
using static ObloidGame;
using static UI;
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
                    ObloidGame.ChangeScene(ObloidGame.CurrentScene, ObloidGame.CurrentScene.GetNode<ColorRect>("UI/BlackFade"), "res://Scenes/Levels/DonationScene.tscn");
                },
                () => {
                    ShowDialogue(CurrentScene.dialogueBox, "NPCDonate", "Thank you.", 2f);
                    ObloidGame.Donations += ObloidGame.Donations;
                    ObloidGame.Roots -= ObloidGame.Roots;
                }
            }
        },
        new InteractionData {
            npcName = "ExitDungeonLadder",
            Effects = new Action[] {
                () => {
                    ObloidGame.ChangeScene(ObloidGame.CurrentScene, ObloidGame.CurrentScene.GetNode<ColorRect>("UI/BlackFade"), "res://Scenes/Levels/Church.tscn");
                },
            }
        },
        new InteractionData {
            npcName = "DungeonEntrance",
            Effects = new Action[] {
                () => {
                    ObloidGame.ChangeScene(ObloidGame.CurrentScene, ObloidGame.CurrentScene.GetNode<ColorRect>("UI/BlackFade"), "res://Scenes/Levels/LevelTest.tscn");
                },
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
