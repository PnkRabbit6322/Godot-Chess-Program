extends VBoxContainer

func _on_White_pressed():
	get_tree().current_scene.placePiecesWhite()
	self.visible = false

func _on_Black_pressed():
	get_tree().current_scene.placePiecesBlack()
	self.visible = false
