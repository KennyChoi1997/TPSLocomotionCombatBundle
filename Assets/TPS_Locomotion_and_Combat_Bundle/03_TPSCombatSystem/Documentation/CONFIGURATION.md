# TPS Combat System – Configuration Guide

This document explains how to configure core components using the Unity Inspector.

---

## Quick Setup Tip

If you are unsure where to start:

1. Open the Demo Scene
2. Select a weapon (Shotgun / Pistol / Rifle)
3. Inspect `WeaponConfig`, `ShooterCore`, and `WeaponController`
4. Use these as reference when setting up your own character

## 1. WeaponConfig

Defines core weapon behavior and tuning values.

### General

| Field        | Description                                                         |
| ------------ | ------------------------------------------------------------------- |
| Damage       | Base damage per shot. **For shotguns, this is applied per pellet.** |
| Fire Mode    | Firing behavior (e.g., Semi Auto, Shotgun).                         |
| Fire Rate    | Shots per second.                                                   |
| Max Distance | Maximum hitscan distance (not gameplay range).                      |
| Hit Mask     | Layer mask for hit detection.                                       |

---

### Aim Constraint

| Field                        | Description                                                            |
| ---------------------------- | ---------------------------------------------------------------------- |
| Max Horizontal Aim Deviation | Horizontal clamp for camera-to-muzzle aiming. Set to **0 to disable**. |
| Max Vertical Aim Deviation   | Vertical clamp for camera-to-muzzle aiming. Set to **0 to disable**.   |

---

### Shotgun Settings

| Field            | Description                       |
| ---------------- | --------------------------------- |
| Pellet Count     | Number of pellets fired per shot. |
| Spread Angle Deg | Spread cone angle of pellets.     |

---

### Ammo

| Field            | Description           |
| ---------------- | --------------------- |
| Magazine Size    | Rounds per magazine.  |
| Max Reserve Ammo | Maximum carried ammo. |

---

### Reload

| Field       | Description                |
| ----------- | -------------------------- |
| Reload Time | Reload duration (seconds). |

---

## 2. HitscanWeapon

Represents a runtime weapon instance.

| Field        | Description                             |
| ------------ | --------------------------------------- |
| Muzzle       | Transform used as the firing origin.    |
| Config       | Reference to a `WeaponConfig`.          |
| Forward Axis | Defines weapon model forward direction. |

### Notes

* Forward Axis must match the weapon mesh orientation.
* Example: many demo weapons use `Minus X`.

---

## 3. ShooterCore

Central system handling firing logic.

### Dependencies

| Field                     | Description                        |
| ------------------------- | ---------------------------------- |
| Aim Provider Behaviour    | Provides aim ray (`IAimProvider`). |
| Damage Resolver Behaviour | Handles hit processing.            |
| Weapon Behaviour          | Current weapon (`IWeapon`).        |

---

### Direction Policy

| Field                   | Description            |
| ----------------------- | ---------------------- |
| Aiming Direction Mode   | Used while aiming.     |
| Hip Fire Direction Mode | Used while not aiming. |

#### Recommended

```text
Aiming Direction Mode: CameraAimToMuzzle
Hip Fire Direction Mode: TrueMuzzleForward
```

---

### Aim State

| Field               | Description              |
| ------------------- | ------------------------ |
| Combat Input Reader | Provides fire/aim input. |

---

### Debug

| Field          | Description                     |
| -------------- | ------------------------------- |
| Draw Debug Ray | Shows firing ray in Scene view. |

---

## 4. WeaponController

Handles weapon state and ammo logic.

### References

| Field            | Description                 |
| ---------------- | --------------------------- |
| Shooter          | Reference to `ShooterCore`. |
| Input Behaviour  | Fire input provider.        |
| Weapon Behaviour | Current weapon instance.    |
| Config           | Weapon configuration.       |

---

### Ammo Initialization

| Field                        | Description                 |
| ---------------------------- | --------------------------- |
| Start With Full Reserve Ammo | Fill reserve ammo at start. |
| Start Reserve Ammo           | Initial reserve ammo.       |

