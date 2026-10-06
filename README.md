# Unity Mining Game

A side-view mining action game built solo in Unity 6.
Dig through the underground, collect resources, upgrade your tools, and fight off enemies.



> 🇯🇵 日本語の説明は[下記](#日本語)をご覧ください。

## Features

- **Stage generation from image files** – levels are created automatically from PNG maps
- **Mining system** – breakable blocks with hit and destruction effects
- **Drop items** – large numbers of items rendered and collected efficiently
- **Fog of war** – unexplored areas stay hidden in darkness and are gradually revealed as you dig
- **Player controller** – custom movement and jumping
- **Enemy AI** – enemies detect, chase and attack the player, then return to their nest
- **Procedural walking animation** for the enemy's legs
- **Tools & skill tree** – mining and placement tools, upgraded with collected materials
- **Inventory and a rideable robot vehicle**

## Tech Stack

- Unity 6 (6000.3.11f1) / URP
- C#
- Shader Graph
- Cinemachine
- Input System
- Blender (3D models)

## Getting Started

1. Clone the repository
   ```
   git clone https://github.com/MertOzzencir/Unity-Mining-Game.git
   ```
2. Open Unity Hub → **Add** → select the `MiningGame` folder
3. Open the project with Unity **6000.3.11f1**
4. Open `Assets/Scenes and Settings/Scenes/SampleScene` and press Play

## Project Structure

```
MiningGame/Assets/
├── _Scripts/
│   ├── Map Generation/   # Stage generation, drop rendering, fog of war
│   ├── Player/           # Player movement and controls
│   ├── State/            # Enemy AI
│   ├── Tools/            # Mining and placement tools
│   ├── Upgrades/         # Skill tree
│   └── Inventory/
├── Shaders/
├── Models/
└── Prefab/
```

## Author

**Mert Özzencir** – Solo developer (programming, design, 3D assets)
GitHub: [@MertOzzencir](https://github.com/MertOzzencir)

---

## 日本語

Unity 6で個人開発している、横視点の採掘アクションゲームです。
地下を掘り進めて資源を集め、ツールを強化しながら敵と戦います。

### 主な機能

- 画像ファイルからステージを自動生成
- ブロックの採掘・破壊システムと破壊演出
- 大量のドロップアイテムを軽量に描画・回収するシステム
- 未探索エリアを闇で隠し、掘り進めると見えるようになる視界システム
- プレイヤーの移動・ジャンプ操作
- 敵キャラクターのAI（発見・追跡・攻撃・巣への帰還）
- 敵キャラクターの脚の歩行アニメーション
- 採掘ツール・設置ツールと、素材で強化できるスキルツリー
- インベントリ、乗り物（ロボット）システム

### 使用技術

Unity 6 (URP) / C# / Shader Graph / Cinemachine / Input System / Blender

### 開発者

メルト・オッゼンジル（企画・プログラミング・3Dアセット制作をすべて一人で担当）