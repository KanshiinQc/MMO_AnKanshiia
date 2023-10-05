extends Node2D
class_name FightMap

# Called when the node enters the scene tree for the first time.
func _ready():
	var fightTile = preload("res://Fight/FightTile.tscn")
	
	for i in 12:
		for y in 12:
			var instance = fightTile.instantiate()
			instance.position = Vector2(32 * i, 32 * y)
			add_child(instance)
			
			var instance2 = fightTile.instantiate()
			instance2.position = Vector2((32 * i) + 16, (32 * y) - 8)
			add_child(instance2)
			
			var instance3 = fightTile.instantiate()
			instance3.position = Vector2((32* i) + 16, (32 * y) + 8)
			add_child(instance3)
			
			var instance4 = fightTile.instantiate()
			instance4.position = Vector2(32 * i, (32 * y) + 16 )
			add_child(instance4)
			
	#for child in get_children():
		#if child is Node2D:
			#var node = child as Node2D
			#print(str(node.position.x) + ", " + str(node.position.y))
			
