using Obloid.Code.Scripts.Data;
using Godot;

namespace Obloid.Code.Scripts.CustomNodes;

[GlobalClass]
public partial class HealthAndSpirit : Node
{
    [Export (PropertyHint.None, hintString: "Stats")]
    public HealthAndSpiritInfo MyHealthAndSpiritInfo = new HealthAndSpiritInfo();
    
    [Signal]
    public delegate void OnTakeDamageEventHandler(int health);
    
    [Signal]
    public delegate void OnDeadEventHandler();
    
    [Signal]
    public delegate void OnTakeHealingEventHandler(int health);
    
    [Signal]
    public delegate void OnUseSpiritEventHandler(int spirit);
    
    [Signal]
    public delegate void OnRegainSpiritEventHandler(int spirit);

    [Export] public bool RegenerateHealth = false;
    [Export] public bool RegenerateSpirit = false;
    
    
    //Can incorporate a timer to start healing after a certain amount of time.

    public float _currentHealth;
    public float _currentSpirit;
    public float _currentDefense;


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (RegenerateHealth)
        {
            Heal(MyHealthAndSpiritInfo.HealthRegenerationAmount);
        }

        if (RegenerateSpirit)
        {
            RegainSpirit(MyHealthAndSpiritInfo.SpiritRegenerationAmount);
        }
    }


    public override void _Ready()
    {
        //setup ints.
        _currentHealth = MyHealthAndSpiritInfo.GetMaxHealth();
        _currentDefense = MyHealthAndSpiritInfo.GetMaxDefense();
        _currentSpirit = MyHealthAndSpiritInfo.GetMaxSpirit();
        
        base._Ready();
    }

    /// <summary>
    /// DealDamage. Returns true if health = 0;
    /// </summary>
    public bool TakeDamage(float damage)
    {
        if (_currentHealth == 0)
        {
            return false;
        }
        
        //Do Damage to health but dont overdo it.
        _currentHealth -= Mathf.Clamp(damage,0, MyHealthAndSpiritInfo.GetMaxHealth());
        
        //Emit that damage has occured. Can be used for health system.
        EmitSignal("OnTakeDamage", _currentHealth);
        
        if (_currentHealth <= 0)
        {
            EmitSignal("OnDead");
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Heal Health but returns false if the players health is already maxed out.
    /// </summary>
    public bool Heal(float heal)
    {
        if (!CanHeal())
        {
            return false;
        }
        
        //Heal character but not more than the max Health amount.
        _currentHealth += Mathf.Clamp(heal,0, MyHealthAndSpiritInfo.GetMaxHealth());
        EmitSignal("OnTakeHealing", _currentHealth);
        return true;
    }

    /// <summary>
    /// Use Spirit. Returns false if the spirit was not used.
    /// </summary>
    public bool UseSpirit(float spirit)
    {
        // Use spirit if player can.
        if ((_currentSpirit - spirit) > 0)
        {
            _currentSpirit -= Mathf.Clamp(spirit,0, MyHealthAndSpiritInfo.GetMaxSpirit());
            EmitSignal("OnUseSpirit", _currentSpirit);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    /// <summary>
    /// Regenerate Spirit but returns false if the players Spirit is already maxed out.
    /// </summary>
    public bool RegainSpirit(float spirit)
    {
        if (!CanRegainSpirit())
        {
            return false;
        }
        
        //Heal character but not more than the max Health amount.
        _currentSpirit += Mathf.Clamp(spirit,0, MyHealthAndSpiritInfo.GetMaxSpirit());
        EmitSignal("OnRegainSpirit", _currentSpirit);
        
        return true; 
    }
    /// <summary>
    /// Check If Spirit isn't Full.
    /// </summary>
    public bool CanRegainSpirit()
    {
        if (_currentSpirit == MyHealthAndSpiritInfo.GetMaxSpirit())
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Check If Health isn't full.
    /// </summary>
   public bool CanHeal()
    {
        if (_currentHealth == MyHealthAndSpiritInfo.GetMaxHealth())
        {
            return false;
        }
        return true;
    }
    
}