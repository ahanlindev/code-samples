class_name DebugOptionsMenu
extends Node

## Template scene used to generate debug toggles. Must be a [DebugToggle] script at root
@export var _toggleTemplate : PackedScene

@onready var _toggleContainer: Container = $Toggles


func _ready():
	# create a toggle for each button
	for flag : DebugManager.Flags in DebugManager.Flags.values():
		var toggle := _toggleTemplate.instantiate() as DebugToggle
		_toggleContainer.add_child(toggle)
		toggle.initialize(flag)
