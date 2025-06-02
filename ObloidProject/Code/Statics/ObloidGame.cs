using System;
using System.Linq;
using Godot;

public static class ObloidGame {
    public static int currentDay = 1;
    public static float currentMinute = 0;
    public const int MAXIMUM_DAYS = 30;
    public const int MAXIMUM_MINUTES = 5;
    public static int Roots = 10;
    public static int donationBuffer;
    public static int Donations = 0;
    public const int FADE_DURATION = 1;
	public static bool canInput = true;
    public static dynamic CurrentScene;
    // allman to K&R find and replace:
    // find ([^\r\n]+)\r?\n[ \t]*\{[ \t]*\r?\n
    // replace $1 {\n
    
    public static void ChangeScene(Node inputNode, ColorRect fadeRectangle, string scenePath) {
        if (fadeRectangle == null) return;
        Fade(fadeRectangle, 0f, 1f, FADE_DURATION);

        Tween tween = fadeRectangle.GetTree().CreateTween();
        tween.TweenInterval(FADE_DURATION);
        tween.TweenCallback(Callable.From(() => {
            if (inputNode != null && inputNode.GetTree() != null) {
                inputNode.GetTree().ChangeSceneToFile(scenePath);
            }
        }));
    }

    public static void Fade(ColorRect inputColorRect, float fromAlpha, float toAlpha, float duration) {
        if (inputColorRect == null) return;
        inputColorRect.Visible = true;
        var color = inputColorRect.Modulate;
        color.A = fromAlpha;
        inputColorRect.Modulate = color;
        Tween tween = inputColorRect.GetTree().CreateTween();
        tween.TweenProperty(inputColorRect, "modulate:a", toAlpha, duration)
             .SetTrans(Tween.TransitionType.Linear)
             .SetEase(Tween.EaseType.InOut);
    }

    /// <summary>
    /// Casts a ray from inputStartPosition to InputEndPosition, optionally returning a property from the collision result such as a hitposition or normal.
    /// Returns false if no collision, true if collision and no property requested, or the property value. 
    /// This function requires a CollisionObject3D as the caster so that it:
    /// 1. Has a world in which to cast the ray.
    /// 2. Can be excluded from the raycast to avoid self-collision.
    /// </summary>
    public struct RaycastHitInfo {
        public Vector3 Position;
        public Vector3 Normal;
        public object Collider;
        public Godot.Collections.Dictionary RawResult;
    }
    public static bool RaycastWorld(CollisionObject3D caster, Vector3 start, Vector3 end, out RaycastHitInfo hitInfo) {
        hitInfo = new RaycastHitInfo();
        var rayResult = caster.GetWorld3D().DirectSpaceState.IntersectRay(
            PhysicsRayQueryParameters3D.Create(start, end, 1, new Godot.Collections.Array<Rid> { caster.GetRid() })
        );
        if (rayResult.Count <= 0)
            return false;
        hitInfo.RawResult = rayResult;
        hitInfo.Position = (Vector3)rayResult["position"];
        hitInfo.Normal = (Vector3)rayResult["normal"];
        hitInfo.Collider = rayResult["collider"];
        return true;
    }
    
