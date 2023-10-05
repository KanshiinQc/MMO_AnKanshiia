extends Node2D


func _on_area_2d_mouse_entered():
	$SelectedTile.visible = true

func _on_area_2d_mouse_exited():
	$SelectedTile.visible = false


func _on_area_2d_input_event(_viewport, event, _shape_idx):
	if(event.is_action_pressed("mouse_left_click")):
		ServerAccessor.GetServerNode().AttackTile(position.x, position.y)
	
