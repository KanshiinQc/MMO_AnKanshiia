extends Node2D
class_name Ore

@export var networkUID: int
@onready var clickableArea : Area2D = $Area2D
# Called when the node enters the scene tree for the first time.

func _on_area_2d_input_event(_viewport, event, _shape_idx):
	if(event is InputEventMouse):
		if(event.is_action_pressed("mouse_left_click")):
			print("Clicked")
			ServerAccessor.GetServerNode().MineOre(networkUID)
