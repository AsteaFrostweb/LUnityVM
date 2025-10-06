# LUnityVM

**LUnityVM** is a Unity plugin that allows you to create fully-featured virtual computers inside your Unity projects, powered by Lua scripting. Each computer has its own filesystem, event system, networked communication (LNet), and multi-instance shell terminals — perfect for moddable games, in-game scripting systems, or InGame Lua programmable computers.

---

## Features

- **Virtual Computers**
  - Each computer has a unique **ComputerID**, supporting multiple computers in the same scene.
  - Fully isolated **Lua virtual machine** per computer.

- **File System**
  - Virtual filesystem per Shell instance for file IO.
  - Lua-accessible API for reading, writing, and listing files and directories.

- **Event System**
  - Internal event handling for each computer.
  - Scripts can subscribe to and dispatch events.

- **Inter-Computer Communication (LNet)**
  - Send and receive "luanet" event from one pc to another
  - Includes Port specification to allow communications with individual shell instances on a single computer

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

1. Clone or download this repository to your machine.
2. Add it as a project via Unity Hub.
3. Load project and Sample Scene.
4. (IF NEEDED) Import NLua and KeraLua (Follow guide depending on Unity version and OS).

   

---

## Getting Started

- If youre already reading this then I'm sure you can figure it out.

## Planned Features / Roadmap

- Improved Shell features (Cursor, More commands)
- Complete File System API
- Create some test projects to showcase package

## Contributing

Pull requests and issues are welcome! Ideas for new Lua APIs, virtual computer features, or shell enhancements are highly encouraged.
