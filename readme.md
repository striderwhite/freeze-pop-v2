# Freeze Pop V2

| Generic Memory LookerAtter'

Uses Pinvoke to expose low level windows API calls to open and modifiy a running processes's memory values directly.

Right now values will be hardcoded for dev purposes, but eventually this will be a forkable template for specific game related bots/trainers.

# Todo

- Create a poiner chain dereferencing method
- - Dereference N sized pointer chains
- Create a thread-dispatchable freeze method
- - This will "freeze"/write an arbirary value into a given memory location
- - Dispach threads for each freezing action
- Create a menu system

# Goals

### Pointers

- Health
- Ammo
- Viewport/Camera vectors
- Position vectors
- Recoil

### Cheats

- Functional aimbot
- ESP
- Perfect Recoil
- Godmode
- Infinite Ammo

### Project

- Establish this as a template for trainers

# Run/Build

```bash
dotnet build

dotnet run
```
