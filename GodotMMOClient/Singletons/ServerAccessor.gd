extends Node


func GetServerNode():
	return get_tree().root.get_node("ServerNode") as Server
