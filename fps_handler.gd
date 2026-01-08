extends Node

const TIMER_LIMIT = 2.0
var timer = 0.0

func _process(delta: float) -> void:
	timer += delta
	if timer > TIMER_LIMIT:
		timer = 0.0
		print("FPS: "+str(Engine.get_frames_per_second()))
	DisplayServer.window_set_title("FPS: "+str(Engine.get_frames_per_second()))
