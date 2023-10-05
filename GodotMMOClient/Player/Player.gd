extends CharacterBody2D
class_name PlayerCharacter

@export var dbUsername: String
@export var peerID: int

var isLocalPlayer: bool
@export var isFighting: bool

#@export var items: Array[Item] = []
@onready var usernameLabel: Label = $Username

## Player Ingame Infos
@export var currentHealth: int
@export var maxHealth: int


func _ready():
	isLocalPlayer = peerID == multiplayer.multiplayer_peer.get_unique_id()
	if(isLocalPlayer):
		$Camera2D.enabled = true
	
	var hud = get_tree().root.get_node("HUD") as GUI
	hud.playerHealth.max_value = maxHealth
	hud.playerHealth.value = currentHealth
	usernameLabel.text = dbUsername


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta): 
	var hud = get_tree().root.get_node("HUD") as GUI
	if(isLocalPlayer):
		## TO CLEAN IT'S DISGUSTING
		hud.playerHealth.value = currentHealth
		var label = hud.playerHealth.get_node("Label") as Label
		label.text = str(currentHealth)
			
	
func _on_input_event(_viewport, event, _shape_idx):
	if(!isLocalPlayer && !isFighting):
		if(event is InputEventMouse):
			if(event.is_action_pressed("mouse_left_click")):
				print("Clicked")
				ServerAccessor.GetServerNode().RequestFightWithPlayer(name.to_int())

## FOR WASD MOVEMENT EXPERIMENTAL --> ADD IT IN PROCESS			
func get_input():
	var input_direction = Input.get_vector("Key_A", "Key_D", "Key_W", "Key_S")
	velocity = input_direction * 100
	if(velocity != Vector2(0,0)):
		ServerAccessor.GetServerNode().SendKeyInput(velocity)
	
	
