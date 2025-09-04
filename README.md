https://github.com/user-attachments/assets/30bcd7d3-4fcc-4850-a941-4b299903348d

# Doctor-Oct-Tentacle-Player-Procedural-Animation-Controller-
A Unity project featuring a tentacle-based player character driven entirely through procedural animation.
Instead of relying on traditional keyframed movement, the player’s motion is dynamically calculated in real-time, creating fluid, organic, and adaptive interactions with the environment.

👉 Current Stage: Tentacle locomotion is focused only on walking across terrain.
👉 Next Stage (Planned): Expand the system to support wall climbing and obstacle traversal using procedural tentacle anchors.

✨ Features

Procedural Tentacle Locomotion
Each tentacle finds its own anchor point in the environment and moves dynamically, adapting to uneven terrain.

Step Prediction with Physics Rays
Tentacles cast parabolic raycasts to predict where they should grab, mimicking realistic step placement.

Idle & Rest System
Tentacles reset naturally when the player is idle, maintaining a grounded and believable pose.

Adaptive Movement
Tentacle movement responds directly to player input (WASD/joystick) while maintaining balance.

Real-Time Foot Placement
No animations are baked — steps are generated on-the-fly depending on movement speed, terrain, and rotation.

Debug Gizmos
Visualize hit points and tentacle placement in the Unity editor for easier development & tuning.


Future Improvements

Add inverse kinematics (IK) for smoother tentacle bending.

Procedural climbing system using tentacle anchors.

Support for VR controllers as tentacle inputs.

Physics-based grabbing and object interaction.
