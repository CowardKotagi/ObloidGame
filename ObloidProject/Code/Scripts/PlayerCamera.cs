using Godot;
public partial class PlayerCamera : Node3D {
    public Player Target;
    Node3D cameraBase;
    Node3D cameraPivot;
    public Camera3D cameraRaw;
    bool canRotateCamera = true;
    float mouseSensitivity = 0.1f;
    public override void _Ready() {
        TopLevel = true;
        cameraBase = this;
        cameraPivot = GetNode<Node3D>("CameraPivot");
        cameraRaw = GetNode<Camera3D>("CameraPivot/CameraRaw");
        if (this.GetParent() is Player) {
            Target = (Player)this.GetParent();
            Target.Camera = this;
        } else {
        GD.PrintErr("PlayerCamera must have a Player as its parent.");
        return;
        }
    }
    
    public override void _UnhandledInput(InputEvent @event) {
        if (Input.MouseMode != Input.MouseModeEnum.Captured){return;}
        if (canRotateCamera == false) { return; }
        if (@event is InputEventKey eventKey) {
            if (eventKey.IsPressed() && Input.IsActionPressed("rotateLeft")) {
                cameraBase.RotateObjectLocal(Vector3.Up.Normalized(), Mathf.DegToRad(90));
            }
            if (eventKey.IsPressed() && Input.IsActionPressed("rotateRight")) {
                cameraBase.RotateObjectLocal(Vector3.Up.Normalized(), Mathf.DegToRad(-90));
            }
        }
    }
    
    public override void _PhysicsProcess(double delta) {
        if (Target == null) { return; }
        
        var desiredPosition = Target.GlobalPosition + Target.GlobalTransform.Basis.Z.Normalized() * (Target.Velocity.Length() * -0.5f);
        desiredPosition.Y = Mathf.Max(desiredPosition.Y, -10);
        cameraBase.GlobalPosition = cameraBase.GlobalPosition.Lerp(desiredPosition, 0.1f);
        cameraBase.RotationDegrees = new Godot.Vector3(Target.RotationDegrees.X, cameraBase.RotationDegrees.Y, Target.RotationDegrees.Z);
        cameraPivot.GlobalPosition = cameraPivot.GlobalPosition.Lerp(cameraBase.GlobalPosition + Vector3.Up * 3f, 0.2f);
        cameraPivot.Rotation = ObloidGame.SphericalLinearInterpolation(cameraPivot.Rotation, cameraBase.Rotation, 0.08f);
        cameraRaw.LookAt(cameraPivot.GlobalPosition);
    }
}
