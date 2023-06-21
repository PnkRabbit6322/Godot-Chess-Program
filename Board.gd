extends Node2D

var King_White = preload("res://King_White.tscn")
var King_Black = preload("res://King_Black.tscn")
var Queen_White = preload("res://Queen_White.tscn")
var Queen_Black = preload("res://Queen_Black.tscn")
var Bishop_White = preload("res://Bishop_White.tscn")
var Bishop_Black = preload("res://Bishop_Black.tscn")
var Rook_White = preload("res://Rook_White.tscn")
var Rook_Black = preload("res://Rook_Black.tscn")
var Knight_White = preload("res://Knight_White.tscn")
var Knight_Black = preload("res://Knight_Black.tscn")
var Pawn_White = preload("res://Pawn_White.tscn")
var Pawn_Black = preload("res://Pawn_Black.tscn")

var sound_capture = preload("res://Assets/Sound/capture.mp3")
var sound_move = preload("res://Assets/Sound/move-self.mp3")
var sound_check = preload("res://Assets/Sound/move-check.mp3")
var sound_castle = preload("res://Assets/Sound/castle.mp3")

var king_white
var king_black
var queen_white
var queen_black
var bishop_white_1
var bishop_white_2
var bishop_black_1
var bishop_black_2
var rook_white_1
var rook_white_2
var rook_black_1
var rook_black_2
var knight_white_1
var knight_white_2
var knight_black_1
var knight_black_2
var pawn_white = []
var pawn_black = []

onready var board = $Board
onready var audio_player = get_node("AudioPlayer")

var global_cells_pos = []

var tile_pos_cur
var tile_pos_prev

var promote_piece = null
var promote_to = null
var selected_piece = null
var cur_piece = null

#false -> black turn
#true -> white turn
var turn_token := true #white move first

var play_as

var castle_rook = []

signal checkPromote

func play_sound(sound):
	if sound == "check":
		audio_player.stream = sound_check
	if sound == "move":
		audio_player.stream = sound_move
	if sound == "capture":
		audio_player.stream = sound_capture
	if sound == "castle":
		audio_player.stream = sound_castle
	audio_player.play()

