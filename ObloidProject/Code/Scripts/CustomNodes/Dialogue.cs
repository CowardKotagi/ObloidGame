using System;
using System.Collections.Generic;
using System.Net.Mime;
using Obloid.Code.Scripts.Data;
using Godot;

namespace Obloid.Code.Scripts.CustomNodes;

[GlobalClass]
public partial class Dialogue : Node
{
    [Export(PropertyHint.None, hintString: "This file is necessary to start conversation")]
    private string dialogueFilePath = "Copy File Path For Dialogue and paste it here.";

    private Godot.Collections.Dictionary<string, DialogueData> _dialogueInfo = new Godot.Collections.Dictionary<string, DialogueData>();
    
    public override void _Ready()
    {
        if (dialogueFilePath == null)
        {
            GD.Print("NO FILE SYSTEM FOUND");
        }
        
        LoadDialogue();
        
        PrintDialogue();
        
        base._Ready();
    }

    public void LoadDialogue()
    {
        var file = FileAccess.Open(dialogueFilePath,FileAccess.ModeFlags.Read);
        
        //Get as text
        if (file != null)
        {
            var jsonText = file.GetAsText();
            //Parse the entire string.
            var jsonResults = Json.ParseString(jsonText);
        
            if (jsonResults.VariantType == Variant.Type.Nil)
            {
                GD.PrintErr($"Failed to parse JSON from {dialogueFilePath}");
                return;
            }
            
            var jsonData = jsonResults.AsGodotDictionary();
            if (!jsonData.ContainsKey("dialogues"))
            {
                GD.PrintErr($"JSON file {dialogueFilePath} missing 'dialogues' key");
                return;
            }
            var dialogueDict = jsonData["dialogues"].AsGodotDictionary();
            
            foreach (var key in dialogueDict.Keys)
            {
                var data = dialogueDict[key].AsGodotDictionary();
                _dialogueInfo[key.AsString()] = new DialogueData()
                {
                    Id = data.ContainsKey("id") ? data["id"].AsString() : "null",
                    Text = data.ContainsKey("text") ? data["text"].AsString() : "null",
                    Speaker = data.ContainsKey("speaker") ? data["speaker"].AsString() : "null",
                    Choices = ParseChoices(data.ContainsKey("choices") ? data["choices"].AsGodotArray() : new Godot.Collections.Array()),
                    NextId = data.ContainsKey("next_id") ? data["next_id"].AsString() : "null",
                    End = data.ContainsKey("end") && data["end"].AsBool()
                };
                
                dialogueDict[key.AsString()] = data;
            }
        }
        else
        {
           
            GD.Print("NO DIALOGUE FOUND ON OBJECT : ", GetParent().Name);
            
        }
    }
    
    private DialogueChoice[] ParseChoices(Godot.Collections.Array choicesData)
    {
        var choices = new List<DialogueChoice>();
        foreach (var choice in choicesData)
        {
            var data = choice.AsGodotDictionary();
            var conditionDict = data.ContainsKey("condition") ? data["condition"].AsGodotDictionary() : null;
            
            choices.Add(new DialogueChoice
            {
                Text = data.ContainsKey("text") ? data["text"].AsString() : "",
                NextId = data.ContainsKey("next_id") ? data["next_id"].AsString() : "",
                RequiredItem = conditionDict != null && conditionDict.ContainsKey("item") ? conditionDict["item"].AsString() : ""
            });
        }
        return choices.ToArray();
    }


    public void PrintDialogue()
    {
        GD.Print(_dialogueInfo["intro"].Speaker + " Says: " + _dialogueInfo["intro"].Text); 
    }
}