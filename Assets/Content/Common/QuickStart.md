# Quick Start — MRCH Template

## Before You Start

1. **Use a Markdown reader.** This file and `Documentation.md` are written in Markdown. A reader with a table of contents makes navigation much easier. Try [MarkText](https://github.com/marktext/marktext) (free), [Typora](https://typora.io/) ($15), or the Markdown preview in your IDE.

2. **Use [GitHub Desktop](https://desktop.github.com/)** for version control — it makes pulling updates from the template and collaborating with your team much easier.

3. **Do not modify anything inside `Assets/Content/Common/Scripts/`.** These are shared across all teams. If you need to customize a component's behavior, you can inherit from its abstract base class and override specific methods. See the *Extending Scripts* section in `Documentation.md` for guidance.

4. **Create your team folder at `Assets/Content/[YOUR_TEAM_NAME]/`** and put all your work there — Models, Prefabs, Scenes, Scripts, etc. Keep it organized as you go; it makes submission much easier. (Your team name can be anything — just let Shengyang know it's you.)

---

## Setting Up Your Scene

1. Find your assigned map scene under `Assets/Content/Common/Scenes/MapTemplate/`.
2. **Copy** it (do not move or cut) to `Assets/Content/[YOUR_TEAM_NAME]/Scenes/`.
3. Rename it to something unique, then double-click to open it.
4. You're ready to start building.

---

## What's Already in the Scene

Your template scene comes pre-configured with:

- **XR Origin** with the Immersal localizer and the `XR Origin Editor Control` component for testing in the Editor without a headset.
- **EventBroadcaster** already wired to the Immersal SDK and Localizer — this is what allows components like `Move & Rotate`'s "After Localized" modes to work. You don't need to touch it.
- **Toast Canvas** with a pre-configured `Toast Manager` and toast prefab — call `ShowToast("message")` from any UnityEvent to display a notification.
- **Map Data with Occlusion** 

---

## Testing in the Editor (Before Build)

The template includes `XR Origin Editor Control` on the XR Origin. In Play Mode:

| Control | Action |
|---|---|
| W / A / S / D | Move forward / left / back / right |
| Q / E | Move down / up |
| Shift | Hold for fast move |
| I / K | Pitch up / down |
| J / L | Yaw left / right |
| Right-click + drag | Free-look with mouse |

A control hint overlay appears in the bottom-right corner of the Game view. Click **x** to collapse it.

---

## Adding Your First Interaction

> **If you haven't used Unity colliders or triggers before**, read these first — they're short and essential:
> - [Colliders and Triggers — Understanding the Basics](https://christopherhilton88.medium.com/colliders-and-triggers-in-unity-understanding-the-basics-7192714f3440)
> - [Unity Docs — Collider](https://docs.unity3d.com/ScriptReference/Collider.html)

### Step 1 — Create a trigger zone

1. In the Hierarchy, right-click **XR Space** → **3D Object → Cube**.
2. Rename it to something descriptive (e.g. `TriggerZone_Entrance`).
3. Disable the **Mesh Renderer** component — the cube only needs to work as an invisible zone.
4. On the **Box Collider**, check **Is Trigger**.

### Step 2 — Add the Interaction Trigger component

1. With the cube selected, click **Add Component** at the bottom of the Inspector.
2. Navigate to **MRCH → Interact → Interaction Trigger**, or search for it by name.
3. Enable **Use Collider Trigger**.

### Step 3 — Wire your UnityEvents

The Interaction Trigger now shows three events: `On Trigger First Enter`, `On Trigger Enter`, and `On Trigger Exit`.

- Click **+** on the event you want to use.
- Drag a GameObject from the Hierarchy into the object slot.
- Use the dropdown to select the method to call.

> **New to UnityEvents?** These resources explain how they work:
> - [Unity Docs — UnityEvents](https://docs.unity3d.com/Manual/UnityEvents.html)
> - [UnityEvents Explained in 4 Minutes](https://www.youtube.com/watch?v=djW7g6Bnyrc)

---

## What Can I Do With This Template?

See `Component_QuickRef.md` for a one-page summary of every available component. A few common patterns:

| I want to… | Use |
|---|---|
| Trigger something when the player walks into a zone | `Interaction Trigger` → Collider Trigger |
| Trigger something when the player gets close | `Interaction Trigger` → Distance Trigger |
| Trigger something when the player looks at an object | `Interaction Trigger` → LookAt Trigger |
| Let the player tap/click on a 3D object | `Touchable Manager` + `Touchable Object` |
| Move an object to a position | `Move & Rotate` → `MoveForOnce()` |
| Make a UI panel always face the player | `Lazy Follow` with Rotation: Look At With World Up |
| Show a pop-up message | `Toast Manager` → `ShowToast("text")` |
| Type out text character by character | `Simple TMP Typewriter` |
| Track whether the player completed multiple steps | `Multi Condition Event Manager` |
| Play audio with a fade-in | `Audio Controller` → `FadeInAudioToTargetVolume()` |
| Toggle an object on/off from a trigger | `Object Toolset` → `ToggleObjectEnabled()` |

---

## Useful References

- **`Documentation.md`** — full reference for every component, all Inspector fields, and public methods
- **`Component_QuickRef.md`** — one-page component lookup with Add Component paths
- **`ReleaseNotes.md`** — version history and changelog