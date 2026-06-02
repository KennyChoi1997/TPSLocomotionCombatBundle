# TPS Locomotion & Combat Bundle

TPS Locomotion & Combat Bundle is a complete third-person gameplay framework for Unity 6.

The package combines a third-person locomotion system, combat system, and integration layer into a single workflow, 
allowing developers to prototype or build TPS-style projects more quickly.

---

## Features

### Locomotion

* CharacterController-based movement
* Walk / Run / Sprint
* Jump support
* Free-look camera
* Camera collision
* Lock-on targeting
* Target switching

### Combat

* Hitscan shooting
* Semi-auto fire
* Full-auto fire
* Shotgun support
* Reload system
* Weapon swapping
* Direct weapon slot selection
* HitBox-based damage

### Bundle Integration

* Unified locomotion and combat workflow
* Single-camera TPS setup
* Integrated aiming and player rotation
* Animator bridge for locomotion and combat
* CQB demo scene included

---

## Requirements

* Unity 6 (6000.x)
* Built-In Render Pipeline (URP)
* New Input System

---

## Package Structure

01_Locomotion

* Third-person locomotion system

02_Combat

* TPS combat framework

05_Integration

* Bundle-specific integration layer

Demo

* Example scenes and setup

---

## Demo Scenes

### Demo_Locomotion

Demonstrates:

* Movement
* Camera control
* Lock-on targeting

### Demo_Combat

Demonstrates:

* Shooting
* Reloading
* Weapon swapping
* HitBox damage

### Demo_CQB

Demonstrates:

* Integrated locomotion and combat workflow
* TPS aiming
* Weapon handling
* Target engagement inside a CQB environment

---

## Controls

### Keyboard & Mouse

* WASD – Move
* Mouse – Look
* Right Mouse Button – Aim
* Left Mouse Button – Fire
* Space – Jump
* R – Reload
* 1 / 2 / 3 – Weapon Slots
* Mouse Wheel Up – Next Weapon
* Mouse Wheel Down – Previous Weapon

### Gamepad

* Left Stick – Move
* Right Stick – Look
* Left Trigger – Aim
* Right Trigger – Fire
* South Button – Jump
* West Button – Reload
* D-Pad Up / Right / Down – Weapon Slots
* Right Shoulder – Next Weapon
* Left Shoulder – Previous Weapon

---

## Quick Start

1. Open Demo_CQB.
2. Press Play.
3. Move using WASD or Left Stick.
4. Aim using Right Mouse Button or Left Trigger.
5. Fire using Left Mouse Button or Right Trigger.

---

## Notes

This package focuses on gameplay systems and integration workflow.

Characters, animations, weapons, and environments included in the demo scenes are intended to demonstrate functionality and may be replaced with project-specific assets.

Refer to QUICK_START.md and INTEGRATION_GUIDE.md for setup details.
