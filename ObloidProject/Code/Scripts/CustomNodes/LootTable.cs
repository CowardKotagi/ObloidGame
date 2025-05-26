using Obloid.Code.Scripts.Data;
using Godot;
using Godot.Collections;

//Make Base class that inherits from this class.
namespace Obloid.Code.Scripts.CustomNodes
{
    
	[GlobalClass]
	public partial class LootTable : Node
	{
		public enum SpawnState
		{
			AtObjectPosition,
			FlingFromObject,
			AroundObject
		}
		
		[ExportCategory("Item Drop Properties")]
		[Export]
        public Array<Item> Items { get; set; } = new Array<Item>();
		
		[ExportCategory("Item Drop Specifics")]
		[Export(PropertyHint.None, hintString: "Use Array Element Numbers to Specifically Spawn Desired Items.")]
		private int[] SpecificItemsToDrop { get; set; }
		
		[ExportCategory("Spawning Properties")] 
		[Export]
		private float _spawnForce;
		
		[Export(PropertyHint.None, hintString: "Set Spawning Type, If Spawn")]
		SpawnState Spawntype { get; set; } = SpawnState.AtObjectPosition;

		private Array<Item> _itemsToDrop;
		
		private RandomNumberGenerator _rng = new RandomNumberGenerator();

		private Node3D _parentNode;

		public override void _Ready()
		{
			_itemsToDrop = new Array<Item>();
			//At Ready Collect items for Object/Enemy/Store to return.
			_parentNode = GetParent<Node3D>();
			GatherItems();
			
			base._Ready();
		}
		
		protected void Spawn(Item item, Vector3 position)
		{

			CharacterBody3D player = GetNode<GameModeDungeon>(GetTree().CurrentScene.GetPath()).Players[0];
			
		    switch (Spawntype)
		    {
		        case SpawnState.AtObjectPosition:
		            // Spawn the item at the specified position and apply a small upward force
		            Node instanceAtPosition = item.Scene.Instantiate();
		            
		            if (instanceAtPosition is Node3D node3D)
		            {
		                // Set position (slightly above ground at y=1)
		                node3D.Position = new Vector3(position.X, 5, position.Z);
		                
		                // Add to the scene tree
		                GetTree().CurrentScene.AddChild(instanceAtPosition);
		                //GD.Print($"Spawned item '{item.Name}' at position: {node3D.Position}");

		                // Apply a small upward force if the item is a RigidBody3D
		                if (instanceAtPosition is RigidBody3D rigidBody)
		                {
		                    Vector3 upwardForce = Vector3.Up * _spawnForce * 0.5f; // Small upward force (half of ForceScale)
		                    rigidBody.ApplyImpulse(upwardForce, Vector3.Zero);
		                   // GD.Print($"Applied upward force to '{item.Name}': {upwardForce}");
		                }
		                else
		                {
		                    GD.Print($"Item '{item.Name}' is not a RigidBody3D; no force applied.");
		                }
		            }
		            else
		            {
		                GD.Print($"Item '{item.Name}' scene is not a Node3D; cannot spawn.");
		                instanceAtPosition.QueueFree(); // Clean up if instantiation fails
		            }
		            break;

		        case SpawnState.FlingFromObject:
		            // Spawn the item and fling it in the direction the player is looking
		            Node instanceFling = item.Scene.Instantiate();
		            
		            if (instanceFling is Node3D node3DFling)
		            {
		                // Set initial position
		                node3DFling.Position = new Vector3(position.X, 1, position.Z);
		                
		                // Add to the scene tree
		                GetTree().CurrentScene.AddChild(instanceFling);
		              // GD.Print($"Spawned item '{item.Name}' at position: {node3DFling.Position}");

		                // Check if we have a valid PlayerNode
		                if (_parentNode == null || !IsInstanceValid(_parentNode))
		                {
		                    GD.Print("PlayerNode is not assigned or invalid; cannot fling item.");
		                    break;
		                }
		                // Get the player's forward direction (assuming PlayerNode faces in its -Z direction)
		                Vector3 playerLookDirection = _parentNode.Position - player.Position;
		                
		                playerLookDirection.Y = 0.2f; // Add a slight upward component to simulate an arc
		                playerLookDirection = playerLookDirection.Normalized();

		                // Apply force if the item is a RigidBody3D
		                if (instanceFling is RigidBody3D rigidBodyFling)
		                {
		                    Vector3 flingForce = playerLookDirection * _spawnForce;
		                    rigidBodyFling.ApplyImpulse(flingForce, Vector3.Zero);
		                    GD.Print($"Flinging item '{item.Name}' in direction {playerLookDirection} with force: {flingForce}");
		                }
		                else
		                {
		                    GD.Print($"Item '{item.Name}' is not a RigidBody3D; cannot apply fling force.");
		                }
		            }
		            else
		            {
		                GD.Print($"Item '{item.Name}' scene is not a Node3D; cannot spawn.");
		                instanceFling.QueueFree();
		            }
		            break;

		        case SpawnState.AroundObject:
		            // Spawn the item at a random position around the object within a radius
		            Node instanceAround = item.Scene.Instantiate();
		            
		            if (instanceAround is Node3D node3DAround)
		            {
		                // Generate a random offset in a circle around the position
		                float radius = 2.0f; // Radius around the object (adjustable)
		                float randomAngle = _rng.RandfRange(0f, Mathf.Pi * 2); // Random angle in radians (0 to 2π)
		                float offsetX = Mathf.Cos(randomAngle) * radius;
		                float offsetZ = Mathf.Sin(randomAngle) * radius;
		                
		                // Set position around the object
		                Vector3 spawnPosition = new Vector3(position.X + offsetX, 1, position.Z + offsetZ);
		                node3DAround.Position = spawnPosition;
		                
		                // Add to the scene tree
		                GetTree().CurrentScene.AddChild(instanceAround);
		                GD.Print($"Spawned item '{item.Name}' around position: {node3DAround.Position}");

		                // Apply a small random force if the item is a RigidBody3D
		                if (instanceAround is RigidBody3D rigidBodyAround)
		                {
		                    Vector3 randomForceDirection = new Vector3(
		                        _rng.RandfRange(-1f, 1f),
		                        0.5f, // Slight upward component
		                        _rng.RandfRange(-1f, 1f)
		                    ).Normalized();
		                    Vector3 force = randomForceDirection * _spawnForce * 0.3f; // Smaller force for scattering
		                    rigidBodyAround.ApplyImpulse(force, Vector3.Zero);
		                    GD.Print($"Applied random force to '{item.Name}': {force}");
		                }
		                else
		                {
		                    GD.Print($"Item '{item.Name}' is not a RigidBody3D; no force applied.");
		                }
		            }
		            else
		            {
		                GD.Print($"Item '{item.Name}' scene is not a Node3D; cannot spawn.");
		                instanceAround.QueueFree();
		            }
		            break;
		    }
		}	

