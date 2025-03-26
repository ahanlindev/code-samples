## Displays the game state stack in the debug menu

extends Control
class_name DebugGameStackDisplay

@onready var stackText: Label = $Data/StateStack/Panel/Text

var stackCount: int = -1;
var topState: GameState = null;


# Inefficient, but works. Can replace with signal listeners when appropriate
func _process(_delta):
	_update_data()


func _update_data() -> void:
	var stack := GameStateStack.gameStateStack
	var stackChanged: bool = (stack.size() != stackCount || stack.front() != topState)
	if (!stackChanged): return

	stackCount = stack.size()
	topState = stack.front()

	var displayString: String = ""

	# Create new lines for each state in the stack, growing downwards
	var lineNumber: int = stack.size() - 1
	for state: GameState in stack:
		var newLine: String = "%d. %s\n" % [lineNumber, _get_game_state_name(state)] # state with number label
		displayString = newLine + displayString
		lineNumber -= 1

	stackText.text = displayString


## Retrieve name of state based on filename of its script
func _get_game_state_name(state: GameState) -> String:
	var filename: String = state.get_script().resource_path.get_file()
	return filename.trim_suffix(".gd") # remove script suffix

