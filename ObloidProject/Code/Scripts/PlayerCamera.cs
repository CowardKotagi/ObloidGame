using Godot;
public partial class PlayerCamera : Node3D {
    public Player Target;
    Node3D cameraBase;
    Node3D cameraPivot;
    RayCast3D cameraRayCast;
    public Camera3D cameraRaw;
        readonly Vector3[] CAMERA_OFFSETS = [
        new Vector3(0, 26, 4),
        new Vector3(0, 20, 20),
        new Vector3(0, 4, 8)
    ];
    int cameraMode = 0;

    public override void _Ready() {
        TopLevel = true;
        cameraBase = this;
        cameraPivot = GetNode<Node3D>("CameraPivot");
        cameraRaw = GetNode<Camera3D>("CameraPivot/CameraRaw");
        cameraRayCast = GetNode<RayCast3D>("CameraPivot/CameraRayCast");
        if (this.GetParent() is Player) {
            Target = (Player)this.GetParent();
            Target.Camera = this;
            cameraRayCast.AddException(Target);
        } else {
            GD.PrintErr("PlayerCamera must have a Player as its parent.");
            return;
        }
    }
    
    public override void _UnhandledInput(InputEvent @event) {
        if (Input.MouseMode != Input.MouseModeEnum.Captured){return;}
        if (@event is InputEventKey eventKey) {
            if (eventKey.IsPressed() && Input.IsActionPressed("rotateLeft")) {
                cameraBase.RotateObjectLocal(Vector3.Up.Normalized(), Mathf.DegToRad(90));
            }
            if (eventKey.IsPressed() && Input.IsActionPressed("rotateRight")) {
                cameraBase.RotateObjectLocal(Vector3.Up.Normalized(), Mathf.DegToRad(-90));
            }
            if (eventKey.IsPressed() && Input.IsActionJustPressed("cameraOptions")) {
                cameraMode = (cameraMode + 1) % CAMERA_OFFSETS.Length;
            }
        }
    }
    
    public override void _PhysicsProcess(double delta) {
        if (Target == null) { return; }
        Vector3 cameraBaseTargetPosition = Target.GlobalPosition + Target.GlobalTransform.Basis.Z.Normalized() * (Target.Velocity.Length() * -0.5f);
        cameraBaseTargetPosition.Y = Mathf.Max(cameraBaseTargetPosition.Y, -10);
        cameraBase.GlobalPosition = cameraBase.GlobalPosition.Lerp(cameraBaseTargetPosition, 0.1f);
        cameraPivot.GlobalPosition = cameraPivot.GlobalPosition.Lerp(cameraBase.GlobalPosition + Vector3.Up * 2, 0.2f);

        cameraBase.RotationDegrees = new Godot.Vector3(Target.RotationDegrees.X, cameraBase.RotationDegrees.Y, Target.RotationDegrees.Z);
        cameraPivot.Rotation = ObloidGame.SphericalLinearInterpolation(cameraPivot.Rotation, cameraBase.Rotation, 0.08f);
        
        if (cameraMode == 2) {
            cameraRayCast.GlobalPosition = cameraPivot.GlobalPosition;
            cameraRayCast.TargetPosition = CAMERA_OFFSETS[2];
            cameraRayCast.ForceRaycastUpdate();
            if (cameraRayCast.IsColliding()) {
                cameraRaw.GlobalPosition = cameraRayCast.GetCollisionPoint();
            } else if (cameraRaw.Position != CAMERA_OFFSETS[2]) {
                cameraRaw.Position = cameraRaw.Position.Lerp(CAMERA_OFFSETS[2], 0.1f);
            }
        } else {
            if (cameraRaw.Position != CAMERA_OFFSETS[cameraMode]) {
                cameraRaw.Position = cameraRaw.Position.Lerp(CAMERA_OFFSETS[cameraMode], 0.1f);
            }
        }
        cameraRaw.LookAt(cameraPivot.GlobalPosition);
    }
}
