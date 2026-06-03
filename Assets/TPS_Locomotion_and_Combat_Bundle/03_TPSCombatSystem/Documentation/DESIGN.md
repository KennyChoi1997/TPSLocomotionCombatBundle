# TPS Combat System – Design Document (Draft)

## 1. Purpose

This project aims to build a **modular, reusable TPS-style combat system**
that can be layered on top of an existing third-person locomotion system.

The primary goal is **clean separation of responsibilities**, allowing:
- Combat logic to be reused across different locomotion setups
- Input, camera, and aiming logic to be swapped without modifying combat core
- Future assetization as an independent combat module

This system is **not** intended to be a full game framework.
It focuses on core combat mechanics only.

---

## 2. Design Goals

- Decouple combat logic from:
  - Camera implementation
  - Input System details
  - Player locomotion implementation
- Favor **interface-based design** over concrete dependencies
- Support Unity **Input System (New)** only
- Prioritize clarity, maintainability, and extensibility over feature count
- Keep v1 scope minimal and predictable

---

## 3. High-Level Architecture

[ Input Layer ]
└─ IFireInput
└─ CombatInputReader (Input System)

[ Aim Layer ]
└─ IAimProvider
└─ CameraAimProvider (Screen-center ray)

[ Combat Core ]
└─ ShooterCore
- Requests aim ray
- Requests fire input
- Performs raycast & hit detection

[ Locomotion System ]
└─ External dependency
(Not modified by combat system)


The combat system does **not** directly reference:
- Player movement logic
- Animator state machines
- Camera hierarchy details

---

## 4. Core Principles

### 4.1 Interface-First Design

All external dependencies are accessed via interfaces:
- `IAimProvider`
- `IFireInput`

This allows:
- Easy replacement of camera systems
- Different control schemes (mouse, gamepad, lock-on, etc.)
- Clean testing and debugging

---

### 4.2 ShooterCore Responsibility

`ShooterCore` is responsible for:
- Requesting an aim ray
- Detecting fire input
- Performing hit detection (raycast)
- Forwarding hit results (future extension)

`ShooterCore` does **not**:
- Know how the camera works
- Know how input is implemented
- Handle damage logic (future module)

---

## 5. Input Handling Strategy

- Uses Unity **Input System (New)** exclusively
- No usage of legacy `UnityEngine.Input`
- Input logic is abstracted via `IFireInput`

This ensures:
- Compatibility with modern Unity versions
- Predictable behavior across platforms
- Clear separation between gameplay logic and control schemes

---

## 6. Aiming Strategy

### Current (v0.x)
- Screen-center ray based aiming
- Suitable for:
  - TPS
  - Lock-on assisted aiming
  - Controller-focused gameplay

### Future Extensions
- Lock-on based aim provider
- Weapon-specific aim providers
- ADS-style camera-aligned aiming

These will be implemented as **additional IAimProvider implementations**.

---

## 7. Planned Extensions (Not v1)

- Damage system (`IDamageable`)
- Weapon abstraction
- Fire rate / recoil / spread
- Hit feedback (FX, camera response)
- Lock-on assisted targeting

These are intentionally excluded from the initial scope.

---

## 8. Integration with Locomotion System

The combat system is designed to:
- Be attached to the same GameObject as locomotion
- Or exist as a sibling component

No direct dependency on:
- CharacterController
- Rigidbody
- Animator parameters

Locomotion is treated as an **external system**.

---

## 9. Non-Goals

- Full animation-driven combat
- Melee combat (initially)
- Networking support
- AI combat behaviors
- Complex weapon inventory systems

---

## 10. Current Status

- Core shooting pipeline functional
- Aim provider abstraction implemented
- Input abstraction implemented
- Editor test flow validated

Project is in **early foundation stage**.

---

## 11. Open Questions

- Best abstraction point for damage handling
- How tightly combat should synchronize with locomotion states
- Whether to expose combat events or keep pull-based design

These will be revisited once locomotion integration is tested.

---
