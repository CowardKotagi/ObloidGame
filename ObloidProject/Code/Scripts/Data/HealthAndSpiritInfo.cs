using Godot;

namespace Obloid.Code.Scripts.Data;

public partial class HealthAndSpiritInfo : Resource
{
    [Export]
    private float MaxHealth = 100f;
    
    //Can incorporate other things later.
    [Export (PropertyHint.None, hintString: "Not incorporated Yet.")]
    private float MaxDefense = 100f;
    
    [Export]
    private float MaxSpirit = 100f;
    
    [Export (PropertyHint.None, hintString: "Regeneration Amount For Health.")]
    public float HealthRegenerationAmount = 0.1f;
    
    [Export (PropertyHint.None, hintString: "Regeneration Amount For Spirit.")]
    public float SpiritRegenerationAmount = 0.1f;


    public float GetMaxHealth()
    {
        return MaxHealth;
    }

    public float GetMaxDefense()
    {
        return MaxDefense;
    }

    public float GetMaxSpirit()
    {
        return MaxSpirit;
    }
}