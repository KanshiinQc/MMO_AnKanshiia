extends Node2D
class_name ServerNode

## DB VARIABLES ##
var db = SQLite.new()
var dbName

## NETWORK VARIABLES ##
var network = ENetMultiplayerPeer.new()
var port = 1909
var max_player = 100

## NETWORK PEER INFOS ##
var connected_peers : Array[PlayerCharacter]
var fight1v1Instances: Array[Fight1v1]

## CONTAINERS ##
@onready var playersContainer : Node = $PlayersContainer
@onready var resourcesContainer : Node = $ResourcesContainer
@onready var fightsContainer: Node = $FightsContainer
var worldMap

## SERVER STARTING ##
func _ready():
	StartServer()	
func StartServer():
	CreateServer()
	SetupDBPath()
	SpawnWorldMap()
	SpawnResources()
	#LoadInitialItems() ## TO INJECT ITEMS IN DATABASE FIRST TIME
	print("Server Started")
func SetupDBPath():
	var ipAdd = IP.resolve_hostname(str(OS.get_environment("COMPUTERNAME")),1)
	print("IP ADD: " + ipAdd)
	if(ipAdd.contains("192.168.40") || ipAdd.contains("172.30.240.1") ):
		dbName = "res://0-Backend/DBLayer/GodotMMO_Local.db3"
	else:
		dbName = "/game/GodotMMO.db3"
func CreateServer():
	network.create_server(port, max_player)
	multiplayer.multiplayer_peer = network
	network.connect("peer_connected", _Peer_Connected)
	network.connect("peer_disconnected", _Peer_Disconnected)	
func SpawnWorldMap():
	var mapScene = preload("res://Maps/WorldMap.tscn")
	worldMap = mapScene.instantiate()
	add_child(worldMap, true)
func SpawnResources():
	var startingX = 16
	var startingY = 4
	var spaceBetween = 32
	for n in 4:
		for m in 4:
			if(m % 2 == 1):
				var ore = preload("res://Mining/Ore.tscn")
				var oreInstance =  ore.instantiate()
				oreInstance.position = Vector2(startingX + (n * spaceBetween), startingY + (m * spaceBetween))
				resourcesContainer.add_child(oreInstance, true)

## !!FOR TESTING!! TO DISABLE ##	
var connectedPeers = 0
@rpc("any_peer")
func AutomaticallyConnectPeer(playerID):
	if(connectedPeers == 0):
		rpc_id(playerID, "AutomaticallyConnectPeer", playerID, "Paul")
	if(connectedPeers == 1):
		rpc_id(playerID, "AutomaticallyConnectPeer", playerID, "Jonathan")
	if(connectedPeers == 2):
		rpc_id(playerID, "AutomaticallyConnectPeer", playerID, "Michael")
	if(connectedPeers == 3):	
		rpc_id(playerID, "AutomaticallyConnectPeer", playerID, "Blanche")
	connectedPeers += 1
## !!FOR TESTING!! TO DISABLE ##


## PEERS CONNECTION FUNCTION ##
func _Peer_Connected(player_id):
	AutomaticallyConnectPeer(player_id)
		
func _Peer_Disconnected(player_id):
	connectedPeers -= 1
	var player = playersContainer.get_node(str(player_id)) as Node2D
	if(player):
		SavePlayerState(player)
		connected_peers.remove_at(connected_peers.find(player))
		player.queue_free()
	print("User " + str(player_id) + " Disconnected")

@rpc("any_peer")
func ConnectPlayer(username):
	var playerScene = preload("res://Player/Player.tscn")
	var playerInstance = playerScene.instantiate() as PlayerCharacter
	connected_peers.append(playerInstance)
	
	var playerID = multiplayer.get_remote_sender_id()
	playerInstance.name = str(playerID)
	playerInstance.peerID = playerID;
	
	var playerInfos = GetUserInfos(username)
	playerInstance.dbID = int(playerInfos["ID"])
	playerInstance.dbUsername = playerInfos["Username"].capitalize()
	playerInstance.dbPassword = playerInfos["Password"]
	
	var playerState = GetPlayerState(playerInstance)
	playerInstance.position.x = playerState["PlayerDisconnectedPosX"]
	playerInstance.position.y = playerState["PlayerDisconnectedPosY"]
	
	playerInstance.maxHealth = 100
	playerInstance.currentHealth = playerInstance.maxHealth
	
	#GetPlayerItems(playerInstance)
	
	playersContainer.add_child(playerInstance, true)

@rpc("any_peer")
func TryLogin(username, password):
	var playerID = multiplayer.get_remote_sender_id()
	
	if(AccountCredentialsAreValid(username, password)):
		rpc_id(playerID, "PlayerConnectionSuccess", "Connection Success")
	else:
		rpc_id(playerID, "PlayerConnectionFailure", "Connection Failure")

