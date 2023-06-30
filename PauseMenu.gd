extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


func _input(event):
	if event.is_action_pressed("Pause"):
		self.visible = !self.visible

func _on_Continue_pressed():
	self.visible = false


func _on_Quit_pressed():
	get_tree().quit()
