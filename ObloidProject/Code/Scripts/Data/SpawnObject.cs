using Godot;

namespace Obloid.Code.Scripts.Data;

/// <summary>
/// A Resource Class which is used to define an Object that can be spawned.
/// </summary>
[GlobalClass]
public partial class SpawnObject : Resource
{
    [Export] public PackedScene SpawningObject;
    
}