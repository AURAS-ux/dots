using System;
using System.Collections.Generic;
using dots.Utils;
using Godot;

namespace dots.Scenes.Player;

public partial class Player : CharacterBody2D
{
   [ExportCategory("Visuals")] 
   [Export] public Sprite2D SpriteBody { get; set; }
   [ExportCategory("Movement")]
   [Export] public float MoveSpeed { get; set; } = 190f;
   
   [ExportCategory("Weapons")]
   [Export] public Node2D SwordNode { get; set; }

   [ExportCategory("SwingConfigs")]
   [Export] public float SlashArcDeg { get; set; } = 120f;

   [Export] public float SlashTime { get; set; } = 0.10f;
   [Export] public AnimationPlayer  AnimationPlayer { get; set; }

   private Dictionary<FacingDir, Node2D> _pivots;
   private Node2D _currentNode;
   
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
      
      _pivots = new()
      {
         { FacingDir.RIGHT, GetNode<Node2D>("WeaponPivotRight") },
         { FacingDir.LEFT , GetNode<Node2D>("WeaponPivotLeft")},
         { FacingDir.UP , GetNode<Node2D>("WeaponPivotUp")},
         { FacingDir.DOWN , GetNode<Node2D>("WeaponPivotDown")}
      };
      _currentNode = _pivots[FacingDir.RIGHT];
      
      AnimationPlayer.AnimationFinished += OnAnimationFinished;
   }

   public override void _Process(double delta)
   {
      var movement = GetMovementFromInput();
      HandleRotation(movement);
      HandleMovement(movement,delta);
      SpriteBody.Frame = (int)_facingDir;
      
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
            SpriteBody.ZIndex = 1;
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
      if(SpriteBody.ZIndex != 0)
         SpriteBody.ZIndex = 0;
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

      if (newFacing == _facingDir)
         return;

      _facingDir = newFacing;

      var newPivot = _pivots[_facingDir];
      _currentNode = newPivot;

      // Only reparent if needed
      if (SwordNode.GetParent() != newPivot)
      {
         SwordNode.Reparent(newPivot, keepGlobalTransform: false);
      }
   }

   private Vector2 GetMovementFromInput()
   {
      return new(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
         Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up"));
   }

   private void HandleMovement(Vector2 movement, double delta)
   {
      Velocity = movement  * MoveSpeed;
   }
}