# Third-Person Locomotion & Camera System

A lightweight and modular third-person locomotion and lock-on camera framework for Unity.

The core system is render pipeline independent.
The demo scene was created using the Built-in Render Pipeline.

Designed for:
- Soulslike games
- Action games
- TPS projects
- Rapid prototyping

---

## Features

- CharacterController-based locomotion
- Free-look third-person camera
- Lock-on targeting system
- Target switching
- Jump system
- Camera collision handling
- Clean modular architecture

---

## Folder Structure

LocomotionSystem/
│
├── Core/
│ └── Scripts/
│ ├── Animator/
│ ├── Camera/
│ ├── Input/
│ ├── Interfaces/
│ └── Locomotion/
│
├── Demo/
│ ├── Animator/
│ ├── Model/
│ ├── Prefabs/
│ ├── Scene/
│ ├── Textures/
│ └── UI/
│
├── Documentation/
│ ├── DEMO_GUIDE.md
│ ├── Manual.md
│ └── README.md
│
├── Settings/
│ └── InputActions/
│
├── CHANGELOG.md
└── Third-Party Notices.txt

---

## Core vs Demo

### Core (Runtime)
Location:
LocomotionSystem/Core/

Contains:
- All runtime scripts
- No third-party assets
- No external dependencies

This folder alone is sufficient to use the locomotion system.

---

### Demo (Optional)

Location:
LocomotionSystem/Demo/

Contains:
- Example character
- Animations
- Example scene
- Sample prefabs
- UI and textures

The Demo folder is **optional** and can be safely deleted without affecting the core system.

---

## Third-Party Content

The Demo folder includes third-party assets provided by:

**Kenney**  
License: CC0 (Public Domain)

These assets are used for demonstration purposes only.

Kenney is not affiliated with or endorsing this product.

See `Third-Party Notices.txt` for details.

---

## Documentation

- Documentation/Manual.md
- Documentation/DEMO_GUIDE.md
- CHANGELOG.md
- Third-Party Notices.txt