using Godot;
using System;
using static ObloidGame;

public partial class ObloidMandrake: RigidBody3D  {
    // Object references
    Node3D PlayerModel;
    CollisionShape3D CollisionShape;
    RayCast3D GroundRayCast;
    AnimationPlayer AnimationPlayer;
    NavigationAgent3D NavigationAgent;
    GameModeDungeon Level;
    // Movement properties
    float MoveForce = 1f;
    float JumpImpulse = 8f;
    float MaxSpeed = 12f;
    // State
    public float Health = 64f;
    float detectionRadius = 10f;
    float wanderRadius = 5f;
    float chaseRadius = 40f;
    float navigationOffset;
    float navigationTimer = 0f;
    int navigationUpdateInterval = 5;
    bool navigationStarted = false;
    bool dontMove = false;
    bool Playercontrol = false;
    bool Grounded;
    Vector3 spawnPosition;
    Vector3 wishDirection;
    Vector3 targetVelocity;
    // Enumerations
    public enum MovementState { Idle, Run, Fall, Die }
    public MovementState movementState = MovementState.Idle;
    public enum AIState { Idle, Wander, Chase, Panic }
    public AIState aiState = AIState.Idle;

    public override void _Ready() {
        PlayerModel = GetNode<Node3D>("ObloidMandrakeModel");
        CollisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        GroundRayCast = GetNode<RayCast3D>("GroundRayCast");
        AnimationPlayer = GetNode<AnimationPlayer>("ObloidMandrakeModel/AnimationPlayer");
        NavigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        Level = GetNode<GameModeDungeon>(GetTree().CurrentScene.GetPath());
        navigationOffset = (float)GD.RandRange(0, 5);
        navigationUpdateInterval = (int)GD.RandRange(2, 5);
        
        spawnPosition = GlobalPosition;
    }
    
    public override void _PhysicsProcess(double delta) {
        if (movementState == MovementState.Die) { return; }
        if (!navigationStarted) {
            navigationOffset -= (float)delta;
            if (navigationOffset <= 0) {
                navigationStarted = true;
                navigationTimer = navigationUpdateInterval;
            }
        } else {
            navigationTimer -= (float)delta;
            if (navigationTimer <= 0f) {
                navigationTimer = navigationUpdateInterval;
                UpdateAIState();
            }
        }
        
        if (GroundRayCast.IsColliding()) {
            Grounded = true;
        }
        
        if (Health <= 0) {
            Health = 0;
            movementState = MovementState.Die;
        }
        UpdateMovementState();
        ApplyMovement(delta);
    }

    private void UpdateAIState() {
        float distanceToPlayer = (Level.Players[0].GlobalPosition - GlobalPosition).Length();

        if(Health <= 15) { aiState = AIState.Panic; }
        if (distanceToPlayer <= chaseRadius) { aiState = AIState.Chase; }
        else if (distanceToPlayer <= detectionRadius) { aiState = AIState.Chase; }
        else { aiState = (GD.Randi() % 2 == 0) ? AIState.Wander : AIState.Idle; }
        
        switch (aiState) {
        case AIState.Idle:
            break;
        case AIState.Wander:
            if (NavigationAgent.IsTargetReached()) {
                Vector2 wanderDirection = new Vector2((float)GD.Randf() * 2f - 1f, (float)GD.Randf() * 2f - 1f).Normalized() * wanderRadius;

                Vector3 newTarget = spawnPosition + new Vector3(wanderDirection.X, 0, wanderDirection.X);
                NavigationAgent.TargetPosition = newTarget;
            }
            break;
        case AIState.Chase:
            NavigationAgent.TargetPosition = Level.Players[0].GlobalPosition;
            break;
        case AIState.Panic:
            Vector3 panicPosition = (GlobalPosition - Level.Players[0].GlobalPosition).Normalized();
            Vector3 panicTarget = GlobalPosition + panicPosition * wanderRadius;
            NavigationAgent.TargetPosition = panicTarget;
            break;
        }
    }
    
    private void UpdateMovementState(){
        switch (movementState) {
            case MovementState.Idle:
                AnimationPlayer.Play("Idle");
                if (!Grounded) {
                    movementState = MovementState.Fall;
                } else if (wishDirection.Length() > 0.1f) {
                    movementState = MovementState.Run;
                }
                break;
            case MovementState.Run:
                AnimationPlayer.Play("Run");
                if (!Grounded) {
                    movementState = MovementState.Fall;
                } else if (wishDirection.Length() <= 0.1f) {
                    movementState = MovementState.Idle;
                }
                break;
            case MovementState.Fall:
                AnimationPlayer.Play("Jump");
                
                if (Grounded) {
                    movementState = wishDirection.Length() > 0.1f ? MovementState.Run : MovementState.Idle;
                }
                break;
            case MovementState.Die:
                AnimationPlayer.Play("Stop");
                AxisLockAngularX = false;
                AxisLockAngularY = false;
                AxisLockAngularZ = false;
                break;
        }
    }
    private void ApplyMovement(double delta) {
        if (Playercontrol) 
        {
            Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            wishDirection = new Vector3(input.X, 0, input.Y).Normalized();
        } 
        else 
        {
            dontMove = (NavigationAgent.TargetPosition - GlobalPosition).Length() < 1.0f;
            wishDirection = dontMove ? Vector3.Zero : (NavigationAgent.GetNextPathPosition() - GlobalPosition).Normalized();
            wishDirection = wishDirection.Normalized();
        }
        Vector3 forceDirection = wishDirection * MoveForce;
        RotateTowards(wishDirection, this);
        ApplyCentralImpulse(forceDirection * 0.9f);
    }
}
