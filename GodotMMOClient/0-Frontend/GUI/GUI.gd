extends Node
class_name GUI

@onready var chatBox: ChatBox = $ChatBox
@onready var playerHealth: ProgressBar = $PlayerHealth
@onready var lootWindow: VBoxContainer = $LootWindow
@onready var loadingAnimation: AnimatedSprite2D = $Loading
@onready var currentPlayerTurnLabel: Label = $CurrentPlayerTurn

func Notify(message):
	$Notifications.text = message


func _on_loot_window_gui_input(event):
	if(event is InputEventMouse):
		if(event.is_action_pressed("mouse_right_click")):
			if(lootWindow.visible):
				lootWindow.visible = false
				for loot in lootWindow.get_children():
					loot.queue_free()
		
func ShowLoading():
	loadingAnimation.visible = true
	
func HideLoading():
	loadingAnimation.visible = false
	
func ShowFightTurns():
	currentPlayerTurnLabel.visible = true

func HideFightTurns():
	currentPlayerTurnLabel.visible = false

func UpdateTurnLabel(playerName: String):
	currentPlayerTurnLabel.text = "[" + playerName + "]'s turn"
