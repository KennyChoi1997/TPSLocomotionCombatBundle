# TPS Combat System

## Overview
Modular TPS shooting system designed to work independently from any locomotion system.

## Features
- Camera-based aiming (screen centre)
- Hitscan shooting system
- Interface-driven modular architecture
- Decoupled from camera and locomotion systems
- Demo scene included

## Demo
The demo scene features a Y-shaped shooting range with three independent branches:

- Shotgun branch (close-range focused)
- Pistol branch (mid-range focused)
- Rifle branch (long-range focused)

Each branch maintains one active target and respawns independently.

## Setup
1. Import the package
2. Add `ShooterCore` to your player
3. Assign an `IAimProvider` (e.g., CameraAimProvider)
4. Assign a weapon configuration
5. Set up input via `CombatInputReader`

## Controls
- Fire: Left Mouse / Trigger
- Aim: Right Mouse

## Notes
- Locomotion system is not included
- Demo uses a simplified setup
- Designed for easy integration into existing projects
- TextMesh Pro is optional (used only for demo UI such as ammo display)

## Dependencies
- Unity Input System (New)

## Documentation
- [DESIGN.md](./DESIGN.md) – system architecture and design decisions
- [CONFIGURATION.md](./CONFIGURATION.md) – inspector reference and setup details

## Third-Party Assets

Some demo assets are sourced from third-party creators.
See [Third-Party Notices.txt](./Third-Party Notices.txt) for details.