func init() -> void:
	pawn_white.resize(8)
	pawn_black.resize(8)
	
	king_white = King_White.instance()
	king_white.set_name("king_white")
	king_white.add_to_group("pieces")
	king_white.add_to_group("white_pieces")
	king_white.add_to_group("kings")
	
	king_black = King_Black.instance()
	king_black.set_name("king_black")
	king_black.add_to_group("pieces")
	king_black.add_to_group("black_pieces")
	king_black.add_to_group("kings")
	
	queen_white = Queen_White.instance()
	queen_white.set_name("queen_white")
	queen_white.add_to_group("pieces")
	queen_white.add_to_group("white_pieces")
	queen_white.add_to_group("queens")
	
	queen_black = Queen_Black.instance()
	queen_black.set_name("queen_black")
	queen_black.add_to_group("pieces")
	queen_black.add_to_group("black_pieces")
	queen_black.add_to_group("queens")
	
	bishop_white_1 = Bishop_White.instance()
	bishop_white_1.set_name("bishop_white_1")
	bishop_white_1.add_to_group("pieces")
	bishop_white_1.add_to_group("white_pieces")
	bishop_white_1.add_to_group("bishops")
	
	bishop_white_2 = Bishop_White.instance()
	bishop_white_2.set_name("bishop_white_2")
	bishop_white_2.add_to_group("pieces")
	bishop_white_2.add_to_group("white_pieces")
	bishop_white_2.add_to_group("bishops")
	
	bishop_black_1 = Bishop_Black.instance()
	bishop_black_1.set_name("bishop_black_1")
	bishop_black_1.add_to_group("pieces")
	bishop_black_1.add_to_group("black_pieces")
	bishop_black_1.add_to_group("bishops")
	
	bishop_black_2 = Bishop_Black.instance()
	bishop_black_2.set_name("bishop_black_2")
	bishop_black_2.add_to_group("pieces")
	bishop_black_2.add_to_group("black_pieces")
	bishop_black_2.add_to_group("bishops")
	
	rook_white_1 = Rook_White.instance()
	rook_white_1.set_name("rook_white_1")
	rook_white_1.add_to_group("pieces")
	rook_white_1.add_to_group("white_pieces")
	rook_white_1.add_to_group("rooks")
	
	rook_white_2 = Rook_White.instance()
	rook_white_2.set_name("rook_white_2")
	rook_white_2.add_to_group("pieces")
	rook_white_2.add_to_group("white_pieces")
	rook_white_2.add_to_group("rooks")
	
	rook_black_1 = Rook_Black.instance()
	rook_black_1.set_name("rook_black_1")
	rook_black_1.add_to_group("pieces")
	rook_black_1.add_to_group("black_pieces")
	rook_black_1.add_to_group("rooks")
	
	rook_black_2 = Rook_Black.instance()
	rook_black_2.set_name("rook_black_2")
	rook_black_2.add_to_group("pieces")
	rook_black_2.add_to_group("black_pieces")
	rook_black_2.add_to_group("rooks")
	
	knight_white_1 = Knight_White.instance()
	knight_white_1.set_name("knight_white_1")
	knight_white_1.add_to_group("pieces")
	knight_white_1.add_to_group("white_pieces")
	knight_white_1.add_to_group("knights")
	
	knight_white_2 = Knight_White.instance()
	knight_white_2.set_name("knight_white_2")
	knight_white_2.add_to_group("pieces")
	knight_white_2.add_to_group("white_pieces")
	knight_white_2.add_to_group("knights")
	
	knight_black_1 = Knight_Black.instance()
	knight_black_1.set_name("knight_black_1")
	knight_black_1.add_to_group("pieces")
	knight_black_1.add_to_group("black_pieces")
	knight_black_1.add_to_group("knights")
	
	knight_black_2 = Knight_Black.instance()
	knight_black_2.set_name("knight_black_2")
	knight_black_2.add_to_group("pieces")
	knight_black_2.add_to_group("black_pieces")
	knight_black_2.add_to_group("knights")
	
	for i in range(pawn_white.size()):
		pawn_white[i] = Pawn_White.instance()
		pawn_white[i].set_name("pawn_white_" + str(i))
		pawn_white[i].add_to_group("pieces")
		pawn_white[i].add_to_group("white_pieces")
		pawn_white[i].add_to_group("pawns")
	for i in range(pawn_black.size()):
		pawn_black[i] = Pawn_Black.instance()
		pawn_black[i].set_name("pawn_black_" + str(i))
		pawn_black[i].add_to_group("pieces")
		pawn_black[i].add_to_group("black_pieces")
		pawn_black[i].add_to_group("pawns")
		
