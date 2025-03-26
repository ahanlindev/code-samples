## Controls lifetime of the player stack display objects in the debug menu

extends Control
class_name DebugPlayerStackController

## Scene to use as the display ui for each player. Must be a [DebugPlayerDataDisplay] at the root.
@export var _display_scene: PackedScene

@onready var _display_container: Container = $DisplayContainer


func _ready() -> void:
	PlayerRoster.newRosterPlayer.connect(_on_player_joined)
	PlayerRoster.discardRosterPlayer.connect(_on_player_left)


func _on_player_joined(info: PlayerInfo) -> void:
	print("Debug: %s joined!" % info.entityName)
	var display := _display_scene.instantiate() as DebugPlayerDataDisplay;
	_display_container.add_child(display);
	display.initialize(info)


func _on_player_left(info: PlayerInfo) -> void:
	print("Debug: %s left!" % info.entityName)
	for display: DebugPlayerDataDisplay in _display_container.get_children():
		if info.entityName == display.playerInfo.entityName:
			display.queue_free()
			return
