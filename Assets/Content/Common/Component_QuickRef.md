# MRCH Component Quick Reference

A summary of every component in the template. Find the full documentation in `Documentation.md`.

---

## Triggers — Make things happen when the player does something

| Component               | Path                                | What it does                                                                                                                                                                                                                   |
|-------------------------|-------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Interaction Trigger** | `MRCH/Interact/Interaction Trigger` | The main trigger hub. Enable any combination of: Collider (physics zone), Distance (proximity), LookAt (gaze), and lifecycle events (Start, OnEnable, Update, OnDisable). Each fires First Enter, Enter, and Exit UnityEvents. |

### Trigger Types (all inside Interaction Trigger)

| Trigger                | When it fires                                                                       |
|------------------------|-------------------------------------------------------------------------------------|
| **Collider Trigger**   | When the player physically enters/exits a trigger collider zone on the same object. |
| **Distance Trigger**   | When the player moves within/outside a set radius from the object.                  |
| **LookAt Trigger**     | When the player looks at the object (within a set angle and distance).              |
| **Start Trigger**      | Once, when the scene starts.                                                        |
| **On Enable Trigger**  | Each time the GameObject is activated.                                              |
| **Update Trigger**     | Every frame. Use sparingly.                                                         |
| **On Disable Trigger** | When the GameObject is deactivated.                                                 |

---

## Interaction — Let the player tap/click on objects

| Component | Path | What it does |
|---|---|---|
| **Touchable Manager** | `MRCH/Interact/Touchable Manager` | Scene-level manager that handles all touch/click input via raycast. One per scene. Fires a universal event on any touch. Can lock input until the player taps a specific object. |
| **Touchable Object** | `MRCH/Interact/Touchable Object` | Add to any object the player should be able to tap. Fires a UnityEvent on successful touch. Must be on the same layer as the Touchable Manager's layer mask. |

---

## Events — Organize and sequence your UnityEvents

| Component | Path | What it does |
|---|---|---|
| **Unity Event Library** | `MRCH/Interact/Unity Event Library` | A named library of single events and event sequences. Call events by name from anywhere with `TriggerEventByName` or `TriggerSequenceByName`. Sequences fire in order, with optional per-event delays. |

---

## Movement & Animation — Move and spin objects

| Component | Path | What it does |
|---|---|---|
| **Move & Rotate** | `MRCH/Objects/Move & Rotate` | Move an object to a target position and/or spin it continuously. Can start on Enable or after localization. Configurable easing, speed, and stop behavior (stop in place / return / snap). |
| **Lazy Follow** | `MRCH/Objects/Lazy Follow` | Smoothly follows a target (default: main camera) with lag, dead-zones, and configurable easing. Supports position follow and/or look-at rotation. The recommended way to make UI panels or labels face the player. |
| **Keep Facing To Cam** | `MRCH/Objects/Keep Facing To Cam` | Legacy billboard component — keeps an object facing the camera. For new work, prefer **Lazy Follow** with `Rotation: Look At`. |

---

## Visual FX — Fade, shine, and animate UI elements

| Component | Path | What it does |
|---|---|---|
| **Image Fade** | `MRCH/Image/Image Fade` | Fades a `RawImage`, `Image`, or `SpriteRenderer` in or out over a set duration. Can auto-deactivate after fading out. |
| **Text Shining** | `MRCH/Text/Text Shining` | Pulses a TMP text's opacity in a smooth cosine wave to create a glowing/breathing effect. |
| **Text Fade** | `MRCH/Text/Text Fade` | Fades a TMP text component in or out over a set duration. |

---

## Text — Display and animate text content

| Component | Path | What it does |
|---|---|---|
| **Simple TMP Typewriter** | `MRCH/Text/Simple TMP Typewriter` | Types text character-by-character into a TMP component with configurable speed, optional typing sound, and smart line-break detection. Can auto-play on enable, with a "first time only" option. |
| **TextMeshPro Helper** | `MRCH/Text/TextMeshPro Helper` | Exposes a single `ChangeContent(string)` method so you can update TMP text directly from a UnityEvent without writing a script. |

---

## Audio — Fade audio in and out

| Component | Path | What it does |
|---|---|---|
| **AudioController** | `MRCH/Audio/Audio Controller` | Fades an AudioSource in (with play) or out (with stop) over a set duration. Also lets you smoothly change volume to any target value. Better than calling `AudioSource.Play()` / `Stop()` directly. |

---

## Toast Notifications — Show pop-up messages

| Component | Path | What it does |
|---|---|---|
| **Toast Manager** | `MRCH/Toast/Toast Manager` | Singleton that shows a slide-in/hold/slide-out toast message. Call `ShowToast("your message")` from any UnityEvent. Pre-configured in the template scene — no setup needed. |
| **Default Toast Instance** | `MRCH/Toast/Default Toast Instance` | The visual prefab component for the toast. Holds the TMP text and optional icon. Already wired in the template. Replace or extend if you need a custom toast appearance. |

---

## Tasks — Track multi-step interactions

| Component | Path | What it does |
|---|---|---|
| **Multi Condition Event Manager** | `MRCH/Tasks/Multi Condition Event Manager` | Define a list of named tasks. Call `CompleteTask("id")` from any trigger or interaction. Fires events on each completion, on progress change (with count), and when all tasks are done. Supports prerequisites and optional retrigger. |
| **Multi Condition Progress UI Helper** | `MRCH/Tasks/Multi Condition Progress UI Helper` | Listens to a Condition Event Manager and updates a TMP text with the current progress, using a configurable format string like `{0} / {1} found`. |

---

## Utilities — General-purpose helpers

| Component | Path | What it does |
|---|---|---|
| **Object Toolset** | `MRCH/Objects/Object Toolset` | Exposes `ToggleComponentEnabled` and `ToggleObjectEnabled` as UnityEvent-callable methods. Toggle scripts, colliders, renderers, or entire GameObjects on/off without writing code. |
| **EventBroadcaster** | *(pre-placed in template scene)* | Bridges the Immersal SDK lifecycle to the rest of the template. Fires events when the SDK initializes and when localization succeeds. Required for "After Localized" triggers in Move & Rotate. Pre-wired — do not move or remove. |

---

## Editor Tools — Editor-only helpers, no runtime cost

| Component | Path | What it does |
|---|---|---|
| **XR Origin Editor Control** | `MRCH/Edit/XR Origin Editor Control` | Move and look around with W/A/S/D + mouse in Play Mode — no headset required for testing. Pre-placed on the XR Origin in the template. Fully stripped from builds. |
| **Editor Note** | `MRCH/Edit/Editor Note` | Attach notes and object references to any GameObject as reminders or instructions for yourself or teammates. Editor-only, stripped from builds. |
| **MR Area Label** | `MRCH/Edit/MR Area Label` | Draws a labeled wireframe box in the Scene view to mark named spatial zones. Useful for planning layouts. Editor-only. |
| **Map Model** | `MRCH/Edit/Map Model` | Attach to the visual/color mesh of your scanned environment. Automatically hides it from the Hierarchy and excludes it from builds — no need to manually delete or disable the map mesh before building. |