func placePieces(side) -> void:
	play_as = side
	
	var used_cells = board.get_used_cells()
	for i in used_cells:
		global_cells_pos.append(board.map_to_world(i))
		
	add_child(rook_black_1)
	rook_black_1.global_position = global_cells_pos[0] + board.cell_size * 0.5
	add_child(knight_black_1)
	knight_black_1.global_position = global_cells_pos[1] + board.cell_size * 0.5
	add_child(bishop_black_1)
	bishop_black_1.global_position = global_cells_pos[2] + board.cell_size * 0.5
	add_child(queen_black)
	queen_black.global_position = global_cells_pos[3] + board.cell_size * 0.5
	add_child(king_black)
	king_black.global_position = global_cells_pos[4] + board.cell_size * 0.5
	add_child(bishop_black_2)
	bishop_black_2.global_position = global_cells_pos[5] + board.cell_size * 0.5
	add_child(knight_black_2)
	knight_black_2.global_position = global_cells_pos[6] + board.cell_size * 0.5
	add_child(rook_black_2)
	rook_black_2.global_position = global_cells_pos[7] + board.cell_size * 0.5
	for i in range(pawn_black.size()):
		add_child(pawn_black[i])
		pawn_black[i].global_position = global_cells_pos[i+8] + board.cell_size * 0.5
		
	add_child(rook_white_1)
	rook_white_1.global_position = global_cells_pos[56] + board.cell_size * 0.5
	add_child(knight_white_1)
	knight_white_1.global_position = global_cells_pos[57] + board.cell_size * 0.5
	add_child(bishop_white_1)
	bishop_white_1.global_position = global_cells_pos[58] + board.cell_size * 0.5
	add_child(queen_white)
	queen_white.global_position = global_cells_pos[59] + board.cell_size * 0.5
	add_child(king_white)
	king_white.global_position = global_cells_pos[60] + board.cell_size * 0.5
	add_child(bishop_white_2)
	bishop_white_2.global_position = global_cells_pos[61] + board.cell_size * 0.5
	add_child(knight_white_2)
	knight_white_2.global_position = global_cells_pos[62] + board.cell_size * 0.5
	add_child(rook_white_2)
	rook_white_2.global_position = global_cells_pos[63] + board.cell_size * 0.5
	for i in range(pawn_white.size()):
		add_child(pawn_white[i])
		pawn_white[i].global_position = global_cells_pos[i+48] + board.cell_size * 0.5
	board.visible = true
	board.gen_bitboard()
	board.gen_legal_move(0)

func _ready():
	init()
	
	promote_piece = null
	promote_to = null
	selected_piece = null
	cur_piece = null
	turn_token = true #white move first
	
	resetBoard()
	
	# warning-ignore:return_value_discarded
	connect("checkPromote", self, "checkPromote")

func resetBoard():
	var flip = true
	for x in range(0, 8):
		flip = !flip
		for y in range(0, 8):
			if flip:
				board.set_cellv(Vector2(x, y), 0)
			else:
				board.set_cellv(Vector2(x, y), 1)
			flip = !flip

func showEndGame(side, type):
	for node in get_tree().get_nodes_in_group("pieces"):
		node.queue_free()
	board.visible = false
	if type == "stalemate":
		get_node("CanvasLayer/GameOverMenu").endGame("stalemate")
	if type == "checkmate":
		if side == 0:
			get_node("CanvasLayer/GameOverMenu").endGame("blackWon")
		if side == 1:
			get_node("CanvasLayer/GameOverMenu").endGame("whiteWon")
	get_node("CanvasLayer/GameOverMenu").show()

func enablePromoteMenu(piece):
	promote_piece = piece
	get_tree().paused = true
	self.get_node("CanvasLayer/PromoteMenu").visible = true
		
func highlightSquare(pos) -> void:
	var tile_index = board.get_cellv(pos)
	
	if tile_index == 0:
		board.set_cellv(pos, 2)
	if tile_index == 1:
		board.set_cellv(pos, 3)

func checkTile(pos):
	for node in get_tree().get_nodes_in_group("pieces"):
		if board.world_to_map(node.global_position) == pos:
			if node.is_in_group("white_pieces"):
				return 1
			if node.is_in_group("black_pieces"):
				return 2
	return 0

func tileHasPiece(pos):
	for node in get_tree().get_nodes_in_group("pieces"):
		if board.world_to_map(node.global_position) == pos:
			cur_piece = node
			if node.is_in_group("white_pieces"):
				return 1
			if node.is_in_group("black_pieces"):
				return 2
	return 0

func deletePiece(pos):
	for node in get_tree().get_nodes_in_group("pieces"):
		if board.world_to_map(node.global_position) == pos:
			node.global_position = Vector2(9999, 9999);
	
func checkTurn() -> bool:
	if cur_piece != null:
		if turn_token == true && cur_piece.is_in_group("white_pieces"):
			return true
		elif turn_token == false && cur_piece.is_in_group("black_pieces"):
			return true
	return false

func _input(event):
	if event.is_action_pressed("mouse_click"):
		mouseEvent()
		
