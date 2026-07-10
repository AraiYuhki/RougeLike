# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Unity roguelike dungeon crawler (Mystery Dungeon style) with a card-based action system. Comments, commit messages, UI text, and editor tool names are in Japanese.

- Unity Editor version: **2023.2.4f1** (see `ProjectSettings/ProjectVersion.txt`; the README's 2021.3.22f1 is outdated)
- There are no CLI build/test/lint commands, no CI workflows, and no test assemblies — everything is built and run through the Unity Editor (scenes: `Assets/Scenes/Title.unity`, `Assets/Scenes/Game.unity`)
- Key plugins: UniTask, DOTween (Demigiant), Addressables, Input System, IngameDebugConsole, UnityDebugSheet, Graphy

## Architecture

### Game loop / state machines

`GameController` (`Assets/Scripts/Game/Controller/GameController.cs`) is the entry point of the Game scene. It drives a `DungeonStateMachine` built on the generic `StateMachine<TEnum, TState>` + `IState` pattern (`Assets/Scripts/Game/StateMachine/StateMachine.cs`). Turn flow cycles through `GameState` states: `PlayerTurn` → `EnemyTurn`, plus `MainMenu`, `NextFloorLoad`, `Dialog`, `Shop` (states in `Assets/Scripts/Game/StateMachine/Dungeon/`). The Title scene has its own `TitleStateMachine` using the same pattern.

### Master data (game balance data)

Master data flows: **TSV files → ScriptableObject tables → `DB` singleton**.

- Row classes: `Assets/Scripts/Master/Info/*Info.cs` (e.g. `CardInfo`, `EnemyInfo`)
- Table classes: `Assets/Scripts/Master/Table/M*.cs`, extending `TableBase<T>` (ScriptableObjects)
- Table assets live in `Assets/ScriptableObjects/M*.asset` and are loaded synchronously via Addressables by the `DB` singleton (`DB.Instance.MCard` etc., `Assets/Scripts/Master/DB.cs`)
- TSV exports live in `Assets/StreamingAssets/*.tsv`; conversion uses `CsvParser`/`CsvColumn` (`Assets/Scripts/Utility/Csv/`) via the editor menu **Tools → Master → ExportToCsv**; reload with **Tools → Master → DB再読み込み**

When adding a new master table, all four pieces are needed: Info class, Table class, ScriptableObject asset registered with Addressables, and a `DB` property.

### Dungeon / floor

`DungeonGenerator` and `FloorManager` (`Assets/Scripts/Game/Dungeon/`) generate floors as a grid of `TileData` (tile types: Room/Path/Wall/Hole) partitioned into `Area`/`Room`/`Path`. Pathfinding utilities (`AStar`, `Dijkstra`, `BackTracking`) are in `Assets/Scripts/Game/Utility/`. Grid positions use the `Point` struct.

### Units and managers

`Unit` (`Assets/Scripts/Game/Unit/Unit.cs`) is the base for `Player` and `Enemy` (AI in `EnemyAI.cs`). Per-floor object lifecycles are owned by manager MonoBehaviours (`EnemyManager`, `ItemManager`, `TrapManager`, `UIManager`, `DamagePopupManager` in `Assets/Scripts/Game/Manager/`), all wired via `[SerializeField]` references on scene objects. Runtime state lives in plain data classes under `Assets/Scripts/Game/Data/` (`PlayerData`, `EnemyData`, `SaveData`, ...), separate from the MonoBehaviours.

### Card system

Cards are the player's action/item system (attack cards, consumables, and passive cards that work while in hand). `CardController` (`Assets/Scripts/Game/Controller/CardController.cs`) manages the deck/hand; card behavior is data-driven from `MCard`/`MAttack`/`MAttackArea`/`MPassiveEffect` master tables. Card UI is in `Assets/Scripts/Game/UI/Card/`. See README.md for the gameplay spec of each implemented card.

### UI framework

Custom menu/navigation framework under `Assets/Scripts/Game/UI/`: `SelectableItem`/`InteractiveItem` elements inside `MenuBase` containers (`VerticalMenu`, `HorizontalMenu`, `GridScrollMenu`, ...), dialogs via `DialogManager` (`UI/Dialogs/`). Input goes through the Input System (`Assets/Resources/InGame.inputactions`) with helpers in `InputUtility`.

### Assemblies

Almost all game code is in the default `Assembly-CSharp`; only `Assets/Scripts/Utility/SaveSystem` and `Assets/Scripts/Utility/Lottery` have their own asmdefs (referencing them from game code works because Assembly-CSharp implicitly references all asmdefs). `SaveSystem`'s `DataBank` handles save I/O with compression (`Compressor`) and encryption (`Cryptor`).

## Editor tools

Custom tooling lives under the **Tools** menu (sources in `Assets/Scripts/Editor/`): ダンジョン設定ツール (dungeon/floor master editing), 攻撃範囲エディタ (attack area editing), フロアビュアー, A*テスター, ダイクストラテスター, and the Master CSV export / DB reload items mentioned above.
