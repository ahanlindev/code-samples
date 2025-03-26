## singleton DebugManager class
extends Node

# Constants and Enums
## Input action used to toggle on and off the debug overlay
const TOGGLE_ACTION := "debug_toggle"

enum Flags {
	DISABLE_ENEMIES
}


# child references
## Canvas layer containing debug menu
@onready var debugCanvas: CanvasLayer = $DebugCanvas


# export variables
## Is debug functionality available?
@export var _enable_debug: bool = true


# private variables
var _debugFlags: Array[bool] = []


# signals
## Emitted when a debug flag changes. Parameters are the flag affected and its new value
signal debug_flag_changed(flag: Flags, new_value: bool)


# public methods
func debug_is_enabled() -> bool: return _enable_debug && (OS.has_feature("debug"))


## Is the passed-in debug flag active?
func get_flag(flag: Flags) -> bool:
	if !debug_is_enabled():
		return false
	if !DebugManager.Flags.values().has(flag):
		printerr("Attempted to get a debug flag that does not exist")
		return false
	return _debugFlags[flag]


func set_flag(flag: Flags, value: bool) -> void:
	if !debug_is_enabled():
		return
	if (!DebugManager.Flags.values().has(flag)):
		printerr("Attempted to initialize a debug flag that does not exist")
		return
	print("Setting debug flag %s to %s" % [Flags.keys()[flag], "true" if value else "false"])
	_debugFlags[flag] = value
	debug_flag_changed.emit(flag, value)


# private methods
func _init() -> void:
	if (!debug_is_enabled()):
		print("Debug mode disabled. Destroying Debug Menu")
		debugCanvas.queue_free()
		return
	for flag in Flags.values():
		_debugFlags.append(false)


func _input(event: InputEvent) -> void:
	if debug_is_enabled() and event.is_action_pressed(TOGGLE_ACTION):
		_toggle_debug_menu()


func _toggle_debug_menu():
	debugCanvas.visible = !debugCanvas.visible;

