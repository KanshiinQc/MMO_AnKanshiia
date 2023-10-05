extends Node2D
class_name Ore

@export var networkUID: int
@onready var animation: AnimatedSprite2D = $AnimatedSprite2D
@onready var oreRespawnTimer: Timer = $Timer

func _ready():
	networkUID = get_instance_id()
	name = str(networkUID)
	animation.frame = 1
	oreRespawnTimer.wait_time = randi_range(20,60)

func TryMine():
	var wasMined = false
	if(animation.frame == 1):
		animation.frame = 0
		wasMined = true
		oreRespawnTimer.start()
	return wasMined


func _on_timer_timeout():
	animation.frame = 1
	oreRespawnTimer.wait_time = randi_range(20,60)
	oreRespawnTimer.start()