		protected void GatherItems()
		{
			//Get a seed
			_rng.Randomize();
			
			//Check if Items is less than 0 before adding to item list.
			if (Items.Count > 0)
			{
				//Get a random number between min and max desire. unless both are set to zero.
		
				// get a random number between 0 and 100
				int diceRoll = _rng.RandiRange(0, 100);
			
				//normalize roll to a percentage.
				float normalizedRoll = diceRoll / 100.0f;
				
				//Iterate through each item and fill the array with elements to the random amount created above.
				
				for (int i = 0; i < Items.Count; i++)
				{
					//GD.Print("SpawnChance :", Items[i].SpawnChance);
					//if item spawn chance is greater or equal to the normalized roll then add it to the items to drop list.
					if (Items[i].SpawnChance >= normalizedRoll)
					{
						//Check to see if the scene is not null.
						if (Items[i].Scene is null)
						{
							GD.Print("Item was not spawned. item container has no scene. Array Element : ", i);
							continue;
						}
						else
						{
							_itemsToDrop.Add(Items[i]);
						}
					}
				}
				
			}
			
			else
			{
				GD.Print("Item count is less then 0, Add items in the loot table node on the object, enemy, or store front.");
			}
			
		}
		
		/// <summary>
		/// Get Spawned Items. This is for iterating specific spawning functionality.
		/// </summary>
		public void GetItemList(out Array<Item> items)
		{
			if (_itemsToDrop.Count == 0)
			{
				GD.Print("Item list is empty, No Items to return because none have been gathered. Check items in lootTable Node. ");
			}
			
			items = _itemsToDrop;
		}
		
		/// <summary>
		/// Spawn Items physically in the world.
		/// </summary>
		public void SpawnItems(Vector3 position, Quaternion rotation)
		{
			GD.Print("Amount of items: ", _itemsToDrop.Count);
			
			if (_itemsToDrop.Count > 0)
			{
				//iterate through each object and spawn the object.
				for (int i = 0; i < _itemsToDrop.Count; i++)
				{
					Spawn(_itemsToDrop[i], position);
				}
			}
			else
			{
				GD.Print("Item List is empty. Check items in lootTable Node");
			}
		}
		
		
	}
}