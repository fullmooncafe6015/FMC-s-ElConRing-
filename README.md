## Project Overview

2D Metroivania platformer inspired by with a mix of Umamusume, Sonic 2D Action Platformer, and Elden Ring/Soulslike genres in general. The player explores procedurally generated rooms, moves between platforms, fights enemies, breaks objects, eats flowers, and activates hidden switches to unlock the portal, which is the ultimate goal to reach the finish line.

The game uses Unity's Input System for player controls, the Audio Mixer, Rigidbody2D for movement, Animator Controllers for character animations, StateMachineBehaviour scripts, ScriptableObjects for enemy properties, particle effects for collecting and destroying enemies, and procedural generation to create different room layouts.

The player can use melee attack and range projectile to defeat enemies and break pots. Some pots contain hidden switches that help complete the room puzzle. Once all required switches are activated, the portal becomes available, allowing the player to reach the destination.

## Script Summary

The project contains scripts for player control, procedural level generation, puzzle tracking, combat, enemies, collectibles, audio, UI/Menu Controller, and gameplay effects.

| Script | Purpose |
|---|---|
| `PlayerController.cs` | Controls player movement, jumping, double jumping, attacking, shooting, health, and player interactions using the Unity Input System. |
| `PlayerControls.cs` | Generated C# wrapper used to access the configured player input actions. |
| `ProceduralLevelGenerator.cs` | Generates rooms, platforms, tilemaps, hazards, decorations, enemies, flowers, pots, switches, and portal placement. |
| `PuzzleController.cs` | Tracks activated switches and determines when the puzzle is completed. |
| `Switch.cs` | Detects player interaction and reports switch activation to the PuzzleController. |
| `SwitchDoor.cs` | Similar to ' Switch.cs but specifically used for the Room Scene only. |
| `Door.cs` | Controls the door-opening behavior for the separate ordered-switch puzzle implementation in the Room scene. |
| `Pot.cs` | Handles breakable pots and reveals a hidden switch when one is assigned. |
| `Projectile.cs` | Controls projectile movement and damage to enemies or other valid targets. |
| `Enemy.cs` | Controls on-ground movement left and right over a short distance; can damage the player via a collider trigger. |
| `EnemyStats.cs` | ScriptableObject containing configurable properties for ground enemies. |
| `FlyEnemyStats.cs` | ScriptableObject containing configurable properties for flying enemies. |
| `FlyEnemy.cs` | Controls flying enemy movement, including circular movement behavior, using its assigned stats. |
| `Flower.cs` | Handles collectible objects, such as Flowers, and updates the player's collected amount. |
| `SoundManager.cs` | Manages game audio and provides persistent sound management across scenes. |
| `GameSession.cs` | Stores gameplay state such as the player's collected Flower. |
| `CameraFolllow.cs` | Controls camera following behavior during gameplay. |
| `AttackHitBox.cs` | Detects enemies or breakable objects inside the player's attack area and applies damage when the player attacks. |
| `DropFlower.cs` | Handles the flower drop created when an enemy is defeated. |
| `ExplosionEffect.cs` | Plays an explosion particle effect when an object, such as a pot or enemy, is destroyed. |
| `FinishTrigger.cs` | Detects when the player reaches the finish area and triggers the game completion sequence. |
| `Hazard.cs` | Detects when the player touches a hazard and applies damage. |
| `HudController.cs` | Updates the in-game HUD to display gameplay information, such as the player's health or collected items. |
| `InstructionBoard.cs` | Allows the player to interact with an instruction board to view the game's controls and instructions. |
| `LevelPortal.cs` | Handles player interaction with the level portal to move to another level or scene. |
| `PlayerHealthBar.cs` | Updates the health bar to visually represent the player's current health. |
| `PressAnyButton.cs` | Waits for player input before continuing from the title or introductory screen. |
| `SceneLoader.cs` | Handles loading and transitioning between game scenes. |

## How to Play

- A/D - Left/Right
- Space - Jump/Double Jump
- Left Mouse - Melee Attack
- E - Range Attack

## Third-Party Assets, Packages, and Tutorials

The project uses Unity's built-in systems and packages, including:

- Unity Input System for keyboard and mouse input.
- Unity 2D Tilemap system for procedural level construction.
- Unity Physics 2D for Rigidbody2D, Collider2D, and trigger interactions.
- Unity Animator for player and enemy animations.
- Unity Particle System for gameplay effects.
- Unity Audio System for music and sound effects.
- ScriptableObjects for configurable enemy data.

### Third-Party Assets

The project uses external visual, audio, and other resources where applicable.

The following information should be completed with the actual assets used in the Project 2 build:

- Player character sprites and animations.
- Enemy sprites and animations.
- Environment and platform tilesets.
- Collectible and puzzle sprites.
- Sound effects and background music.
- Particle effect textures or prefabs.

