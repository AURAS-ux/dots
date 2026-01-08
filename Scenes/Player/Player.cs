using System;
using System.Collections.Generic;
using dots.Utils;
using Godot;

namespace dots.Scenes.Player;
//tall military warrior 2d pixel art with minimum gear. face uncovered. grey hair.

public partial class Player : CharacterBody2D
{
   [ExportCategory("Visuals")] 
   [Export] public Sprite2D SpriteBody { get; set; }
   [ExportCategory("Movement")]
   [Export] public float MoveSpeed { get; set; } = 550f;

   [Export] public float SpeedMultiplier { get; set; } = 60;
   [ExportCategory("Animations")]
   [Export] public AnimationPlayer  AnimationPlayer { get; set; }
   
   private bool _attacking;
   private float _lockedAimAngle;
   private FacingDir _facingDir = FacingDir.RIGHT;
   
   public override void _Ready()
   {
	  GD.Print("Player ready");
	  if (SpriteBody == null)
	  {
		 GD.PrintErr("No sprite for body set");
		 throw new Exception("No sprite for body set in Player script");
	  }
	  AnimationPlayer.AnimationFinished += OnAnimationFinished;
	  AnimationPlayer.Play("idle",customSpeed:0.2f);
   }

   public override void _Process(double delta)
   {
	  var movement = GetMovementFromInput();
	  HandleRotation(movement);
	  HandleMovement(movement,delta);
	  
	  MoveAndSlide();
   }

   public override void _Input(InputEvent @event)
   {
	  if (@event.IsActionPressed("attack"))
	  {
		 Slash();
	  }
   }

   private void Slash()
   {
	  if (_attacking) return;
	  _attacking = true;
	  
	  GD.Print($"Slasing in direction {_facingDir}");
	  
	  switch (_facingDir)
	  {
		 case FacingDir.RIGHT:
			AnimationPlayer.Play("slash_right");
			break;
		 case FacingDir.LEFT:
			AnimationPlayer.Play("slash_left");
			break;
		 case FacingDir.UP:
			AnimationPlayer.Play("slash_up");
			break;
		 case FacingDir.DOWN:
			AnimationPlayer.Play("slash_down");
			break;
		 default:
			GD.PrintErr("Unhandled animation event for facing");
			break;
	  }
   }

   private void OnAnimationFinished(StringName animationName)
   {
	  _attacking = false;
   }

   private void HandleRotation(Vector2 input)
   {
	  if (input == Vector2.Zero)
		 return;

	  // Pick facing (robust for diagonal input)
	  FacingDir newFacing;
	  if (Mathf.Abs(input.X) > Mathf.Abs(input.Y))
		 newFacing = input.X < 0 ? FacingDir.LEFT : FacingDir.RIGHT;
	  else
		 newFacing = input.Y < 0 ? FacingDir.UP : FacingDir.DOWN;

	  _facingDir = newFacing;
   }

   private Vector2 GetMovementFromInput()
   {
	  return new(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
		 Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up"));
   }

   private void HandleMovement(Vector2 movement, double delta)
   {
	   if (_attacking)
	   {
		   Velocity = Vector2.Zero;
		   return;
	   }
	   
	   Velocity = movement  * MoveSpeed * (float)delta * SpeedMultiplier;
	   if (Input.IsActionPressed("move_up"))
	   {
		   AnimationPlayer.Play("walk_up");
	   }
	   else if (Input.IsActionPressed("move_down"))
	   {
		   AnimationPlayer.Play("walk_down");
	   }
	   else if (Input.IsActionPressed("move_left"))
	   {
		   AnimationPlayer.Play("walk_left");
	   }
	   else if (Input.IsActionPressed("move_right"))
	   {
		   AnimationPlayer.Play("walk_right");
	   }
	   else
	   {
		   AnimationPlayer.Play("idle",customSpeed:0.2f);
	   }
   }
}
