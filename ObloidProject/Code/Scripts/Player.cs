using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Projectile;
using static ObloidGame;

public partial class Player : CharacterBody3D {
    // Foreign Object references
    public PlayerCamera Camera;
    PackedScene rocketScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player/Rocket.tscn");
    // Player object references
	Node3D PlayerModel;
	CollisionShape3D CollisionShape;
	RayCast3D GroundRayCast;
    ShapeCast3D ShapeCast;
    AudioStreamPlayer3D GunshotAudio;
    Marker3D Barrel;
    AnimationPlayer AnimationPlayer;
	// Movement properties
    Vector3 velocityHorizontal;
	float velocityLength;
	Vector2 moveInput;
	Vector3 wishDirection;
	Vector3 gravity = new Godot.Vector3(0, -0.4f, 0);
	float runSpeed = 20f;
    float acceleration = 2.5f;
    // State
    float Health = 64f;
    float gunCharge;
	bool isCharging;
    bool canShoot;
	bool canDodge;
	bool canJump;
    float shootCooldown = 0f;
    const float SHOOT_COOLDOWN_DURATION = 1.88f;
    float dodgeCooldown = 0f;
    const float DODGE_COOLDOWN_DURATION = 1f;
    float autoTargetAngle = 25f;
    // Enums
    enum MovementStates : int { Idle, Run, Ledge, Fall }
	MovementStates movementState = MovementStates.Idle;

	public override void _Ready() {
		PlayerModel = GetNode<Node3D>("Drizzy");
		CollisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		GroundRayCast = GetNode<RayCast3D>("GroundRayCast");
        GunshotAudio = GetNode<AudioStreamPlayer3D>("GunShotAudio");
        Barrel = GetNode<Marker3D>("Barrel");
        AnimationPlayer = GetNode<AnimationPlayer>("Drizzy/AnimationPlayer");
	}

    public override void _PhysicsProcess(double delta) {
        velocityHorizontal = new Vector3(Velocity.X, 0, Velocity.Z);

        Vector3 cameraForward = Camera.GlobalTransform.Basis.Z.Normalized();
        Vector3 cameraRight = Camera.GlobalTransform.Basis.X.Normalized();
        moveInput = ObloidGame.canInput ? Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward") : new Godot.Vector2(0, 0);
        wishDirection = (cameraForward * moveInput.Y + cameraRight * moveInput.X).Normalized();
        Cooldowns(delta);
        
        if (Input.IsActionPressed("Light Attack") && canShoot) {
            isCharging = true;
            gunCharge += (float)delta / 1.5f;
            gunCharge = Mathf.Clamp(gunCharge, 0f, 1f);
        }
        if (Input.IsActionJustReleased("Light Attack") && isCharging && canShoot) {
            Shoot();
        }
        if (Input.IsActionJustPressed("Dodge") && canDodge) {
            Dodge();
        }

        UpdateMovementState();
        MoveAndSlide();
    }

	private void UpdateMovementState() {
		switch (movementState) {
			case MovementStates.Idle:
                AnimationPlayer.Play("Idle");
				canJump = true;
				if (!IsOnFloor()) { movementState = MovementStates.Fall; }
				if (moveInput.Length() > 0.01) { movementState = MovementStates.Run; }
				Velocity = Velocity.Lerp(Vector3.Zero, 0.2f);
				Move();
				break;
			case MovementStates.Run:
                AnimationPlayer.Play("Run");
				canJump = true;
				if (!IsOnFloor()) { movementState = MovementStates.Fall; }
				if (moveInput.Length() < 0.01 && Velocity.Length() < 1) { movementState = MovementStates.Idle; }
				Move();
				break;
			case MovementStates.Fall:
                AnimationPlayer.Play("Fall");
				if (IsOnFloor()) {
                    movementState = moveInput.Length() < 0.01 && Velocity.Length() < 1 ? MovementStates.Idle : MovementStates.Run;
                }
				Move();
				break;
		}
	}

    private void Cooldowns(double delta) {
        if (!canShoot) {
            shootCooldown -= (float)delta;
            if (shootCooldown <= 0f) {
                canShoot = true;
                shootCooldown = 0f;
            }
        }
        if (!canDodge) {
            dodgeCooldown -= (float)delta;
            if (dodgeCooldown <= 0f) {
                canDodge = true;
                dodgeCooldown = 0f;
            }
        }
    }

    private void Move() {
        if (IsOnFloor()) {
            RotateTowards(wishDirection, this);
            Vector3 targetVelocity = wishDirection * runSpeed;
            Velocity = Velocity.MoveToward(targetVelocity, acceleration);
            ApplyFloorSnap();
        } else {
            RotateTowards(wishDirection, this);
            if (Velocity.Y > -15) {
                Velocity += gravity;
            }
        }
    }
    
    private void Dodge() {
        Vector3 wishMotion = wishDirection.Length() > 0.1f ? wishDirection * 10f : this.GlobalTransform.Basis.Z.Normalized() * -10f;
        var motionParameters = new PhysicsTestMotionParameters3D {
            From = GlobalTransform,
            Motion = wishMotion,
            Margin = 0.01f,
            MaxCollisions = 4
        };
        var result = new PhysicsTestMotionResult3D();
        bool collided = PhysicsServer3D.BodyTestMotion(GetRid(), motionParameters, result);
        if (!collided) {
            GlobalPosition += wishMotion;
        } else {
            float safeFraction = result.GetCollisionSafeFraction();
            GlobalPosition += wishMotion * safeFraction;
        }
        Velocity = wishDirection * velocityHorizontal.Length();
        canDodge = false;
        dodgeCooldown = DODGE_COOLDOWN_DURATION;
    }

    private void Shoot() {
        if (CurrentScene is not GameModeDungeon Dungeon) { return; }
        if (CurrentScene.Name == "Peaceful") { return; }
        GD.Print("Shooting with charge: " + gunCharge);
        Vector3 flingDirection = this.GlobalTransform.Basis.Z.Normalized();
        float maxPower = 25f;
        if (movementState == MovementStates.Fall) {
            Velocity += flingDirection * maxPower;
        } else {
            Velocity += flingDirection * (gunCharge * maxPower);
        }
        var candidates = Dungeon.Enemies;
        Vector3 origin = GlobalPosition;
        Vector3 forward = -GlobalTransform.Basis.Z.Normalized();
        float maxRange = 80f;
        float angle = autoTargetAngle;
        var target = AcquireTarget(candidates, origin, forward, maxRange, angle);
        Vector3 targetPosition = target != null ? target.GlobalTransform.Origin : Barrel.GlobalPosition + -GlobalTransform.Basis.Z * 100f;
        SpawnProjectile(rocketScene, CurrentScene.GetNode("Entities"), targetPosition, Barrel.GlobalPosition, this);
        gunCharge = 0f;
        isCharging = false;
        canShoot = false;
        shootCooldown = SHOOT_COOLDOWN_DURATION;
        GunshotAudio.Play();
    }
}
