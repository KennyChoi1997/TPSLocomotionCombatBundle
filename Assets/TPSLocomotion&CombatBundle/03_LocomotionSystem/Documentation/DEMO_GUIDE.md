# Demo Guide

The demo scene is designed to help users quickly understand and test the locomotion,
camera, and lock-on systems in an isolated environment.

### Render Pipeline Note

The demo scene was created using the Built-in Render Pipeline.

If materials appear incorrect when opened in a different render pipeline,
you may need to adjust the demo materials to match your project settings.

This affects demo visuals only and does not impact the core system.

---

## Demo Zones Overview

### 1. Spawn Area
- Starting location
- Displays keyboard and controller input hints
- Leads directly into the locomotion test area

---

### 2. Basic Locomotion & Camera Test
Purpose:
- Demonstrate walking, running, sprinting, jumping, and free-look camera control

Includes:
- Flat and elevated terrain
- Simple obstacles
- Open space for movement testing

---

### 3. Lock-On Test
Purpose:
- Showcase the lock-on system and target switching

Includes:
- Multiple dummy targets
- Left and right target switching paths

Test:
- Lock-on toggle
- Target switching (stick X-axis or mouse wheel)
- Auto-break when obstructed or outside angle

---

### 4. Playground
Purpose:
- A sandbox area to experiment with all systems together

Includes:
- Open arena layout
- Mixed obstacles
- Multiple targets for free testing

---

## Notes
- All demo assets exist solely for demonstration purposes and can be safely removed in production projects.
- The demo scene is optional and not required for using the core system.
- The core locomotion and camera systems do NOT depend on any demo assets or demo packages.