@rpc("any_peer")
func TryRegister(username, password):
	var playerID = multiplayer.get_remote_sender_id()
	if(username.length() < 8):
		rpc_id(playerID, "PlayerRegisterFailure", "You Need At Least 8 Characters For The Username")
	elif(password.length() < 8):
		rpc_id(playerID, "PlayerRegisterFailure", "You Need At Least 8 Characters For The Password")
	elif(UsernameAlreadyExists(username)):
		rpc_id(playerID, "PlayerRegisterFailure", "Username Already Exists")
	else:
		CreateAccount(username, password)
		rpc_id(playerID, "PlayerRegisterSuccess", "Account Created, Please Connect")

@rpc("any_peer")
func PlayerConnectionSuccess(_message):
	pass
	
@rpc("any_peer")
func PlayerConnectionFailure(_message):
	pass

@rpc("any_peer")
func PlayerRegisterSuccess(_message):
	pass
	
@rpc("any_peer")
func PlayerRegisterFailure(_message):
	pass
	

## DB FUNCTIONS - LOGIN/REGISTER ##
func AccountCredentialsAreValid(username, password):
	db.path = dbName
	db.open_db()
	var table_name = "Players"
	var query = "SELECT username FROM " + table_name + " WHERE username = '" + username + "' AND Password = '" + password + "'"
	db.query(query)
	return db.query_result.size() > 0

func UsernameAlreadyExists(username):
	db.path = dbName
	db.open_db()
	var table_name = "Players"
	var query = "SELECT username FROM " + table_name + " WHERE username = '" + username + "'"
	db.query(query)
	return db.query_result.size() > 0

func CreateAccount(username, password):
	db.path = dbName
	db.open_db()
	var table_name = "Players"
	var dict: Dictionary = Dictionary()
	dict["username"] = username
	dict["password"] = password
	db.insert_row(table_name, dict)

## DB FUNCTIONS - PLAYER STATE ##
func GetUserInfos(username):
	db.path = dbName
	db.open_db()
	var table_name = "Players"
	var query = "SELECT id, username, password FROM " + table_name + " WHERE username = '" + username + "'"
	db.query(query)
	return db.query_result[0]
		
func SavePlayerState(player: PlayerCharacter):
	db.path = dbName
	db.open_db()
	var table_name = "Players"
	var query = "UPDATE " + table_name + " SET PlayerDisconnectedPosX = " + str(player.position.x) + ", PlayerDisconnectedPosY = " + str(player.position.y) + " WHERE ID = " + str(player.dbID)	
	db.query(query)
	
func GetPlayerState(player: PlayerCharacter):
	db.path = dbName
	db.open_db()
	var table_name = "Players"
	var query = "SELECT PlayerDisconnectedPosX, PlayerDisconnectedPosY FROM " + table_name + " WHERE ID = " + str(player.dbID)
	db.query(query)
	return db.query_result[0]

func GetPlayerItems(player: PlayerCharacter):
	db.path = dbName
	db.open_db()
	var table_name = "PlayerItems"
	var table_name_joint = "Items"
	var query = "SELECT Items.Name, PlayerItems.Quantity FROM " + table_name + " INNER JOIN " + table_name_joint + " ON " + table_name + ".ItemID = " + table_name_joint + ".ID WHERE PlayerID = " + str(player.dbID)
	db.query(query)
	for result in await db.query_result:
		var item = Item.new()
		item.itemName = result["Name"]
		item.quantity = result["Quantity"]
		player.items[item.itemName] = item.quantity
		add_child(item)

## DB FUNCTIONS - ITEM TRANSACTIONS
func PlayerAlreadyHasAQuantityOfThisItemID(playerID, itemID):
	db.path = dbName
	db.open_db()
	var table_name = "PlayerItems"
	var query = "SELECT PlayerID FROM " + table_name + " WHERE PlayerID = " + str(playerID) + " AND ItemID = " + str(itemID) 
	db.query(query)
	return db.query_result.size() > 0
	
func AddItemQtyToPlayer(playerID, itemID, quantity):
	for i in connected_peers:
		if i.peerID == playerID:
			var player = i
			db.path = dbName
			db.open_db()
			var table_name = "PlayerItems"
			if(PlayerAlreadyHasAQuantityOfThisItemID(player.dbID, itemID)):
				var query = "UPDATE " + table_name + " SET Quantity = Quantity + " + str(quantity) + " WHERE PlayerID = " + str(player.dbID) + " AND ItemID = " + str(itemID)
				db.query(query)
			else:	
				var dict: Dictionary = Dictionary()
				dict["PlayerID"] = player.dbID
				dict["ItemID"] = itemID
				dict["Quantity"] = quantity
				db.insert_row(table_name, dict)

