extends Node2D

var player:PlayerCharacter

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if(!player):
		var serverNode = get_tree().root.get_node("ServerNode") as ServerNode
		if (serverNode.connected_peers.size() > 0):
			player = serverNode.connected_peers[0]
	queue_redraw()
	
func _draw():
	if(player):
		if(player.currentPointPath.is_empty()):
			return
		else:
			draw_polyline(player.currentPointPath, Color.RED)
	