The following third-party assets are:

- (Characters)

  - Grass Wonder: 
    - She is from the popular Japanese gacha game franchise called Umamusume: Pretty Derby. 
      - https://umamusume.com/characters/grasswonder/
    - This specific design for this game is a fan-designed character called Tanuki, a deformed chibi in the Japanese 2chan thread "深夜のションボリたぬきスレ". 
    - The Tanuki artwork is mostly made by an anonymous user, but her Tanuki design is partially made by a user called Zzhao/Hakuhan (Their social media is impossible to find).

  - Black Knight (Enemy): 
    - Part of the Tanuki thread "深夜のションボリたぬきスレ".

  - Mambo (Enemy): 
    - El Condor Pasa's pet bird, an official character from Umamusume, and the artwork design is part of "深夜のションボリたぬきスレ".

  - El Condor Pasa (NPC): 
    - Official character from Umamusume.
      - https://umamusume.com/characters/elcondorpasa/
    - Her Tanuki design is from "深夜のションボリたぬきスレ".

  - Yamanin Zephyr (NPC): 
    - Official character from Umamusume.
      - https://umamusume.com/characters/yamaninzephyr/
    - Her Tanuki design is from "深夜のションボリたぬきスレ".

  - Maurice (Decoration Painting):
    - Part of the Tanuki thread "深夜のションボリたぬきスレ".

- (Platformer)

  - https://assetstore.unity.com/packages/2d/environments/pixel-art-platformer-village-props-166114
  - https://assetstore.unity.com/packages/2d/environments/asset-fttgr-free-pixel-art-platformer-222174
  - https://assetstore.unity.com/packages/p/pixel-2d-castle-tileset-135397
  - https://assetstore.unity.com/packages/2d/environments/platformer-set-150023

- (Object)

  - Justaway (Pot):
    - Based on Gintama's Justaway in-universe merchandise.
      - https://gintama.fandom.com/wiki/Justaway
    - Artwork design comes from "深夜のションボリたぬきスレ"

  - ChickenLeg:
    - Artwork design comes from "深夜のションボリたぬきスレ"

  - Flower:
    - https://btl-games.itch.io/springtime-dend

  - Portal:
    - https://benmusick.artstation.com/projects/xGr14

  - Switch:
    - https://assetstore.unity.com/packages/p/pixel-2d-castle-tileset-135397

- (UI)

  - Player's HP Handle Slider is custom-made by Duong Banh.

  - TitleScreen Background is hand-drawn and was used previously from Project 1.

- (Music)

  - (Elden Ring 8-bit) https://www.youtube.com/watch?v=Kl4-HAepMtM&list=RDKl4-HAepMtM&start_radio=1
  - (RERoom) https://youtu.be/8Hq0l4kd8tQ?si=wLZWULTJVwA3xopL
  - (CityCrisis8bit) https://youtu.be/KwuKYBMrlVg?si=AwLsuiXmqzIMHLF7
  - (DeathSound) https://youtu.be/VUfvRciny_Y?si=igGfiGjy3zyyyzTA
  - (Breaking Glass Sound) https://www.myinstants.com/en/search/?name=break%20glass
  - (Sekiro Parry Sound) https://www.youtube.com/watch?v=qtNDctR2iMM
  - (Fortnite Gun Sound) https://www.myinstants.com/en/search/?name=fortnite+gun
  - (GemChime) From Midterm Zippack
  - (Portal) Elden Ring Confirmation Sound https://www.zedge.net/notification-sounds/c1592250-a288-4475-9fb4-040d72881616
  - (Portal Appears) Elden Ring Traverse Mist Sound https://www.zedge.net/notification-sounds/3e0b9d30-aaf2-47a8-a51e-c0d29b3f90bf
  - (Result) https://www.youtube.com/watch?v=X4iYFVZwZoI&list=RDX4iYFVZwZoI&start_radio=1

Some scripts are from the Midterm Review/Exam with custom modifications, and some were previously used in Project 1.

### Tutorials and Learning References

Procedural Generations are varied sources, such as:
  - https://youtube.com/playlist?list=PLcRSafycjWFenI87z7uZHFv6cUG2Tzu9v&si=ZU5vuYeIKEVmnIwb
  - https://youtu.be/-5OS1s-NWRw?si=BrLE0oANMEuKeObY
  - https://youtu.be/NtY_R0g8L8E?si=8U2Od8okTQ29HJRA

Any tutorial code or examples used are from Project 1, and this project is a continuation that builds on newer concepts and lecture materials. 

The following tutorial references for the project are:
  - https://youtu.be/FXXc0hTWIMs?si=8yds-00jGFSc2YD7 
  - https://ezgif.com/gif-to-sprite (EzGif Sprite Sheet Converter)
  - https://www.youtube.com/shorts/bCzxNCvEtc0

