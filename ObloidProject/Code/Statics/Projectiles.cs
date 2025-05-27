using Godot;
using static ObloidGame;
using System;
using Obloid.Code.Scripts.CustomNodes;

public static class Projectile {
    public static void SpawnProjectile(PackedScene projectileScene, Node parent, Vector3 targetPosition, Vector3 spawnPosition, CollisionObject3D excludeNode) {
        Node3D rocketInstance = (Node3D)projectileScene.Instantiate();
        parent.AddChild(rocketInstance);
        rocketInstance.LookAtFromPosition(spawnPosition, targetPosition);
        RayCast3D rayCast = rocketInstance.GetChild(1) as RayCast3D;
        if (rayCast != null && excludeNode != null) {
            rayCast.AddException(excludeNode);
        }
    }
    
    public static void HandleProjectiles(Node3D projectile, float projectileSpeedModifier, Node levelInstance) {
        if (!projectile.IsInsideTree()) return;
        projectile.GlobalPosition -= projectile.GlobalTransform.Basis.Z.Normalized() * projectileSpeedModifier;
        if (projectile.GetChildCount() > 1 && projectile.GetChild(1) is RayCast3D rayCast) {
            if (rayCast.IsColliding() && rayCast.GetCollider() is not CharacterBody3D) {
                Explode(rayCast.GetCollisionPoint(), 10f, 22f, levelInstance);
                projectile.QueueFree();
            }
        }
    }

    public static Node3D AcquireTarget(Node3D[] candidates, Vector3 origin, Vector3 forward, float maxRange, float angleDegrees) {
        Node3D bestTarget = null;
        float bestDistance = float.MaxValue;
        float minDotThreshold = Mathf.Cos(Mathf.DegToRad(angleDegrees));

        foreach (var node in candidates) {
            if (node is not RigidBody3D target) continue;
            Vector3 toTarget = (target.GlobalPosition - origin).Normalized();
            float dot = forward.Dot(toTarget);
            float distance = (origin - target.GlobalPosition).Length();

            if (dot >= minDotThreshold && distance <= maxRange) {
                if (distance < bestDistance) {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }
        }
        return bestTarget;
    }
    
    public static void Explode(Vector3 targetPosition, float radius, float power, Node levelInstance) {
        var nodes = levelInstance.GetTree().GetNodesInGroup("Targetable");
        foreach (Node3D node in nodes) {
            Node3D entity = node;
            float distance = (entity.GlobalPosition - targetPosition).Length();
            if (distance > radius) { continue; }
            Vector3 direction = (entity.GlobalPosition - targetPosition).Normalized();
            direction = new Vector3(direction.X, Mathf.Max(0, direction.Y), direction.Z);
            if (entity is RigidBody3D rigidbody) {
                rigidbody.ApplyImpulse((direction * power) * 4+ Vector3.Up * 10f);
            }
            if (entity is CharacterBody3D character) {
                character.Velocity += direction * power + Vector3.Up * power/1.6f;
            }
            if (entity is ObloidMandrake Obloid) {
                Obloid.Health -= 32f;
                Damage(entity, 32);
            }
        }
    }
}