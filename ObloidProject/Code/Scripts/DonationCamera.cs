using Godot;
using System;

public partial class DonationCamera : Node3D {
    [Export] public Node3D Pivot;
    [Export] public Camera3D CameraRaw;

    public override void _PhysicsProcess(double delta) {
        Pivot.RotateObjectLocal(Vector3.Up.Normalized(), Mathf.DegToRad(1));
        CameraRaw.LookAt(new Vector3(0, 0, 0) );
    }

}
