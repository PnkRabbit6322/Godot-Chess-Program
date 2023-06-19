extends VBoxContainer

func _on_PromoteKnight_pressed():
	get_tree().paused = false
	get_tree().current_scene.promote_to = "Knight"
	get_tree().current_scene.promote()
	self.visible = false

func _on_PromoteRook_pressed():
	get_tree().paused = false
	get_tree().current_scene.promote_to = "Rook"
	get_tree().current_scene.promote()
	self.visible = false

func _on_PromoteBishop_pressed():
	get_tree().paused = false
	get_tree().current_scene.promote_to = "Bishop"
	get_tree().current_scene.promote()
	self.visible = false

func _on_PromoteQueen_pressed():
	get_tree().paused = false
	get_tree().current_scene.promote_to = "Queen"
	get_tree().current_scene.promote()
	self.visible = false