    /// <summary>
    /// Spawns a debug cube at the given position and size, as a child of inputTarget. This is useful for visualizing positions/objects that you can't otherwise see.
    /// </summary>
    public static void DebugSpawnCube(Vector3 inputPosition, float inputSize, Node inputTarget) {
        MeshInstance3D cube = new MeshInstance3D();
        cube.Mesh = new BoxMesh();
        cube.TopLevel = true;
        ((BoxMesh)cube.Mesh).Size = new Vector3(inputSize, inputSize, inputSize);
        inputTarget.AddChild(cube);
        cube.GlobalPosition = inputPosition;
    }
    /// <summary>
    /// Returns all Node3D objects from inputArray within a cylindrical area defined by inputPosition, radius, and height.
    /// </summary>
    public static Node3D[] GetObjectsWithinArea(Node3D[] inputArray, Vector3 inputPosition, float radius, float height) {
        height *= 0.5f;
        int matchCount = 0;
        Node3D[] matches = new Node3D[inputArray.Length];
        foreach (Node3D node in inputArray) {
            Vector3 offsetFromInputPosition = node.GlobalPosition - inputPosition;
            if (offsetFromInputPosition.Y < -height || offsetFromInputPosition.Y > height) continue;
            Vector2 offsetXZ = new Vector2(offsetFromInputPosition.X, offsetFromInputPosition.Z);
            if (offsetXZ.Length() <= radius) {
                matches[matchCount++] = node;
            }
        }
        Node3D[] result = new Node3D[matchCount];
        Array.Copy(matches, result, matchCount);
        return result;
    }
    /// <summary>
    /// Performs spherical linear interpolation (slerp) between two Euler rotations. 
    /// I don't remember why I made this, but I remember Godot's built-in interpolation being broken and I trust my past self.
    /// </summary>
    public static Vector3 SphericalLinearInterpolation(Vector3 inputInitialRotation, Vector3 targetRotation, float weight) {
        Quaternion initialQuaternion = new Quaternion(Basis.FromEuler(inputInitialRotation));
        Quaternion targetQuaternion = new Quaternion(Basis.FromEuler(targetRotation));
        Quaternion interpolatedQuaternion = initialQuaternion.Slerp(targetQuaternion, weight);
        Vector3 finalRotation = interpolatedQuaternion.GetEuler();
        return finalRotation;
    }
    /// <summary>
    /// Rotates inputNode to face the given lookDirection smoothly. 
    /// Useful for players, enemies, or any Node3D that needs to face a direction.
    /// </summary>
    public static void RotateTowards(Godot.Vector3 lookDirection, Node3D inputNode) {
        if (lookDirection.Length() <= 0.0001f) { return; }
        float targetRotation = (float)System.Math.Atan2(lookDirection.X * -1, lookDirection.Z * -1);
        //inputNode.GlobalRotation = new Godot.Vector3(inputNode.Rotation.X, targetRotation, inputNode.Rotation.Z);
        inputNode.Rotation = new Godot.Vector3(inputNode.Rotation.X, Godot.Mathf.LerpAngle(inputNode.Rotation.Y, targetRotation, 0.4f), inputNode.Rotation.Z);
    }
    
    public static void Damage(dynamic inputVictim, int inputDamage) {
        if (inputVictim.Health > 0) {
            inputVictim.Health -= inputDamage;
        } else {
            GD.Print("no health found");
        }
    }
    
