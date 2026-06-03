# TPS Combat System – Integration Guide

This guide explains how to integrate the TPS Combat System with your own third-person character or locomotion system.

---

## 1. Overview

The combat system is designed to be independent from locomotion.

To integrate it, you need to connect:

* Input → Combat
* Camera → Aim
* Weapon → ShooterCore
* (Optional) Locomotion → Combat via bridge

---

## 2. Minimal Setup (No Locomotion Integration)

This is the fastest way to get shooting working.

### Step 1 – Add Components

Add the following to your player:

* `ShooterCore`
* `CombatInputReader`
* `CameraAimProvider`
* `WeaponController`
* `HitscanWeapon`

---

### Step 2 – Assign References

#### ShooterCore

* Aim Provider → `CameraAimProvider`
* Weapon → `HitscanWeapon`
* Input → `CombatInputReader`

#### WeaponController

* Shooter → `ShooterCore`
* Weapon → `HitscanWeapon`
* Config → `WeaponConfig`

#### HitscanWeapon

* Muzzle → weapon barrel transform
* Config → same `WeaponConfig`

---

### Step 3 – Press Play

You should now be able to:

* Aim using camera center
* Fire using input
* Hit targets

---

## 3. Locomotion Integration (Recommended)

To properly integrate with a third-person controller, use the bridge system.

---

## 4. CombatAimToLocomotionBridge

This component connects combat aiming with your locomotion system.

---

### Step 1 – Add the Bridge

Add:

* `CombatAimToLocomotionBridge`

to your player object.

---

### Step 2 – Implement Interface

Your locomotion system must implement:

```csharp
public interface IRotationMoveBridgeTarget
{
    void SetRotationAllowed(bool allowed);
    void SetMoveReference(Transform reference);
}
```

---

### Step 3 – Assign Bridge References

In `CombatAimToLocomotionBridge`:

| Field                   | What to assign              |
| ----------------------- | --------------------------- |
| Combat Input Reader     | `CombatInputReader`         |
| Target Behaviour        | Your locomotion component   |
| Aim Yaw Reference       | Aim camera yaw transform    |
| Free Look Yaw Reference | Normal camera yaw transform |

---

### Step 4 – Behaviour

When aiming:

* Character rotates toward aim direction
* Movement aligns with camera aim

When not aiming:

* Locomotion system regains control

---

## 5. Example Integration

Typical setup:

* CharacterController handles movement
* Camera controls view
* Combat system handles shooting
* Bridge synchronizes rotation

---

## 6. Common Issues

### Weapon not aligned with crosshair

* Check `Forward Axis` in `HitscanWeapon`

---

### Shots not hitting targets

* Check `Hit Mask` in `WeaponConfig`

---

### Character rotates incorrectly

* Check `IRotationMoveBridgeTarget` implementation

---

### Nothing happens when firing

* Check `CombatInputReader`
* Ensure Input System is enabled

---

## 7. Notes

- The bridge is optional but recommended
- The system is fully modular and replaceable
- You can implement custom aim providers or input systems

### Integration Notes

- Integration quality depends on the locomotion system used
- Additional tuning may be required for movement and rotation behavior
- Camera-relative movement and rotation handling must be aligned with the combat system

---

End of file.