func mouseEvent():
	var mouse_pos = get_viewport().get_mouse_position()
	var tile_pos = board.world_to_map(mouse_pos)
	var global_tile_pos = board.map_to_world(tile_pos) + board.cell_size * 0.5
	
	tile_pos_prev = tile_pos_cur
	tile_pos_cur = board.world_to_map(global_tile_pos)
	
	if selected_piece != null:
		resetBoard()
	
	if tileHasPiece(tile_pos_cur) != 0:
		highlightSquare(tile_pos_cur)
		if checkTurn():
			selected_piece = cur_piece
			board.showLegalMove(selected_piece)
		else:
			if !board.make_move(selected_piece, board.pos_to_square(tile_pos_cur)):
				return
			emit_signal("checkPromote")
			turn_token = !turn_token
			if turn_token:
				board.gen_legal_move(0)
				board.check_endgame(0)
			elif !turn_token:
				board.gen_legal_move(1)
				board.check_endgame(1)
			selected_piece = null
			
			#playMove(board.alphaBetaRoot(depth, isMaximizePlayer))
			resetBoard()
	elif selected_piece != null:
		if checkTurn():
			if !board.make_move(selected_piece, board.pos_to_square(tile_pos_cur)):
				return
			emit_signal("checkPromote")
			turn_token = !turn_token
			if turn_token:
				board.gen_legal_move(0)
				board.check_endgame(0)
			elif !turn_token:
				board.gen_legal_move(1)
				board.check_endgame(1)
			selected_piece = null
			#playMove(board.alphaBetaRoot(depth, isMaximizePlayer))
			resetBoard()
	else: resetBoard()

class RemovedPiece:
	var piece: Node2D
	var pos: Vector2
	var first_move: bool
	
	func _init(a, b, c):
		self.piece = a
		self.pos = b
		self.first_move = c

func checkPromote():
	if play_as == "White":
		if selected_piece.is_in_group("pawns"):
			if selected_piece.is_in_group("white_pieces"):
				if selected_piece.global_position.y == 32:
					enablePromoteMenu(selected_piece)
			if selected_piece.is_in_group("black_pieces"):
				if selected_piece.global_position.y == 480:
					enablePromoteMenu(selected_piece)
	elif play_as == "Black":
		if selected_piece.is_in_group("pawns"):
			if selected_piece.is_in_group("white_pieces"):
				if selected_piece.global_position.y == 480:
					enablePromoteMenu(selected_piece)
			if selected_piece.is_in_group("black_pieces"):
				if selected_piece.global_position.y == 32:
					enablePromoteMenu(selected_piece)

func promote():
	var new_piece
	if promote_piece.is_in_group("white_pieces"):
		if promote_to == "Knight":
			new_piece = Knight_White.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("white_pieces")
			new_piece.add_to_group("knights")
		elif promote_to == "Rook":
			new_piece = Rook_White.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("white_pieces")
			new_piece.add_to_group("rooks")
		elif promote_to == "Bishop":
			new_piece = Bishop_White.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("white_pieces")
			new_piece.add_to_group("bishops")
		elif promote_to == "Queen":
			new_piece = Queen_White.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("white_pieces")
			new_piece.add_to_group("queens")
	if promote_piece.is_in_group("black_pieces"):
		if promote_to == "Knight":
			new_piece = Knight_Black.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("black_pieces")
			new_piece.add_to_group("knights")
		elif promote_to == "Rook":
			new_piece = Rook_Black.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("black_pieces")
			new_piece.add_to_group("rooks")
		elif promote_to == "Bishop":
			new_piece = Bishop_Black.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("black_pieces")
			new_piece.add_to_group("bishops")
		elif promote_to == "Queen":
			new_piece = Queen_Black.instance()
			new_piece.global_position = promote_piece.global_position
			new_piece.add_to_group("pieces")
			new_piece.add_to_group("black_pieces")
			new_piece.add_to_group("queens")

	add_child(new_piece)
	promote_piece.queue_free()

func _process(_delta):
#	if play_as == "White" && !turn_token:
#		playRandom()
#	elif play_as == "Black" && turn_token:
#		playRandom()
	pass
