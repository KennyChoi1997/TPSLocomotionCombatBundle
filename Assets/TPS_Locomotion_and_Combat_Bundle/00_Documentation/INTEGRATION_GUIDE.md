# INTEGRATION GUIDE

## Overview

TPS Locomotion & Combat Bundle combines the Locomotion System and TPS Combat System into a unified third-person gameplay workflow.

The integration layer is located inside:

04_Integration

This layer connects movement, aiming, weapon handling, camera behaviour, and animation systems while keeping locomotion and combat responsibilities separated.

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

Responsibilities:

* Adjust camera distance
* Manage aim state transitions
* Notify dependent systems when aim mode changes

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

### BundleAimViewportController

Synchronizes viewport-based aiming and crosshair positioning.

Responsibilities:

* Update viewport aim position
* Support shoulder aiming offsets
* Maintain alignment between aiming and UI

---

### CameraShoulderOffsetController

Applies shoulder-camera positioning during aim mode.

Responsibilities:

* Maintain centered free-look camera behaviour
* Shift camera position during aiming
* Support TPS-style aiming presentation

---

### PlayerRotationCoordinator

Synchronizes player rotation with combat aiming.

Responsibilities:

* Rotate the player toward the camera while aiming
* Prevent unwanted rotation behaviour during locomotion
* Support TPS-style aiming movement
* Support backpedaling behaviour while aiming

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
* CameraShoulderOffsetController

to provide a unified TPS workflow.

---

## Combat Setup

ShooterCore receives aiming information through:

* BundleAimProvider

and weapon information through:

* WeaponManager

The combat system remains modular and independent from locomotion logic.

---

## Demo Scene

### IntegratedDemo (Demo_CQB)

The CQB demo scene demonstrates:

* Movement
* Camera control
* Aiming
* Shooting
* Reloading
* Weapon swapping
* HitBox damage
* Player rotation integration
* Locomotion and combat animation integration

This scene is intended to showcase the complete integrated workflow.

---

## Layer Configuration

Recommended layers:

* Player
* Obstacle
* Target

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

Developers integrating other locomotion or combat solutions may require additional customization.

The included locomotion and combat systems remain modular and may also be used independently if desired.


### Bundle Version Notes

To support the integrated workflow, some components included in this bundle may contain minor adjustments or integration-specific tuning compared to the standalone Locomotion System and TPS Combat System packages.

These changes are intended to improve compatibility, usability, and overall integration within the bundle environment.

Standalone packages continue to be maintained independently and may receive updates separately from the bundle version.

