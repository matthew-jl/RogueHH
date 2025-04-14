# RogueHH ⚔️

**A 3D Turn-Based Roguelike Dungeon Crawler made in Unity**

[![Gameplay Showcase GIF Placeholder](./gifs/showcase.gif)](./gifs/showcase.gif)

## About The Game

Dive into **RogueHH**, a challenging turn-based roguelike adventure! Explore procedurally generated dungeon floors, battle increasingly difficult enemies, upgrade your character, and conquer the final boss. Each run offers a fresh experience with randomized layouts and strategic combat encounters.

## Key Features

- **Procedurally Generated Dungeons:** Never explore the same floor twice! Dungeons are randomly generated with unique room layouts, corridors, and decoration placements.

  [![Dungeon Generation GIF Placeholder](./gifs/dungeon-generation.gif)](./gifs/dungeon-generation.gif)

- **Tactical Turn-Based Combat:** Engage enemies in strategic, turn-based battles. Every move, attack, and skill activation counts! Features include:

  - Attack variations with sword trails.
  - Critical hits with visual feedback (screen shake, distinct damage popup).
  - Damage calculation considering enemy defense.
    [![Critical Hit GIF Placeholder](./gifs/critical-hit.gif)](./gifs/critical-hit.gif)

- **Grid-Based Movement with A\* Pathfinding:** Navigate the dungeons tile by tile. Hover over a valid tile to see the shortest path highlighted using the A\* algorithm before committing to your move.

  [![Movement Pathfinding GIF Placeholder](./gifs/movement.gif)](./gifs/movement.gif)

- **Character Progression & Upgrade System:** Enhance your chances of survival!

  - Gain Experience (EXP) and Zhen (currency) by defeating enemies.
  - Level up to automatically boost stats and unlock skills.
  - Spend Zhen in the Upgrade Menu to further customize Health, Attack, Defense, Critical Rate, and Critical Damage.  
    [![Upgrade Menu GIF Placeholder](./gifs/upgrade-menu.gif)](./gifs/upgrade-menu.gif)

- **Skill System:** Unlock and utilize powerful active and passive (buff) skills as you level up. Manage cooldowns and buff durations to gain the upper hand.

  [![Skill Usage GIF Placeholder](./gifs/skill-lifesteal.gif)](./gifs/skill-lifesteal.gif)

- **Intelligent Enemies:** Enemies exhibit different states (Idle, Alert, Aggro) based on player proximity and line-of-sight. They will pathfind towards the player once alerted.

  [![Enemy AI State Change GIF Placeholder](./gifs/enemy-state.gif)](./gifs/enemy-state.gif)

- **Enemy Variety & Scaling:** Encounter Common, Medium, and Elite enemies with distinct appearances and increasing stats. Difficulty ramps up as you descend deeper into the dungeon.

- **Atmospheric Presentation:** Immerse yourself in the dungeon with:

  - Aesthetic low-poly 3D graphics.
  - Post-processing effects enhancing visual quality.
  - Dynamic point lighting centered on the player for a dark, moody feel.
  - Atmospheric background music and sound effects.

- **Challenging Boss Floor:** Test your skills and build against the formidable final boss in a dedicated arena.

- **Save System:** Your progress (stats, level, max floor reached, Zhen) is automatically saved, allowing you to continue your run later.

## Technical Highlights

- Built with **Unity Engine**.
- Leverages **Scriptable Objects** for modular data management (stats, items) and event handling (Event Bus pattern).
- Procedural generation algorithms for map creation.
- A\* Algorithm implemented for pathfinding.
- Game state saved using **BinaryFormatter** to `Application.persistentDataPath`.
- Implementation of multiple **Design Patterns**, such as Singleton, State, and Observer.

## Basic Controls

- **Move:** Click on a highlighted valid tile.
- **Attack:** Click on an adjacent enemy (1 tile away horizontally or vertically).
- **Skills:** Press number keys (1-9) to select/activate unlocked skills.
- **Skip Turn:** Press `Spacebar`.
- **Pause:** Press `Escape`.
