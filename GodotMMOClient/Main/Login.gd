extends Control
class_name Login

@export var username: String
@export var password: String

func _on_button_login_pressed():
	username = $Username.text
	password = $Password.text
	ServerAccessor.GetServerNode().TryLogin(username, password)

func _on_button_register_pressed():
	username = $Username.text
	password = $Password.text
	ServerAccessor.GetServerNode().TryRegister(username, password)


func _on_password_text_submitted(_new_text):
	username = $Username.text
	password = $Password.text
	ServerAccessor.GetServerNode().TryLogin(username, password)


func _on_username_text_submitted(_new_text):
	username = $Username.text
	password = $Password.text
	ServerAccessor.GetServerNode().TryLogin(username, password)

func AutoLogin(username, password):
	ServerAccessor.GetServerNode().TryLogin(username, password)
