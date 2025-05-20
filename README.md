# Bruiser

A prototype project for a physics-based animal football game built in Unity. Each
player controls a different animal, and gameplay is networked using Unity
Netcode for GameObjects.

## Repository Layout

- `Project Bruiser/Assets/` – Unity assets and scripts
  - `Scenes/` – Unity scenes
  - `Scripts/` – C# game logic
    - `Animals/` – base classes for animal controllers
    - `Game/` – game management scripts
    - `Networking/` – network setup and utilities
- `Project Bruiser/Packages/` – Unity package manifests
- `Project Bruiser/ProjectSettings/` – Project configuration

## Getting Started

1. Install the Unity version referenced in `ProjectSettings/ProjectVersion.txt`.
2. Open the `Project Bruiser` folder with Unity Hub.
3. Press play in the `SampleScene` to start a local host session via
   `NetworkBootstrap`.

This repository is at an early stage and serves as a starting point for future
development.