---

### Runtime State (Read Only)

| Field                | Description             |
| -------------------- | ----------------------- |
| Current Ammo         | Current magazine ammo.  |
| Current Reserve Ammo | Remaining reserve ammo. |
| Is Reloading         | Reload state.           |
| Fire Cooldown        | Current cooldown timer. |

---

## 5. Important Notes

### Shotgun Damage

* Applied **per pellet**
* Total damage = Damage × Pellet Count

---

### Max Distance

* Defines **maximum raycast distance**
* Not the recommended gameplay range

---

### Aim Deviation

Recommended default:

```text
Horizontal: 0
Vertical: 0
```

---

### Forward Axis

* Must match weapon model orientation
* Incorrect setting causes aim misalignment

---

## 6. Recommended Defaults

### Pistol

* Fire Mode: Semi Auto
* Aiming: CameraAimToMuzzle
* Hip Fire: TrueMuzzleForward
* Deviation: 0 / 0

---

### Shotgun

* Fire Mode: Shotgun
* Pellet Count: 10–12
* Spread Angle: 6–8

---

### Rifle

* Fire Mode: Semi / Full Auto
* Max Distance: higher than pistol
* Deviation: 0 / 0

---

## 7. Interfaces Overview

The TPS Combat System is built using an interface-based architecture.

These interfaces define how core systems communicate without tightly coupling implementations.

---

### IAimProvider

Provides an aim ray used for hit detection.

**Purpose:**
- Decouples aiming logic from camera implementation

**Example Implementation:**
- `CameraAimProvider` (screen-centre ray)
- `DualAimProvider` (switches between free look and aim camera)

---

### IFireInput

Represents fire input state.

**Purpose:**
- Decouples input system from combat logic

**Example Implementation:**
- `CombatInputReader` (Unity Input System)

---

### IDamageable

Represents an object that can receive damage.

**Purpose:**
- Allows any object to respond to hits without ShooterCore knowing implementation details

**Example Implementation:**
- `SimpleHealth`

---

### IWeapon

Represents a weapon that can be used by `ShooterCore`.

**Purpose:**
- Abstracts weapon-specific behavior (muzzle, direction, firing data)

**Example Implementation:**
- `HitscanWeapon`

---

### Design Notes

- Interfaces allow replacing systems without modifying core combat logic
- Camera, input, and weapon implementations are fully swappable
- This is the foundation of the system's modular design

---

## 8. CombatAimToLocomotionBridge

Bridges the combat aiming system with an external locomotion system.

### Purpose

This component allows the combat system to control how the character behaves when aiming,
without directly depending on any specific locomotion implementation.

---

### What It Does

When aiming:

- Disables locomotion-driven rotation
- Allows combat system to control facing direction
- Switches movement reference to aim camera direction

When not aiming:

- Restores locomotion rotation control
- Switches movement reference back to free-look camera

---

### Why It Exists

The combat system is designed to be independent from locomotion.

This bridge:
- Connects combat state → locomotion behavior
- Keeps both systems decoupled
- Allows integration with any third-person controller

---

### Requirements

The target locomotion system must implement:

- `IRotationMoveBridgeTarget`

This interface is used to:
- Enable / disable rotation control
- Set movement reference transform

---

### Typical Setup

1. Add `CombatAimToLocomotionBridge` to your player
2. Assign:
   - CombatInputReader
   - Locomotion target (implements `IRotationMoveBridgeTarget`)
   - Aim camera yaw reference
   - Free-look yaw reference

---

### Notes

- This component is optional
- Use only if your locomotion system requires rotation switching
- Integration results may vary depending on locomotion implementation
- Final movement feel may require project-specific tuning
- Additional tuning of movement, rotation, and input handling may be required

---

## 9. Optional Dependencies

### TextMesh Pro (TMP)

TextMesh Pro is used only for demo UI elements such as ammo display.

- The core combat system does NOT depend on TMP
- If TMP is not installed, the system will still function normally
- UI elements will simply not be displayed

---

End of file.
