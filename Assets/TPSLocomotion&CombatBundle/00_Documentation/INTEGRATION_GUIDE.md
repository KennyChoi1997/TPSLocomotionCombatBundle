# INTEGRATION GUIDE

## Overview

TPS Locomotion & Combat Bundle combines the standalone Locomotion and Combat packages into a single gameplay workflow.

The integration layer is located inside:

05_Integration

This layer connects movement, aiming, weapon handling, and animation systems without modifying the original locomotion or combat architecture.

---

## Architecture

Locomotion System
↓
Integration Layer
↓
Combat System

The integration layer acts as a bridge between the two systems.

---

## Core Components

### CameraModeController

Responsible for switching camera behaviour between:

* Free Look
* Aim Mode

The component adjusts camera distance and notifies other systems when the player enters or exits aim mode.

---

### BundleAimProvider

Provides the combat system with a camera-based aim ray.

Responsibilities:

* Generate aim rays from the active gameplay camera
* Support viewport-based aiming
* Provide a consistent aiming solution for the integrated setup

Used by:

* ShooterCore

---

### PlayerRotationCoordinator

Synchronizes player rotation with combat aiming.

Responsibilities:

* Rotate the player toward the camera while aiming
* Prevent unwanted rotation behaviour during locomotion
* Maintain TPS-style character control

---

### IntegratedAnimatorBridge

Connects locomotion and combat animation states.

Responsibilities:

* Update aiming state
* Forward fire events
* Forward reload events
* Synchronize locomotion and combat parameters

---

### WeaponManager

Handles weapon ownership and switching.

Responsibilities:

* Equip weapons
* Switch active weapons
* Notify dependent systems of weapon changes

---

## Camera Setup

The bundle uses a single gameplay camera.

Unlike the standalone Combat demo, which uses separate camera behaviour, the bundle relies on:

* CameraModeController
* BundleAimProvider

to provide a unified TPS workflow.

---

## Combat Setup

ShooterCore receives aiming information through:

BundleAimProvider

and weapon information through:

WeaponManager

The combat system remains modular and independent from locomotion logic.

---

## Demo Scene

### Demo_CQB

The CQB demo scene demonstrates:

* Movement
* Camera control
* Aiming
* Shooting
* Reloading
* Weapon swapping
* HitBox damage

This scene is intended to showcase the complete integrated workflow.

---

## Layer Configuration

Recommended layers:

Player
Obstacle
Target

### Obstacle

Used for:

* Walls
* Floors
* Buildings
* Environmental geometry

### Target

Used for:

* Practice targets
* Damageable enemies
* HitBox objects

---

## Notes

The integration layer is designed specifically for the systems included in this bundle.

Developers integrating other locomotion or combat solutions may need additional customization.

The standalone Locomotion and Combat packages remain modular and can be used independently if preferred.
