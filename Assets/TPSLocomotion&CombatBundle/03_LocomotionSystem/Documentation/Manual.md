# Third-Person Locomotion & Camera System – User Manual

---

## 1. Overview

This package provides a modular third-person locomotion and camera system designed for fast prototyping and production use.

Main features:

* CharacterController-based movement
* Free-look third-person camera
* Lock-on targeting system
* Sprint and jump support
* Clean and extensible architecture
* Demo scene included (optional)

The core system has **no external dependencies** beyond Unity’s built-in modules.

---

## 2. Installation

1. Import the package into your Unity project.
2. (Optional) Open the demo scene:

LocomotionSystem/Demo/Scene/Locomotion_Demo

3. Press **Play** to test the system.

The demo content is optional and can be removed if not needed.

---

## 3. Folder Structure

LocomotionSystem/
│
├── Core/
│   ├── Scripts/
│   │   ├── Animator/
│   │   ├── Camera/
│   │   ├── Input/
│   │   ├── Interfaces/
│   │   └── Locomotion/
│
├── Demo/
│   ├── Animator/
│   ├── Model/
│   ├── Prefabs/
│   ├── Scene/
│   ├── Textures/
│   └── UI/
│
├── Settings/
│   └── InputActions/
│
└── Documentation/

The **Core** folder contains the runtime system.
The **Demo** folder is only for demonstration.

---

## 4. Using Without Demo

You may safely delete the entire `Demo` folder.

The runtime system is fully contained in:
LocomotionSystem/Core/

No demo assets are required for gameplay.

---

## 5. Input Setup

This package uses **Unity Input System**.

### If input does not work:

1. Install Input System via Package Manager (if not installed)
2. Open:
LocomotionSystem/Settings/InputActions/

3. Assign `LocomotionInputAction` to your PlayerInput component

Make sure the PlayerInput component is set to **Invoke Unity Events** or **Send Messages** depending on your setup.

---

## 6. Player Setup (Minimal)

Required components:

* CharacterController
* Animator
* PlayerLocomotion
* PlayerInputReader
* ThirdPersonCameraController

You can reference the prefab:
LocomotionSystem/Demo/Prefabs/DemoPlayer

---

## 7. Animator Requirements

Required Animator Parameters:

| Parameter        | Type  |
| ---------------- | ----- |
| Speed            | Float |
| MoveX            | Float |
| MoveZ            | Float |
| IsSprinting      | Bool  |
| IsLockOn         | Bool  |
| IsJumping        | Bool  |
| VerticalVelocity | Float |

Example Animator:
LocomotionSystem/Demo/Animator/Player_Locomotion_AC

You may replace the animations with your own.

---

## 8. Camera System

### Free Mode

* Mouse / right stick rotation
* Adjustable sensitivity
* Pitch clamping

### Lock-On Mode

* Automatic target tracking
* Camera alignment with target
* Target switching support

---

## 9. Lock-On Setup

Targets must use:

Layer:
LockOnTarget

Obstacles blocking camera or targeting should use:
Obstacle

---

## 10. Extending the System

The locomotion system exposes:

* Current velocity
* Grounded state
* Input direction
* Vertical velocity

You can extend this for:

* Dodge / Roll
* Combat system
* Root-motion blending
* Ability system

---

## 11. Known Limitations

* No ledge detection
* No climbing system
* Slopes above ~45° not supported by default
* Root-motion movement not included

---

## 12. Demo Content Notice

The Demo folder contains:

* Example character model
* Example animations
* Animator Controller
* UI and scene setup

These assets are provided **for demonstration only**.

The core locomotion and camera systems do **not depend** on any demo assets.

---

## 13. Third-Party Content

See:
Third-Party Notices.txt

for licensing information.

---

## 14. Support

This system is designed to be modular and easy to modify.

You are encouraged to customize it for your project needs.
