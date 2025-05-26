using System;
using Godot;

namespace Obloid.Code.Scripts.Data;

public partial class CharacterData : Resource
{
    [Export] public String CharacterName;
    [Export] private float Strength = 0;
    [Export] private float Wisdom = 0;
    [Export] private float Agility = 0;
    [Export] private float Defense = 0;
    
    
    [Export] private float Speed = 0;
    [Export] private float LightDamage = 0;
    [Export] private float HeavyDamage = 0;
    
}