extends Node2D
class_name WorldMap

# Called when the node enters the scene tree for the first time.
func _ready():
	var worldTile = preload("res://WorldTiles/WorldTile.tscn")
	
	for i in 14:
		for y in 10:
			var instance = worldTile.instantiate()
			instance.position = Vector2(32 * i, 32 * y)
			add_child(instance)
			
			var instance2 = worldTile.instantiate()
			instance2.position = Vector2((32 * i) + 16, (32 * y) - 8)
			add_child(instance2)
			
			var instance3 = worldTile.instantiate()
			instance3.position = Vector2((32* i) + 16, (32 * y) + 8)
			add_child(instance3)
			
			var instance4 = worldTile.instantiate()
			instance4.position = Vector2(32 * i, (32 * y) + 16 )
			add_child(instance4)
