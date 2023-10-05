extends Node2D

func _on_area_2d_mouse_entered():
	$SelectedTile.visible = true

func _on_area_2d_mouse_exited():
	$SelectedTile.visible = false
	
func _on_area_2d_input_event(_viewport, event, _shape_idx):
	pass
	if(event.is_action_pressed("mouse_left_click")):
		#print(str(position.y) + "   " + str($MovePosition.global_position.y))
		ServerAccessor.GetServerNode().MovePlayer(get_global_mouse_position().x, get_global_mouse_position().y)
