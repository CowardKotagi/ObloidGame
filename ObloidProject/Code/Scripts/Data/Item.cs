using Godot;

namespace Obloid.Code.Scripts.Data
{
    [GlobalClass]
    public partial class Item : Resource
    {
        [Export] public string Name { get; set; }

        [Export(PropertyHint.Range, "0.05,1.0,0.05")]
        public double SpawnChance;

        [Export] public PackedScene Scene { get; set; }
        
    }
}