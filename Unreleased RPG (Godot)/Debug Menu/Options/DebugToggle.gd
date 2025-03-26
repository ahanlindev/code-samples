class_name DebugToggle
extends Container

# child references
@onready var _label: Label = $Label
@onready var _button : Button = $Button

# private variables
var _debugFlag : DebugManager.Flags


# public methods
func initialize(flag: DebugManager.Flags):
	_debugFlag = flag

	# Use enum flag name as label  
	_label.text = DebugManager.Flags.keys()[_debugFlag]

	# Update appropriate debug flag when button is toggled 
	_button.toggled.connect(_on_button_toggled)

	# Keep syncrhonized with debug value even if it's set elsewhere
	DebugManager.debug_flag_changed.connect(_on_debug_flag_changed)
	
	# Initialize flag to match toggle on startup
	_on_button_toggled(_button.is_pressed())


# private methods
func _on_button_toggled(newValue: bool):
	DebugManager.set_flag(_debugFlag, newValue)
	
func _on_debug_flag_changed(flag: DebugManager.Flags, value: bool):
	if (flag == _debugFlag): _button.set_pressed_no_signal(value) # emitting signal would inf loop here