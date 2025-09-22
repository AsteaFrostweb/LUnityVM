# LUnityVM

**LUnityVM** is a Unity plugin that allows you to create fully-featured virtual computers inside your Unity projects, powered by Lua scripting. Each computer has its own filesystem, event system, networked communication (LNet), and multi-instance shell terminals — perfect for moddable games, in-game scripting systems, or Minecraft-style Lua programmable computers.

---

## Features

- **Virtual Computers**
  - Each computer has a unique **ComputerID**, supporting multiple computers in the same scene.
  - Fully isolated **Lua virtual machine** per computer.

- **File System**
  - Virtual filesystem per computer for scripts, configs, and assets.
  - Lua-accessible API for reading, writing, and listing files and directories.

- **Event System**
  - Internal event handling for each computer.
  - Scripts can subscribe to and dispatch events.

- **Inter-Computer Communication (LNet)**
  - Broadcast or targeted events between computers in the same scene.

- **Shell Terminals**
  - Multiple shell instances per computer.
  - Shells run asynchronously off the main Unity thread.
  - All calls that interact with Unity are safely dispatched to the main thread.

---

## Dependencies

- **Unity** 2021.3 LTS or newer  
- **[NLua](https://github.com/NLua/NLua)** – Lua interpreter for .NET/Unity  
- **[KeraLua](https://github.com/NLua/KeraLua)** – Lua VM backend for NLua  

---

## Installation

1. Clone or download this repository into your Unity `Packages` or `Assets` folder.
2. Import NLua and KeraLua (via Unity Package Manager or DLLs).
3. Add a `ComputerManager` prefab or component to your scene to start creating virtual computers.

---

## Getting Started

```csharp
using LUnityVM;

public class Example : MonoBehaviour
{
    void Start()
    {
        // Create a new computer
        var computer = ComputerManager.CreateComputer();

        // Run a Lua script in a shell
        var shell = computer.OpenShell();
        shell.RunScript("print('Hello from Lua!')");

        // Send an event to another computer
        computer.LNet.BroadcastEvent("HelloEvent", "Hello from Computer 1!");
    }
}
```

## Planned Features / Roadmap

 - Save/load virtual filesystem state.

 - Lua modules per computer.

 - Customizable terminal UI components.

 - Advanced LNet networking (optional cross-scene or cross-project).

## Contributing

Pull requests and issues are welcome! Ideas for new Lua APIs, virtual computer features, or shell enhancements are highly encouraged.
