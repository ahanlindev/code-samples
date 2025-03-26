## Displays the state stack for a given player in the debug menu

extends Control
class_name DebugPlayerDataDisplay

# Child Nodes
@onready var controllerNumText: Label = $Data/ControllerNum/Text
@onready var controllerNameText: Label = $Data/ControllerName/Text
@onready var rosterNumText: Label = $Data/RosterNum/Text
@onready var playerNameText: Label = $Data/PlayerName/Text
@onready var playerHpText: Label = $Data/PlayerHP/Text
@onready var stackText: Label = $Data/StateStack/Panel/Text

# Change-detection cache variables
var stackCount: int = -1
var topState: State = null
var playerHp: int = -1

# Persistent information
var playerInfo: PlayerInfo


func initialize(info: PlayerInfo):
	# set up references
	playerInfo = info
	
	# init displays for immutable values
	var playerEntity := info.playerEntity
	if playerEntity:
		controllerNumText.text = str(playerEntity.deviceNumber)
		controllerNameText.text = Input.get_joy_name(playerEntity.deviceNumber)
		rosterNumText.text = str(playerEntity.rosterNumber)
	playerNameText.text = info.entityName


# Poll for changes to mutable values
func _process(_delta):
	if playerInfo == null: return
	_update_state_stack()
	_update_hp()


func _update_state_stack() -> void:
	var stack :=             playerInfo.playerEntity.playerStateStack.stateStack
	var stackChanged: bool = (stack.size() != stackCount || stack.front() != topState)
	if (!stackChanged): return

	stackCount = stack.size()
	topState = stack.front()

	var displayString: String = ""

	# Create new lines for each state in the stack, growing downwards
	var lineNumber: int = stack.size() - 1
	for state: State in stack:
		var newLine: String = "%d. %s\n" % [lineNumber, _get_state_name(state)] # state with number label
		displayString = newLine + displayString
		lineNumber -= 1

	stackText.text = displayString


func _update_hp():
	if (playerInfo.hp == playerHp): return
	playerHp = playerInfo.hp
	playerHpText.text = str(playerHp)


## Retrieve name of state to display
func _get_state_name(state: State) -> String:
	var stateName: String
	if state is GameState_Connection:
		stateName = "GSC: %s" % state.gameState.get_script().resource_path.get_file()
	else:
		stateName = state.get_script().resource_path.get_file()
	return stateName.trim_suffix(".gd") # remove script suffix
	