func GiveRandomItemsToPlayer(playerID):
	for i in 10:
		var itemID = randi_range(0, 300)
		var quantity = randi_range(2, 30)
		AddItemQtyToPlayer(playerID, itemID, quantity)

## DB FUNCTIONS - ITEMS MISC ##
func LoadInitialItems():
	var itemNames:Array[String]    
	for filePath in DirAccess.get_files_at("res://Items/Sprites"):  
		if filePath.get_extension() == "png":  
			itemNames.append(filePath.get_basename())
	
	db.path = dbName
	db.open_db()
	var table_name = "Items"
	for itemName in itemNames:
			var dict: Dictionary = Dictionary()
			dict["Name"] = itemName
			db.insert_row(table_name, dict)
	
	
## PLAYER ACTIONS FUNCTIONS ##			
@rpc("any_peer")
func MovePlayer(xPos, yPos):
	print(str(multiplayer.get_remote_sender_id()) + "CALLED MovePlayer" + " To Position: " + str(xPos) + "," + str(yPos))
	var playerID = multiplayer.get_remote_sender_id()
	var player = playersContainer.get_node(str(playerID)) as PlayerCharacter
	
	var idPath
	
	if player.isMoving:
		idPath = player.astarGrid.get_id_path(
		player.tileMap.local_to_map(player.targetPosition),
		player.tileMap.local_to_map(Vector2i(xPos, yPos))
		).slice(1)
		
	else:
		idPath = player.astarGrid.get_id_path(
		player.tileMap.local_to_map(player.global_position),
		player.tileMap.local_to_map(Vector2i(xPos, yPos))
		).slice(1)
	print (idPath)
	
	
	
	if(!idPath.is_empty()):
		player.currentIdPath = idPath
		player.currentPointPath = player.astarGrid.get_point_path(
			player.tileMap.local_to_map(player.targetPosition),
			player.tileMap.local_to_map(Vector2i(xPos, yPos))
		)
	
@rpc("any_peer")	
func MineOre(networkUID):
	var playerID = multiplayer.get_remote_sender_id()
	var oreToMine = resourcesContainer.get_node(str(networkUID))
	if(oreToMine.TryMine()):
		var quantity = randi_range(10, 30)
		var thread = Thread.new()
		thread.start(AddItemQtyToPlayer.bind(playerID, 0, quantity))
		thread.wait_to_finish()
		rpc_id(playerID, "NotifyPlayer", str(quantity) + " ores added to inventory")
	else:
		rpc_id(playerID, "NotifyPlayer", "Cannot mine this ore yet..")	

@rpc("any_peer", "unreliable")
func SendKeyInput(velocity: Vector2):
	var player = GetPlayerByID(multiplayer.get_remote_sender_id())
	player.velocity = velocity
		
		
## FIGHT FUNCTIONS ##
@rpc("any_peer")
func RequestFightWithPlayer(playerID):
	var requesterID = multiplayer.get_remote_sender_id()
	var requesterPlayer = GetPlayerByID(requesterID)
	
	var fightScene = preload("res://Fight/Fight1v1.tscn")
	var fightInstance = fightScene.instantiate()
	fight1v1Instances.append(fightInstance)
	fightInstance.player1ID = requesterID
	fightInstance.player2ID = playerID
	fightInstance.currentTurnPlayerID = requesterID
	
	for player in connected_peers:
		if(player.peerID == playerID):
			var player1 = player
			player1.isFighting = true
			var fightPosition = Vector2(208, 208)
			player1.position = fightPosition
			player1.navAgent.target_position = fightPosition
			rpc_id(requesterID, "NotifyPlayer", "Started fight with [" + player1.dbUsername + "]")
			rpc_id(requesterID, "StartFightWithPlayer")
		if(player.peerID == requesterID):
			var player2 = player
			player2.isFighting = true
			var fightPosition = Vector2(208, 224)
			player2.position = fightPosition
			player2.navAgent.target_position = fightPosition
			rpc_id(playerID, "NotifyPlayer", "Started fight with [" + player2.dbUsername + "]")
			rpc_id(playerID, "StartFightWithPlayer")
			
	rpc_id(fightInstance.player1ID, "UpdateFightTurn", requesterPlayer.dbUsername)
	rpc_id(fightInstance.player2ID, "UpdateFightTurn", requesterPlayer.dbUsername)
	

	fightsContainer.add_child(fightInstance, true)

@rpc("any_peer")
func StartFightWithPlayer():
	pass

