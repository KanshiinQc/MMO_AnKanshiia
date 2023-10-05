extends VBoxContainer
class_name ChatBox

@onready var chatLog = $RichTextLabel
@onready var inputLabel: Label = $HBoxContainer/Label
@onready var inputField: LineEdit = $HBoxContainer/LineEdit

var username = "User"

func _input(event):
	if(visible):
		if event is InputEventKey:
			if event.is_action_pressed("Key_Enter"):
				inputField.grab_focus()
			if event.is_action_pressed("Key_Escape"):
				inputField.release_focus()

func AddMessage(username, text):
	chatLog.text +=  "\n" + "[" + username + "]: " + text
		
func _on_line_edit_text_submitted(new_text):
	if(new_text.length() > 0):
		AddMessage(username, new_text)
		ServerAccessor.GetServerNode().RequestAddMessageToChat(new_text)
		inputField.text = ""
