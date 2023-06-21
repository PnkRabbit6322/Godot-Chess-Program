extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

onready var side_btn = self.get_node("Side")
onready var volume_silder = self.get_node("VolumeSlider")

# Called when the node enters the scene tree for the first time.
func _ready():
	volume_silder.min_value = 0.0001
	volume_silder.step = 0.0001
	volume_silder.max_value = 1

func _on_Start_pressed():
	print(get_tree().current_scene)
	get_tree().current_scene.placePieces(side_btn.get_item_text(side_btn.selected))
	self.visible = false
	get_node("CanvasLayer").visible = false

func _on_Settings_pressed():
	volume_silder.visible = !volume_silder.visible

func _on_Quit_pressed():
	get_tree().quit()

func _on_HSlider_drag_ended(value_changed):
	get_tree().current_scene.get_node("AudioPlayer").volume_db = log(volume_silder.value) * 20
