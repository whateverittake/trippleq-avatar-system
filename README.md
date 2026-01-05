

# Avatar System

## Setup (Inject from game)
1. Create `AvatarDatabaseSO` asset in your project (Assets/...).
2. Create a game-side installer that initializes `AvatarService` and provides it to your UI.
3. UI subscribes to `OnAvatarChanged` and `OnInventoryChanged`.

## Notes
- Sprites/icons should live in the game project (Assets/...), not inside this package.