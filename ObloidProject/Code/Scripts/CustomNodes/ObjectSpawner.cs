using Obloid.Code.Scripts.Data;

using Godot;
using Godot.Collections;

namespace Obloid.Code.Scripts.CustomNodes;

//KILL THE MONSTERS FEED THE CHILDREN STAY ALIVE!!!!
[GlobalClass]
public partial class ObjectSpawner : Node3D
{
    [Export (PropertyHint.None, "A List Of Objects to Spawn in the world on markers.")]
    Array<SpawnObject> Objects = new Array<SpawnObject>();

    [Export (PropertyHint.None, "If Radius is greater then 0 the the spawner will spawn objects in a radius around the marker.")]
    private float _offsetRadius;

    [Export] private int SpawnAmount;
    
    Array<Marker3DSpawner> _markers = new Array<Marker3DSpawner>();
    
    RandomNumberGenerator _rng;
    
    public override void _Ready()
    {
        //get all markers in scene.
        GetAllChildMarkers();
        
        SpawnMultipleObjects();
        
        base._Ready();
    }

    void GetAllChildMarkers()
    {
        //Get all node children
        Array<Node> children = GetChildren();

        //iterate through each child
        foreach (Node child in children)
        {
            //Check if child  is a marker 3D.
            if (child is Marker3DSpawner marker)
            {
                //if so, add to the array.
                _markers.Add(marker);
            }
        }
    }

    bool HasPostionBeenUsedBefore()
    {
        return false;
    }
    
    void SpawnMultipleObjects()
    {
        for (int i = 0; i < SpawnAmount; i++)
        {
            if (_markers[i]._isUsed)
            {
                continue;
            }
            else
            {
                Vector3 position = GetARandomMarkerPosition();
                
                Node anObject = Objects[i].SpawningObject.Instantiate();

                if (anObject is Node3D node3D)
                {
                    node3D.Position = position;
                    
                    GetTree().CurrentScene.GetNode("Entities").CallDeferred("add_child",anObject);
                }
            }
        }
    }
    
    void SpawnSingleObject()
    {
        if (_offsetRadius != 0)
        {
            // get a position in a radius around the marker.
        }
        
    }
    Vector3 GetARandomMarkerPosition()
    {
        for (int i = 0; i < _markers.Count; i++)
        {
            if (!_markers[i].isUsed)
            {
                _markers[i].isUsed = true;
                return  _markers[i].Position;
            }
        }
        
        return Vector3.Zero;
        //if we are here no markers were gathered.
    }

    public void ResetMarkers()
    {
        foreach (Marker3DSpawner marker in _markers)
        {
            marker.isUsed = false;
        }
    }
    
    /// <summary>
    /// Manually Spawn a scene with a manual radius around a random marker in scene.
    /// </summary>
    void ManualSpawn(PackedScene objectToSpawn,float radius)
    {
        Node instanceAtPosition = objectToSpawn.Instantiate();

        if (instanceAtPosition is Node3D node3D)
        {
            node3D.Position = GetARandomMarkerPosition();
            
            GetTree().CurrentScene.AddChild(instanceAtPosition);
        }
    }
}