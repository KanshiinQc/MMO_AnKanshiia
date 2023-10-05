extends Node
class_name Server


## NETWORK VARIABLES ##
var network = ENetMultiplayerPeer.new()
#var ip = "170.187.179.212"
var ip = "127.0.0.1"
var port = 1909


## CONTAINERS ##
@onready var playersContainer : Node = $PlayersContainer
@onready var resourcesContainer : Node = $ResourcesContainer
@onready var fightsContainer: Node = $FightsContainer
@onready var worldMapNode = $WorldMap
@onready var fightMapNode = $FightMap


## HUD ##
var guiNode : GUI
var loginNode: Login

## ITEMS ##
var itemSprites: Dictionary


## INITIAL GAME STARTING ##

## FOR TESTING ##
@rpc("any_peer")
func AutomaticallyConnectPeer(_playerID, username):
	loginNode.username = username
	loginNode.password = ""
	TryLogin(loginNode.username, loginNode.password)
## FOR TESTING ##

## CONNECTING ##
func _ready():
	LoadItemSprites()
	LoadGUI()
	ConnectToServer()	
func LoadItemSprites():
	var number = 0 
	for filePath in DirAccess.get_files_at("res://Items/Sprites"):  
		if filePath.get_extension() == "png": 
			itemSprites[number] = "res://Items/Sprites/" + filePath.get_file()
			number += 1			
func LoadGUI():
	var guiStart = preload("res://0-Frontend/GUI/GUI.tscn")
	var GUIStartInstance = guiStart.instantiate()
	get_tree().root.add_child.call_deferred(GUIStartInstance)
	guiNode = GUIStartInstance
	loginNode = guiNode.get_node("Login") as Login
func ConnectToServer():
	NotifyPlayer("Attempting to connect to server. Please wait...")
	network.create_client(ip, port)
	multiplayer.multiplayer_peer = network
	multiplayer.connected_to_server.connect(_OnConnectionSucceeded)
	multiplayer.connection_failed.connect(_OnConnectionFailed)


func _OnConnectionFailed():
	guiNode.HideLoading()
	NotifyPlayer("Could not connect to server. Is it down ?")
	
func _OnConnectionSucceeded():
	guiNode.HideLoading()
	NotifyPlayer("Connected to server successfully. You can now log in")

@rpc("any_peer")
func ConnectPlayer():
	rpc_id(1, "ConnectPlayer", loginNode.username)

@rpc("any_peer")
func TryLogin(username, password):
	rpc_id(1, "TryLogin", username, password)

@rpc("any_peer")
func TryRegister(username, password):
	rpc_id(1, "TryRegister", username, password)

@rpc("any_peer")
func PlayerConnectionSuccess(message):
	ConnectPlayer()
	MakeGameWorldObjectsVisible()
	SetupChatBoxUI()
	SetupPlayerUI()
	NotifyPlayer(message)
	
@rpc("any_peer")
func PlayerConnectionFailure(message):
	NotifyPlayer(message)

@rpc("any_peer")
func PlayerRegisterSuccess(message):
	NotifyPlayer(message)
	
@rpc("any_peer")
func PlayerRegisterFailure(message):
	NotifyPlayer(message)
	

## SETUP UI / GAMEWORK VISIBILITY ##
func SetupChatBoxUI():
	guiNode.chatBox.visible = true
	guiNode.chatBox.username = loginNode.username
	guiNode.chatBox.inputLabel.text = "[" + guiNode.chatBox.username + "]"
	
func SetupPlayerUI():
	loginNode.queue_free()
	guiNode.playerHealth.visible = true

func MakeGameWorldObjectsVisible():
	worldMapNode.visible = true
	playersContainer.visible = true
	resourcesContainer.visible = true	

@rpc("any_peer", "unreliable")
func SendKeyInput(velocity: Vector2):
	rpc_id(1, "SendKeyInput", velocity)


	

# PLAYER ACTIONS FUNCTIONS ##		
@rpc("any_peer")
func MovePlayer(xPos, yPos):
	rpc_id(1, "MovePlayer", xPos, yPos)

@rpc("any_peer")
func MineOre(networkUID):
	rpc_id(1, "MineOre", networkUID)


## FIGHT FUNCTIONS ##	
@rpc("any_peer")
func RequestFightWithPlayer(playerID):
	rpc_id(1, "RequestFightWithPlayer", playerID)

@rpc("any_peer")
func StartFightWithPlayer():
	worldMapNode.visible = !worldMapNode.visible
	worldMapNode.set_process(!worldMapNode.is_processing())
	fightMapNode.visible = !fightMapNode.visible
	fightMapNode.set_process(!fightMapNode.is_processing())
	get_viewport().get_camera_2d().zoom = Vector2(3, 3)
	for resource in resourcesContainer.get_children():
		if resource is Ore:
			var res = resource as Ore
			res.visible = false
	guiNode.ShowFightTurns()

@rpc("any_peer")
func AttackTile(positionX, positionY):
	rpc_id(1, "AttackTile", positionX, positionY)

@rpc("any_peer")
func UpdateFightTurn(username: String):
	guiNode.UpdateTurnLabel(username)

@rpc("any_peer")
func TerminateFight(playerInitialPosition: Vector2):
	worldMapNode.visible = !worldMapNode.visible
	worldMapNode.set_process(!worldMapNode.is_processing())
	fightMapNode.visible = !fightMapNode.visible
	fightMapNode.set_process(!fightMapNode.is_processing())
	get_viewport().get_camera_2d().zoom = Vector2(4, 4)
	for resource in resourcesContainer.get_children():
		if resource is Ore:
			var res = resource as Ore
			res.visible = true
	var player = GetLocalPlayer() as PlayerCharacter
	player.position = playerInitialPosition
	guiNode.HideFightTurns()
	
@rpc("any_peer")
func ShowLoot(lootDictionnary: Dictionary):
	guiNode.lootWindow.visible = true
	for i in lootDictionnary.size():
		var itemUIScene = preload("res://Items/ItemUI.tscn")
		var lootImage = itemUIScene.instantiate()
		lootImage.scale = Vector2(4,4)
		lootImage.position.x = i * 64
		var filepath = itemSprites[lootDictionnary.keys()[i]]
		var imageLoad = load(itemSprites[lootDictionnary.keys()[i]])
		lootImage.texture = imageLoad
		var label = lootImage.get_node("Quantity") as Label
		label.text = str(lootDictionnary.values()[i])
		guiNode.lootWindow.add_child(lootImage, true)


## CHAT & NOTIFS FUNCTIONS ##
@rpc("any_peer")
func NotifyPlayer(message):
	guiNode.Notify(message)
	
@rpc("any_peer")
func RequestAddMessageToChat(message):
	rpc_id(1, "RequestAddMessageToChat", message)
	
@rpc("any_peer")	
func AddMessageToChat(username, message):
	guiNode.chatBox.AddMessage(username, message)


## PLAYER UTILITIES FUNCS
func GetLocalPlayer():
	for player in playersContainer.get_children():
		if player is PlayerCharacter:
			if player.isLocalPlayer:
				return player
				
func GetLocalPlayerFight():
	var localPlayer = GetLocalPlayer()
	for fight in fightsContainer.get_children():
			if fight is Fight1v1:
				if fight.player1ID == localPlayer.peerID || fight.player2ID == localPlayer.peerID:
					return fight

func GetPlayerByID(playerID):	
	for player in playersContainer.get_children():
		if player is PlayerCharacter:
			if player.peerID == playerID:
				return player
