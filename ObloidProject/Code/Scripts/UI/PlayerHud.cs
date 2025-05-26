using Godot;
using System;
using Obloid.Code.Scripts.CustomNodes;
using Obloid.Code.Scripts.Data;

public partial class PlayerHud : Control
{
    //Control Of Health.
    //Control of Time.
    //Control of Spirit.
    [Export] private TextureRect SunDial;
    [Export] private ProgressBar HealthBar;
    [Export] private ProgressBar SpiritBar;

    private GameModeDungeon _levelRef;
    private Player _playerRef;
    
    [Export]
    public NodePath _playerPath;
    
    
    public override void _Ready()
    {
        base._Ready();
        
        _levelRef = GetNode<GameModeDungeon>(GetTree().CurrentScene.GetPath());
        _playerRef = GetNode<Player>(_playerPath);

        if (_playerRef is not null)
        {
            ConnectToPlayerHealth();
        }
        

        GD.Print(_playerRef.Name);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        ChangeSunDial(delta);
    }

    public void ConnectToPlayerHealth()
    {
 
        HealthAndSpirit _playerHAndS = _playerRef.GetNode<HealthAndSpirit>("HealthAndSpirit");

        if (_playerHAndS is not null)
        {
            _playerHAndS.Connect("on_take_damage", Callable.From((int currentHealth) =>
            {
                ChangeHealthBar(currentHealth);
            }));
            _playerHAndS.Connect("on_take_healing", Callable.From((int currentHealth) =>
            {
                ChangeHealthBar(currentHealth);
            }));
            
            _playerHAndS.Connect("on_use_spirit", Callable.From((int currenSpirit) =>
            {
                ChangeSpiritBar(currenSpirit);
            }));
            _playerHAndS.Connect("on_regain_spirit", Callable.From((int currenSpirit) =>
            {
                ChangeSpiritBar(currenSpirit);
            }));
        }
    }

    public void ChangeHealthBar(float Health)
    {
        HealthBar.Value = Health;
    }

    public void ChangeSpiritBar(float Spirit)
    {
        
        SpiritBar.Value = Spirit;
    }

    public void ChangeSunDial(double currentDial)
    {
        // Get Max Time.
        // A full rotation is 360 degrees.
        // End of day is full rotation on the clock.
        float _min = ObloidGame.currentMinute;
        float _maxTime = ObloidGame.MAXIMUM_MINUTES;
        
        float NormalizedTime = _min / _maxTime;
        
        float RotationDegree = NormalizedTime * 360;

        //-45 degrees places it at the perfect rotation.
        SunDial.RotationDegrees =  -45 + RotationDegree;
    }
}
