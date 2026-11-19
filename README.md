# COMP 590 Escape Room

A VR escape room game built in Unity for COMP 590 (Assignment 6).

**Team:** Grace, Lucy, Lauren, Julianne

**Demo video:** _add link here_

## Requirements

- **Unity 6000.3.26f1** (exact version, install via Unity Hub). Do not click "Upgrade" if Unity prompts you.
- Unity modules: **Android Build Support**, including **OpenJDK** and **Android SDK & NDK Tools**
- **Git** and **Git LFS**
  ```bash
  brew install git-lfs
  git lfs install
  ```
  Install Git LFS *before* cloning, or binary assets (audio, models, textures) will be broken pointer files.
- Render pipeline: Universal Render Pipeline (URP)
- Target: Meta Quest (Android), tested in the editor with the Meta XR Simulator

## Setup

1. Clone the repo (after installing Git LFS).
2. Open the project folder in Unity Hub with Unity 6000.3.26f1.
3. Wait for the first import. It can take several minutes.
4. Open **File > Build Profiles**, select the **Meta Quest** profile, and click **Switch Platform** if it is not already active.
5. Open the main scene in `Assets/Scenes/` and press Play with the Meta XR Simulator.
6. Confirm there are no pink materials or console errors before making changes.

If the simulator does not launch: **Edit > Project Settings > XR Plug-in Management**, open the Windows/Mac/Linux Standalone tab, and make sure **OpenXR** is checked.

## Building

- Use **File > Build Profiles > Build** (not Build and Run) to produce an `.apk` without a headset.
- Save builds **outside the project folder**. Build output is git-ignored and should never be committed.
- Share APKs through a GitHub Release or a drive link.

## Team Workflow

- Pull before starting work, push when you finish a task.
- Use a branch per feature or puzzle (e.g. `feature/puzzle-1`) and merge via pull request.
- Prefer one prefab or scene per puzzle to avoid scene merge conflicts. Tell the group before editing the main scene.
- Always commit `.meta` files with their assets. Move or delete assets inside the Unity Editor only.
- Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, build folders, or `.DS_Store`.

## Game Overview

_Theme, puzzles, win/lose conditions, movement, particle system, custom shader, and the non-visual sense (e.g. audio) go here._

## Outside Resources

_List every asset, tutorial, or package used, with attribution._

- Meta XR All-in-One SDK
- Unity XR packages (Input System, URP)
- Adventure demo kit by Hunter Arc Studio from the Unity Asset Store
- Simple Low Poly Keys by 3Digitalis from the Unity Asset Store
- Yughues Free Sand Materials by Nobiax/Yughus from the Unity Asset Store
- https://www.youtube.com/watch?v=naleCT1D684 - How To Create Night Lighting In Unity URP | Step by Step Tutorial | Unity 6 | Realistic Lighting |
- https://www.youtube.com/watch?v=VCi_ts5Ubwk - 🗺 Fast HDRI Sky Unity URP - How To Make - Quick Tip
- Game end screen logic generated and guided by Claude