    public static void HandleTime(double delta, SceneTree tree) {
        ObloidGame.currentMinute += (float)(delta / 60f);
        if (ObloidGame.currentMinute >= ObloidGame.MAXIMUM_MINUTES) {
            tree.ChangeSceneToFile("res://Scenes/Levels/Church.tscn");
            ObloidGame.currentMinute = 0f;
            return;
        }
    }
    public static T[] GetEntitiesOfType<T>(Node Root) where T : Node {
        Node entitiesNode = Root.GetNode("Entities");
        if (entitiesNode == null) { return new T[0]; }
        return entitiesNode.GetChildren().Cast<Node>().OfType<T>().ToArray();
    }
    /*
    ███████████████████████████████████████████████████████████████████████████████████████████████████████████████
    ███████████████████████████████████████████████████████████████████████████████████████████████████████████████
    ███████████████████████████████████████████████████████████████████████████████████████████████████████████████
    ███████████████████████████████████████████████████████████████████████████████████████████████████████████████
    ███████████████████████████████████████████████████████████████████████████████████████████████████████████████
    ███████████████████████████████████████████████████████████████████████████████████████████████████████████████
    ████████████████████████████████▓▒█████████████████████████████████████████████████████████████████████████████
    ████████████████████████████▒░░░░░▒████████████████████████████████████████████████████████████████████████████
    ████████████████████████████▒░░░░░░▒███████████████████████████████████████████████████████████████████████████
    █████████████████████████████▒░░░░░░░██████████████████████████████████████████████████████████████████████████
    ██████████████████████████████▒░░░░░░░▓████████████████████████████████████████████████████████████████████████
    ███████████████████████████████▓░░░░░░░▒███████████████████████████████████████████████████████████████████████
    █████████████████████████████████░░░░░░░▓█████████████████████▓░░██████████████████████████████████████████████
    ██████████████████████████████████░░░░░░░▒████████████████▓░░░░░▒▒█████████████████████████████████████████████
    ███████████████████████████████████░░░░░░░▒████████████▒░░░░░░░░░░░████████████████████████████████████████████
    ████████████████████████████████████▒░░░░░░░███████▓░░░▒░░░░░░░░░░▒▓███████████████████████████████████████████
    █████████████████████████████████████▒░░░░░░░██▓▒░░░░░░░▒▓▒░░░░▓███████████████████████████████████████████████
    ██████████████████████████████████████▓░░░░░░░░░░░░░░░░░░░▓████████████████████████████████████████████████████
    ███████████████████████████████████████▓░░░░░░░░░░░░░░░░░█░▒███████████████████████████████████████████████████
    ████████████████████████████████████▓▒░░░░░░░░▒▒▓▓▓▒░░░░░▓█████████████████████████████████████████████████████
    █████████████████████████████████▒░░░░░░░░░░░▓▒██████▒░░░░▒████████████████████████████████████████████████████
    █████████████████████████████▓▒░░░░░░░░░░░░░▒▓████████▓░░░▓▓███████████████████████████████████████████████████
    █████████████████████████▓▒░░░░░░░░░░░░░░░░░▓█▓████████▒░░▓████████████████████████████████████████████████████
    ██████████████████████▒░░░░░░░░░░░░░░▒██▓░░▒██████████▓███▒████████████████████████████████████████████████████
    ██████████████████▓░░░░░░░░░░░░░░░▓████▒░░░██▓▓██░▓███████▒▒███████████████████████████████████████████████████
    ███████████████▒░░░░░░░░░░░░░░▒███████▓░░░▓█▓████░░▓▓█████▒▒▓██████████████████████████████████████████████████
    ███████████▓▒░░░░░░░░░░░░░▒▓█████████▓░░░░██▒██▒░░░░▓▓▓███▓▓░██████████████████████████████████████████████████
    ███████▓▒░░░░░░░░░░░░░░▓█████████████░░░░▒▓░▒▒▓▒░░░▒█████▓▒▒▒██████████████████████████████████████████████████
    ███████▒░░░░░░░░░░░▒████████████████▒░░░░░▒▓▓░░░░░░░░▒▒▒▓░░░▒▓█████████████████████████████████████████████████
    ████████▒░░░░░░░▒███████████████████░░░░░░░▒▒▒░░░░░░░░░▓█▒▒████████████████████████████████████████████████████
    █████████▓░░▒▓█████████████████████▓░░░░░░░░▒░░░░░░░░░░▓▒██████████████████████████████████████████████████████
    ███████████████████████████████████░░░░░░▓█▓▓▓▒▒▓▒▒▒▒▒▓░░▓█████████████████████████████████████████████████████
    ██████████████████████████████████▓░░░░░░▒▓▓▓▓▓▓██▓█▓█▓█▒░▓████████████████████████████████████████████████████
    ██████████████████████████████████▒░░░▒▓░░░░░▒▒█▓▒▓▓███████▓███████████████████████████████████████████████████
    ██████████████████████████████████░░░░▒█▒░░░░░░░░░░▓█████████▓█████████████████████████████████████████▓▓▓▓▓▓██
    █████████████████████████████████▒░░░░▒█▒░░░░░░░░░░░▓▒██▓█▓████▓███████████████████████████████████▓▓▓▓▓▓█▓▓▓▓▓
    █████████████████████████████████▒░░░░▒██░░░░░░░░░░▒▓▒██░█▒▓████████████████████████████████████▓▓██▓▓▓▓▓▓▓▓▓▓▓
    ███████████████████████████████▓░░░░░▓███▓░░░░░░░░░▒░░█░▒▒░░░░▓███████████████████████████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
    ██████████████████████████████▒░░░░▒██████░░░░░░░░░░░█▓░░░░░░░░▓███████████████████████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ███████████████████████████████▓░▒███████▒░░░░░░░░▒▒▒█▓▓░░░░░░▒█▓██████████████████▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ████████████████████████████████████████▒░░░░░░░░░░▓██▓░░░░░░▒▓▓█████████████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    █████████████████████████▓███████████████░░░░░░░▒░▒██▒░░░░░░░▒▓▓███▓█▓██▓▓▓▓▓▓▓█▓▓█▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ████████████████████████▓▓████▓█████████▒░░░░░░░░▒▒░░░░░░░░░░▓███▓█▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ████████████████████▓▓▓▓▓▓▓▓█▓▓██████████▓▒░▓▓▒▓▓░░░░░░░░░░░▓████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓██▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    █████████████████████▓▓▓▓▓▓▓█▓▓██▓▓███████▓▓▓▓░░░░░░░░░░░░░▒██▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    █████████████████▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓██▓▒░░░░░░░░░░░░░░░▒▒▒▒██▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓
    ██████████████████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓██▒▒▒░▒▒░░░▒░░░░▒▓▓▓▓▓▓▓██████▓▓▓▓▓▓██▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ████████████████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓███▒▒░░░░░░░░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓
    ██████████████████▓███▓██▓▓▓▓▓▓▓▓▓████▓▒▒▒▒░░░▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓██▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ██████████▓██████▓▓▓▓▓█▓▓▓▓▓█▓▓▓▓▓▓▓███▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ████████▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓███▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ██████▓▓▓▓▓▓▓▓▓▓▓▓▓██▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓██▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ██▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    █▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ▓▓▓▓▓▓▓▓▓██▓██▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    █▓██▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓████▓█▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
    ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓███▓▓▓▓
    ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓██▓▓▓▓▓▓▓▓
*/
}