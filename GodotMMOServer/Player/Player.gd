extends CharacterBody2D
class_name PlayerCharacter

@export var dbID: int
@export var dbUsername:String
@export var dbPassword:String
@export var peerID: int
@export var isFighting: bool
@export var items: Array[Item] = []

## Player Ingame Infos
@export var currentHealth: int
@export var maxHealth: int

var speed = 100
var accel = 15
var angle = 0 


#@onready var navAgent: NavigationAgent2D = $NavigationAgent2D
var astarGrid: AStarGrid2D
var currentIdPath: Array[Vector2i]
var currentPointPath: PackedVector2Array
var targetPosition: Vector2
var tileMap: TileMap 
var isMoving = false

var xClick: int
var yClick: int

# Called when the node enters the scene tree for the first time.
func _ready():
	#get_tree().get_nodes_in_group("")
	tileMap = get_tree().root.get_node("ServerNode").get_node("WorldMap").get_node("TileMap") as TileMap
	astarGrid = AStarGrid2D.new()
	astarGrid.cell_size = Vector2i(16,16)
	astarGrid.offset = Vector2(16,16)
	astarGrid.region = tileMap.get_used_rect()
	astarGrid.diagonal_mode = AStarGrid2D.DIAGONAL_MODE_ALWAYS
	astarGrid.update()
	print(tileMap.get_used_rect())
	
	var test = astarGrid.get_point_path(Vector2i(0,0), Vector2i(14,40))
	
	for x in tileMap.get_used_rect().size.x:
		for y in tileMap.get_used_rect().size.y:
			var tilePosition = Vector2i(
				x + tileMap.get_used_rect().position.x,
				y + tileMap.get_used_rect().position.y
				)
			var tileData = tileMap.get_cell_tile_data(0, tilePosition)
			if tileData == null || not tileData.get_custom_data("WALKABLE"):
				astarGrid.set_point_solid(tilePosition)
				
	

		
func _physics_process(delta):
	SetPlayerAnimation(angle)
	
	if(currentIdPath.is_empty()):
		return
	
	if !isMoving:
		isMoving == true
		targetPosition = tileMap.map_to_local(currentIdPath.front())
		global_position = global_position.move_toward(targetPosition, 1.5)
	
	var tan : Vector2 = targetPosition - global_position
	var angle_rad = atan2(tan.y, tan.x)
	angle = rad_to_deg(angle_rad) + 180
	
	if(global_position == targetPosition):
		currentIdPath.pop_front()
		if(!currentIdPath.is_empty()):
			targetPosition = tileMap.map_to_local(currentIdPath.front())
		else:
			isMoving = false

func SetPlayerAnimation(movingAngle):
	if(isMoving):
		if(movingAngle >= 337.5 || movingAngle < 22.5):
			$AnimatedSprite2D.play("WALK_L")
		elif(movingAngle >= 22.5 && movingAngle < 67.5):
			$AnimatedSprite2D.play("WALK_U_L")
		elif(movingAngle >= 67.5 && movingAngle < 112.5):
			$AnimatedSprite2D.play("WALK_U")
		elif(movingAngle >= 112.5 && movingAngle < 157.5):
			$AnimatedSprite2D.play("WALK_U_R")
		elif(movingAngle >= 157.5 && movingAngle < 202.5):
			$AnimatedSprite2D.play("WALK_R")
		elif(movingAngle >= 202.5 && movingAngle < 247.5):
			$AnimatedSprite2D.play("WALK_D_R")
		elif(movingAngle >= 247.5 && movingAngle < 292.5):
			$AnimatedSprite2D.play("WALK_D")
		elif(movingAngle >= 292.5 && movingAngle < 337.5):
			$AnimatedSprite2D.play("WALK_D_L")
	else:
		$AnimatedSprite2D.stop()
