# Pathfinder-Platformer
This Unity project is one of many that are used to teach my Game-U students game design. This unit focuses on implementing the behavior of the player's avatar in a simple 3D action platformer. This project is not a game, but it could be used as an ingredient for games.
## Lesson 1 - CharacterController
Project setup and creation of player proxy. CharacterController, Camera.main, and Cursor.lockState.
## Lesson 2 - Input System
Input System, action maps, actions, and bindings.
## Lesson 3 - Jump
Gravity and the "jump" action.
## Lesson 4 - Move (part 1)
Cinemachine Virtual Camera with an Orbital Transposer that follows the player's avatar and a Composer that looks at the avatar's head. Initial implementation just uses absolute directional movement (it ignores the camera).
## Lesson 5 - Move (part 2)
Camera relative movement. Quaternion.LookRotation, vector extension methods, Mathf.Atan2, and Euler angles.
## Lesson 6 - Move (part 3)
In-air movement that preserves momentum and enforces speed limits.
## Lesson 7 - Moving Platforms (part 1)
Collision detection of moving platforms with OnControllerColliderHit. When to use FixedUpdate versus Update. Ensure the platform uses the "animate physics" update mode.
## Lesson 8 - Moving Platforms (part 2)
Remove camera jittering when riding a platform. Add momentum when jumping from a moving platform.
## Lesson 9 - Wall running & jumping (part 1)
Collision detection of walls with with OnControllerColliderHit. Determine direction of wall run with Vector3.ProjectOnPlane and Vector3.Normalize. Override character's velocity when running on the wall.
## Lesson 10 - Wall running & jumping (part 2)
Jump off wall.
## Lesson 11 - Wall running & jumping (part 3)
Turn avatar towards direction of wall run.
## Lesson 12 - Double jump
Tracking states for normal jump and double jump.
## Lesson 13 - Dashing (part 1)
Add "dash" to Input Actions. CountDown helper class (includes countdown interval and cooldown interval).
## Lesson 14 - Dashing (part 2) & final thoughts
During a dash, disable steering and override velocity.
