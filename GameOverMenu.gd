extends Control

onready var side_btn = self.get_node("Side")
onready var endgame_text = self.get_node("CanvasLayer/EndgameText")

var endgame_text_whiteWon = preload("res://Assets/WhiteWon.png")
var endgame_text_blackWon = preload("res://Assets/BlackWon.png")
var endgame_text_whiteResign = preload("res://Assets/WhiteResign.png")
var endgame_text_blackResign = preload("res://Assets/BlackResign.png")
var endgame_text_drawStalemate = preload("res://Assets/Stalemate.png")
var endgame_text_drawAgreement = preload("res://Assets/Draw.png")
var endgame_text_drawRepetition = preload("res://Assets/Repetition.png")

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _on_Quit_pressed():
	get_tree().quit()

func _on_Continue_pressed():
	get_tree().current_scene._ready()
	get_tree().current_scene.get_node("/root/Main/Board")._ready()
	get_tree().current_scene.placePieces(side_btn.get_item_text(side_btn.selected))
	self.visible = false
	get_node("CanvasLayer").visible = false

func endGame(type):
	if type == "whiteWon":
		endgame_text.texture = endgame_text_whiteWon
	if type == "blackWon":
		endgame_text.texture = endgame_text_blackWon
	if type == "whiteResign":
		endgame_text.texture = endgame_text_whiteResign
	if type == "whiteResign":
		endgame_text.texture = endgame_text_blackResign
	if type == "stalemate":
		endgame_text.texture = endgame_text_drawStalemate
	if type == "draw":
		endgame_text.texture = endgame_text_drawAgreement
	if type == "repetition":
		endgame_text.texture = endgame_text_drawRepetition
		
func show():
	self.visible = true
	get_node("CanvasLayer").visible = true
