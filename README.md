# MRCH MR Template

A Unity + Immersal template for building Mixed Reality experiences, developed for the **Mixed Reality and Cultural Heritage** course at NYU Shanghai.

**Unity**: `6000.0.65f1` · **Immersal**: `2.3.0`

> In theory the template should work on newer Unity versions (2022 LTS, Unity 6.1+) but they have not been tested.

---

## Before You Start

1. **Read `QuickStart.md` first** — it walks you through opening your scene, setting up a folder, and wiring your first interaction. Find it at `Assets/Content/Common/QuickStart.md` or on [GitHub](https://github.com/DHL-NYUSH/MRCH-MR-Template/blob/main/Assets/Content/Common/QuickStart.md).

2. **Refer to `Documentation.md` as needed** — it covers every component, its Inspector fields, and public methods. Find it at `Assets/Content/Common/Documentation.md` or on [GitHub](https://github.com/DHL-NYUSH/MRCH-MR-Template/blob/main/Assets/Content/Common/Documentation.md).

3. **Use a Markdown reader** — both files are written in Markdown. A reader with automatic table of contents support will save you a lot of scrolling. Good options:
    - [MarkText](https://github.com/marktext/marktext) — free, cross-platform
    - [Typora](https://typora.io/) — $15, polished
    - Most IDEs (VS Code, Rider) have Markdown preview built-in or via a free extension

---

## Getting Started

See `QuickStart.md` for the full walkthrough. In short:

1. Find your assigned map scene under `Assets/Content/Common/Scenes/MapTemplate/`.
2. Copy it to `Assets/Content/[YOUR_TEAM_NAME]/Scenes/` and rename it.
3. Create your content under `Assets/Content/[YOUR_TEAM_NAME]/` — organize into subfolders (Models, Prefabs, Scenes, Scripts, etc.) as you go.
4. **Do not modify anything inside `Assets/Content/Common/Scripts/`.** Inherit from the abstract base classes if you need custom behavior.

---

## Template Structure

```
Assets/
└── Content/
    ├── Common/
    │   ├── QuickStart.md
    │   ├── Documentation.md
    │   ├── Component_QuickRef.md   ← one-page component lookup
    │   ├── Scenes/
    │   │   └── MapTemplate/        ← your assigned scene is here
    │   └── Scripts/                ← do not modify
    └── [YOUR_TEAM_NAME]/           ← all your work goes here
```

---

## Component Overview

The template provides ready-to-use components for:

- **Triggers** — fire UnityEvents when the player enters a zone, gets close, looks at an object, or on any Unity lifecycle event (`Interaction Trigger`)
- **Touch / Click** — detect player taps on 3D objects (`Touchable Manager` + `Touchable Object`)
- **Movement** — move and rotate objects with easing, with optional localization-aware start modes (`Move & Rotate`, `Lazy Follow`)
- **Text** — typewriter effect, fade, shining animation, and a quick content-swap helper (`Simple TMP Typewriter`, `Text Fade`, `Text Shining`, `TMP Helper`)
- **Audio** — fade in/out an AudioSource (`Audio Controller`)
- **Visuals** — fade images and sprites (`Image Fade`)
- **Toast** — show pop-up notifications (`Toast Manager`)
- **Tasks** — track multi-step interactions and fire completion events (`Multi Condition Event Manager`)
- **Utilities** — toggle objects and components from UnityEvents, named event sequences, and more

See `Component_QuickRef.md` for a one-line description of every component and its Add Component path.

---

## Maintainer

Written and maintained by **Shengyang Peng**.  
For bugs or feature suggestions:

- Email: [billy.peng@nyu.edu](mailto:billy.peng@nyu.edu)
- GitHub Issues: [MRCH-MR-Template/issues](https://github.com/DHL-NYUSH/MRCH-MR-Template/issues)

---

## License

Released under the **MIT License** — see `LICENSE` for details.  
If you use this template, attribution is appreciated. Please mention `MRCH-MR-Template` and include the license file in your project.