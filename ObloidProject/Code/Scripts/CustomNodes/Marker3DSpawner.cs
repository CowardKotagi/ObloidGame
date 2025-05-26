using Godot;
using Godot.Collections;

namespace Obloid.Code.Scripts.CustomNodes;



/// <summary>
/// A Custom Marker Class which is used to as a marker for objects to spawn on.
/// </summary>
[GlobalClass]
public partial class Marker3DSpawner : Marker3D
{
    public bool _isUsed { get; set; } = false;
    
    Array<Vector3> UsedPositions { get; set; } = new Array<Vector3>();
    public bool isUsed { get; set; }

    void AddUsedPosition(Vector3 position)
    {
        UsedPositions.Add(position);
    }

    Vector3 GetRandomPosition(float Radius)
    {
        //Spawn Raycast that hits the ground. to check if the floor is a walkable surface.
        if (Radius > 0)
        {
            
        }

        float angle = (float)GD.RandRange(0, Mathf.Pi * 2); // Random Angle 
        float r = (float)GD.RandRange(0, Radius);
        float x = r*Mathf.Cos(angle);
        float z = r*Mathf.Sin(angle);
        
        Vector3 positionResult = new Vector3(Position.X + x, Position.Y, Position.Z + z);
        
        AddUsedPosition(positionResult);
        
        return positionResult;
        
       
    }
}