@rpc("any_peer")
func AttackTile(positionX, positionY):
	var requesterID = multiplayer.get_remote_sender_id()
	
	var fightInstance = GetFightInstanceByPlayerID(requesterID)
	if(fightInstance.currentTurnPlayerID != requesterID):
		rpc_id(requesterID, "NotifyPlayer", "This is not your turn to play!")
		return
		
	var playerAttacked: PlayerCharacter
	var playerAttacking: PlayerCharacter
	
	if(requesterID == fightInstance.player1ID):
		playerAttacked = GetPlayerByID(fightInstance.player2ID)
		playerAttacking = GetPlayerByID(fightInstance.player1ID)
	else:
		playerAttacked = GetPlayerByID(fightInstance.player1ID)
		playerAttacking = GetPlayerByID(fightInstance.player2ID)
		
	if(playerAttacked.position.x == positionX && playerAttacked.position.y == positionY - 8):
		var damage = 50
		playerAttacked.currentHealth = playerAttacked.currentHealth - damage
		
		var notifyMessage = playerAttacking.dbUsername + " attacked " + playerAttacked.dbUsername + " for " + str(damage)
		rpc_id(playerAttacked.peerID, "NotifyPlayer", notifyMessage)
		rpc_id(playerAttacking.peerID, "NotifyPlayer", notifyMessage)
		
		if (playerAttacked.currentHealth <= 0):
			playerAttacked.currentHealth = 0
			CompleteFightInstance(fightInstance)
		else:
			fightInstance.currentTurnPlayerID = playerAttacked.peerID
			var newTurnPlayer = GetPlayerByID(playerAttacked.peerID)
			rpc_id(fightInstance.player1ID, "UpdateFightTurn", newTurnPlayer.dbUsername)
			rpc_id(fightInstance.player2ID, "UpdateFightTurn", newTurnPlayer.dbUsername)
	else:
		rpc_id(requesterID, "NotifyPlayer", "Nothing to Hit on this tile!")

@rpc("any_peer")
func UpdateFightTurn(_username: String):
	pass

func CompleteFightInstance(fightInstanceToComplete: Fight1v1):
	rpc_id(fightInstanceToComplete.player1ID, "TerminateFight", fightInstanceToComplete.player1PositionBeforeFight)
	var player1Loot: Dictionary = GenerateLoot()
	rpc_id(fightInstanceToComplete.player1ID, "ShowLoot", player1Loot)
	
	rpc_id(fightInstanceToComplete.player2ID, "TerminateFight", fightInstanceToComplete.player2PositionBeforeFight)
	var player2Loot: Dictionary = GenerateLoot()
	rpc_id(fightInstanceToComplete.player2ID, "ShowLoot", player2Loot)
	
	var player1 = GetPlayerByID(fightInstanceToComplete.player1ID)
	player1.isFighting = false
	player1.currentHealth = player1.maxHealth
	
	var player2 = GetPlayerByID(fightInstanceToComplete.player2ID)
	player2.isFighting = false
	player2.currentHealth = player2.maxHealth
	
	fight1v1Instances.remove_at(fight1v1Instances.find(fightInstanceToComplete))
	fightInstanceToComplete.queue_free()

@rpc("any_peer")
func TerminateFight(_playerInitialPosition: Vector2):
	pass
	
@rpc("any_peer")
func ShowLoot(_lootDictionnary: Dictionary):
	pass
		

## CHAT & NOTIFS FUNCTIONS ##
@rpc("any_peer")
func AddMessageToChat(_message):
	pass
	
@rpc("any_peer")	
func RequestAddMessageToChat(message):
	var requesterID = multiplayer.get_remote_sender_id()
	var requesterPlayer = GetPlayerByID(requesterID) as PlayerCharacter
	for player in connected_peers:
		if(player != requesterPlayer):
			rpc_id(player.peerID, "AddMessageToChat", requesterPlayer.dbUsername, message)

@rpc("any_peer")
func NotifyPlayer(_message):
	pass


## UTILITIES ##
func GetFightInstanceByPlayerID(playerID):
	for fight in fight1v1Instances:
		if (fight.player1ID == playerID || fight.player2ID == playerID):
			return fight

func GetPlayerByID(playerID):
	for player in connected_peers:
		if player.peerID == playerID:
			return player

func GenerateLoot():
	var lootDictionnary: Dictionary
	for i in 4:
		var itemID = randi_range(1, 200)
		var quantity = randi_range(1,5)
		lootDictionnary[itemID] = quantity	
	return lootDictionnary

func _process(delta):
	queue_redraw()

func _draw():
	if connected_peers.size() > 0:
		var player = GetPlayerByID(connected_peers[0].peerID)
		for x in player.tileMap.get_used_rect().size.x:
			for y in player.tileMap.get_used_rect().size.y:
				var tilePosition = Vector2i(
					x + player.tileMap.get_used_rect().position.x,
					y + player.tileMap.get_used_rect().position.y
					)
				var tileData = player.tileMap.get_cell_tile_data(0, tilePosition)
				if tileData == null || not tileData.get_custom_data("WALKABLE"):
					draw_circle(tilePosition, 2.0, Color.RED) 
				else:
					draw_circle(tilePosition , 2.0, Color.GREEN)
