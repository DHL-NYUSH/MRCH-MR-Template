# MRCH Template Documentation + Manual

## Tips Before You Start

- Please read the `QuickStart.md` file in the same folder before proceeding.
- Some technical details are mentioned in `Scripts/Readme.md`.

---

## Where to Find These Components

All components are organized under the **MRCH** category in the Add Component menu. Each section below lists the exact menu path for that component, e.g.:

> **Inspector → Add Component → MRCH → Interact → Interaction Trigger**

You can also search by name directly:

> **Inspector → Add Component → (type the component name)**

## Interaction Triggers

> **Add Component → MRCH → Interact → Interaction Trigger**

A single component that provides multiple types of spatial and lifecycle event triggers. Enable only the trigger types you need — the Inspector will hide unneeded fields automatically.

> **Note on Distance & LookAt performance**: For efficiency, Distance and LookAt checks do not run every frame. They are sampled roughly every 25 frames (approximately 2 times per second at 60 fps). This is sufficient for most MR experiences. Collider triggers are physics-driven and are not affected by this.

### Collider Trigger

- **Requirement**: The same GameObject must have a `Collider` component with `Is Trigger` enabled. A warning is logged in the Console if this is missing.
- Variables:

  - `Use Collider Trigger` (boolean): Enables the trigger functionality.
- Events:

  - `On Trigger First Enter`: Fires **once**, the first time the player enters the collider. Resets when the scene reloads.
  - `On Trigger Enter`: Fires every time the player enters the collider.
  - `On Trigger Exit`: Fires every time the player exits the collider.

### Distance Trigger

- **Requirement**: None.
- Variables:

  - `Use Distance Trigger` (boolean): Enables the trigger functionality.
  - `Distance` (float, meters): The radius within which events are triggered. Be aware that the physical-to-scanned-map scale ratio may differ — test in your scanned environment before finalizing values.
- Events:

  - `On Distance First Enter`: Fires **once**, the first time the player enters the distance range. Resets when the scene reloads.
  - `On Distance Enter`: Fires every time the player enters the distance range.
  - `On Distance Exit`: Fires when the player moves outside the distance range.

### LookAt Trigger

- **Requirement**: None.
- Variables:

  - `Use LookAt Trigger` (boolean): Enables the trigger functionality.
  - `Look At Angle` (float, degrees): The maximum angle between the player's forward direction and the direction toward this object for the "looking at" condition to be met. A smaller value means the player must aim their gaze more precisely at the object.
  - `Look At Distance` (float, meters): The maximum distance for the look-at check to be active. Set to **0 for unlimited range** (no distance restriction applies).
- Events:

  - `On Look At First Enter`: Fires **once** when both conditions are met (within distance AND within angle) for the first time. Resets when the scene reloads.
  - `On Look At Enter`: Fires each time both conditions are met.
  - `On Look At Distance Exit`: Fires when the player exits the `Look At Distance` range. The angle condition is **not** checked on exit — only distance matters here.

> **Scene Gizmo tip**: When `Show Gizmos` is on and `Use LookAt Trigger` is enabled, the Scene view draws a color-coded visualization: **green** = both conditions met (in range + looking at), **blue** = only one condition met, **red** = neither.

---

### Events Triggers

All lifecycle event triggers are grouped under a single `Use Events Triggers` toggle. Enable it first to reveal the individual sub-triggers below.

#### Start Trigger

- Variables:

  - `Use Start Trigger` (boolean): Enables this sub-trigger.
- Events:

  - `On Start`: Fires during `Start()` — once, on the first frame the script is active, before any `Update()` calls. [(ref)](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html)

#### On Enable Trigger

- Variables:

  - `Use On Enable Trigger` (boolean): Enables this sub-trigger.
- Events:

  - `On Enable`: Fires during `OnEnable()` — each time the GameObject is activated or re-enabled. [(ref)](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html)

#### Update Trigger

> ⚠️ Use this sparingly. Firing a UnityEvent every frame is expensive and usually unnecessary — consider Distance or LookAt triggers instead.

- Variables:

  - `Use Update Trigger` (boolean): Enables this sub-trigger.
- Events:

  - `On Update`: Fires every frame during `Update()` while the MonoBehaviour is enabled. [(ref)](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html)

#### On Disable Trigger

- Variables:

  - `Use On Disable Trigger` (boolean): Enables this sub-trigger.
- Events:

  - `On Disable`: Fires during `OnDisable()` — when the GameObject is deactivated or the component is destroyed. [(ref)](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html)

---

### Setting (Interaction Trigger)

#### Show Gizmos

- **Type**: boolean — Draws visualization in the Scene view. For Distance Trigger: a yellow wire sphere. For LookAt Trigger: a color-coded sphere, a line from camera to object, and an angle/distance readout. Gizmos are only visible when the GameObject is selected.

#### Debug Mode

- **Type**: boolean — If enabled, every event that fires prints a message to the Console via `Debug.Log()`. Useful for verifying your event wiring during development.

---

## Unity Event Library

> **Add Component → MRCH → Interact → Unity Event Library**

The Unity Event Library is a centralized store for named events and event sequences on a single GameObject. Instead of wiring triggers directly to target functions everywhere, you can register events here by name and then call `TriggerEventByName` or `TriggerSequenceByName` from any other component's UnityEvent — keeping your wiring organized and readable.

### Named Unity Event

A single named UnityEvent with an optional delay.

- Variables:

  - `eventName` (string): A unique name to identify this event. Used as the key when calling `TriggerEventByName`.
  - `waitBeforeInvoke` (float, seconds): Delay before the event fires. The calling code waits this many seconds before invoking the UnityEvent.
  - `unityEvent` (UnityEvent): The Unity event to invoke.

### Named Unity Event Sequence

A named list of `NamedUnityEvent` entries that fire one after another, in order.

> ⚠️ Events in a sequence run **sequentially, not in parallel**. Each event waits for the previous one's `waitBeforeInvoke` delay to finish before starting.

- Variables:

  - `sequenceName` (string): A unique name to identify this sequence. Used as the key when calling `TriggerSequenceByName`.
  - `namedUnityEventSequence` (NamedUnityEvent[]): The ordered list of events. Each entry can have its own `waitBeforeInvoke` delay before it fires.

### Public Methods

##### TriggerEventByName(string eventName)

- Finds the `NamedUnityEvent` with a matching name and invokes it (after its `waitBeforeInvoke` delay). Logs a warning in the Console if no matching name is found.

##### TriggerSequenceByName(string sequenceName)

- Finds the `NamedUnityEventSequence` with a matching name and runs all its events in order. Each event waits for its own `waitBeforeInvoke` delay before firing. Logs a warning in the Console if no matching name is found.

---

## Move and Rotate

> **Add Component → MRCH → Objects → Move & Rotate**

> **Localization note**: "After Localized" modes depend on the Immersal SDK successfully recognizing the physical location. See the `QuickStart.md` for how localization works.

Animates a GameObject's position and/or rotation using DOTween. The **initial position and rotation** are captured each time the object is enabled — this is the reference point that `MoveBackForOnce()`, `JumpBackToInitialPosition()`, and all "return to origin" behaviors use.

### Inspector Variables

#### Move Options

##### Move Start Mode

- **Type**: `enum` (toggle buttons) — Determines what movement, if any, starts automatically. Options:
  - `None` — No automatic movement. Call movement methods manually from a UnityEvent.
  - `For Once On Enable` — Moves to `Move Target` once when the object is enabled.
  - `Forth And Back On Enable` — Continuously moves back and forth between the initial position and `Move Target` when enabled.
  - `For Once After Localized` — Moves to `Move Target` once after the first successful localization.
  - `Forth And Back After Localized` — Continuously moves back and forth after the first successful localization.

##### Move Target

- **Type**: `Transform` — The destination the object moves toward. **Required** for any move mode other than `None`.

##### Move Speed

- **Type**: `float` (meters per second) — Controls how fast the object travels. The actual duration is calculated from the distance to the target divided by this speed, so farther targets will take longer at the same speed.

##### Move Type

- **Type**: `Ease` (DOTween) — The animation curve applied to movement. Default is `InOutSine`. Try others to get different feels — `Linear` for mechanical, `OutBounce` for playful, etc.

##### On Stop (Move)

- **Type**: `enum` — What happens when `StopMovement()` is called:
  - `Stop In Place` — Freezes the object at its current position.
  - `Return To Origin` — Smoothly moves back to the initial position.
  - `Snap To Origin` — Instantly jumps back to the initial position.

#### Rotate Options

##### Rotate Start Mode

- **Type**: `enum` (toggle buttons) — Options:
  - `None` — No automatic rotation.
  - `On Enable` — Starts rotating continuously when the object is enabled.
  - `After Localized` — Starts rotating continuously after the first successful localization.

##### Rotation Axis

- **Type**: `Vector3` — The axis around which the object rotates. `(0, 1, 0)` = spin around Y (vertical), `(1, 0, 0)` = tip forward/back, `(0, 0, 1)` = roll.

##### Rotate Duration

- **Type**: `float` (seconds) — Time for one full 360° rotation.

##### Rotate Type

- **Type**: `Ease` (DOTween) — Animation curve for rotation. Default is `Linear` (constant spin).

##### On Stop (Rotate)

- **Type**: `enum` — What happens when `StopRotation()` is called. Same three options as On Stop (Move): `Stop In Place`, `Return To Origin`, `Snap To Origin`.

#### Setting

##### Show Gizmos

- **Type**: `boolean` — If enabled, draws a cyan line from this object to the `Move Target` in the Scene view when the object is selected.

### Public Methods

##### MoveForOnce()

- Moves the object to `Move Target` once, using `Move Speed` and `Move Type`.

##### MoveBackForOnce()

- Smoothly moves the object back to its initial position (the position captured when the object was last enabled).

##### JumpBackToInitialPosition()

- Instantly teleports the object back to its initial position, with no animation.

##### MoveForthAndBack()

- Continuously moves the object back and forth between its initial position and `Move Target` in a loop. Equivalent to `Forth And Back` start modes.

##### RotateObject()

- Starts a continuous rotation loop around `Rotation Axis`, with each full cycle taking `Rotate Duration` seconds.

##### RotateBackToOrigin()

- Smoothly rotates the object back to its initial rotation (captured on enable). Duration is proportional to how far off it currently is.

##### JumpBackToInitialRotation()

- Instantly snaps the object back to its initial rotation, with no animation.

##### StopMovement()

- Stops movement according to the `On Stop (Move)` setting.

##### StopRotation()

- Stops rotation according to the `On Stop (Rotate)` setting.

---

## Object Toolset

> **Add Component → MRCH → Objects → Object Toolset**

A small utility component that exposes common GameObject and component operations as public methods, so they can be called directly from UnityEvents without writing any code.

### Public Methods

##### ToggleComponentEnabled(Component)

- **Parameter**: `Component` — The component to toggle.
- **Description**: Flips the `enabled` state of a `Behaviour` component (e.g. disabling an `Animator`, a `Collider`, or any script). If the component does not have an `enabled` property, a warning is logged.

##### ToggleObjectEnabled(GameObject)

- **Parameter**: `GameObject` — The object to toggle.
- **Description**: Flips the active state of a GameObject (`SetActive`). Deactivating a GameObject also deactivates all of its children. Note: children remember their own `activeSelf` state, so they will return to it when the parent is reactivated.

---

### Image Fade

> **Add Component → MRCH → Image → Image Fade**

> **Requires**: A `Raw Image`, `Image`, or `SpriteRenderer` component on the same GameObject. If none is found, or more than one is found, a warning will be logged in the Console.

This component allows fading effects on a `Raw Image`, `Image`, or `SpriteRenderer`.

#### Inspector Variables

##### secondsToFade

- **Type**: `float` (seconds)
- **Description**: The duration of the fade in or fade out effect.

##### fadeInOnAwake

- **Type**: `boolean`
- **Description**: If enabled, the image starts fully transparent and fades in over `secondsToFade` each time the GameObject is enabled or re-enabled.

##### deactivateItAfterFading

- **Type**: `boolean`
- **Description**: If enabled, the GameObject will automatically deactivate itself after a `Fadeout()` completes (i.e., when the alpha reaches zero).

#### Public Methods

##### FadeIn()

- **Description**: Sets the image to fully transparent, then fades it in to full opacity over the `secondsToFade` duration.

##### Fadeout()

- **Description**: Sets the image to fully opaque, then fades it out to fully transparent over the `secondsToFade` duration. If `deactivateItAfterFading` is enabled, the GameObject deactivates itself when the fade completes.

##### SetTimeToFade(float)

- **Parameter**: `float` — New fade duration in seconds.
- **Description**: Updates the `secondsToFade` value.

---

### Lazy Follow

> **Add Component → MRCH → Objects → Lazy Follow**

Makes a GameObject smoothly follow a target (defaulting to the main camera) with a configurable lag and dead-zone threshold. This is the recommended replacement for `Keep Facing To Cam`, and can also handle positional following with the same smoothing logic.

> **Tip**: To replicate `Keep Facing To Cam` behavior, set `Position Follow Mode: None` and `Rotation Follow Mode: Look At` (or `Look At With World Up`). You get the same effect with smoother interpolation and more tuning options.

#### Target Config

##### Target

- **Type**: `Transform` (optional)
- **Description**: The object to follow. If left empty, defaults to the main camera's transform on enable.

##### Target Offset

- **Type**: `Vector3`
- **Description**: A positional offset relative to the target. Defaults to `(0, 0, 0.5)` — half a meter in front of the target. Adjust to reposition the follower relative to the camera/target.

##### Follow In Local Space

- **Type**: `boolean`
- **Description**: If enabled, reads and writes positions in local space instead of world space. Not compatible with `LookAt` or `LookAtWithWorldUp` rotation modes — the component will disable this automatically with a warning if you try.

##### Apply Target In Local Space

- **Type**: `boolean`
- **Description**: If enabled, the final computed position/rotation is written to `localPosition`/`localRotation` instead of world space.

#### General Follow Params

##### Movement Speed

- **Type**: `float`
- **Description**: How quickly the object interpolates toward its target. Lower values = more lag and a floaty feel. Higher values = snappier, closer to the target.

##### Movement Speed Variance

- **Type**: `float` (0–0.999)
- **Description**: Adds a speed range around `Movement Speed`. At `0`, speed is constant. At `0.25` with speed `6`, the object moves between `4.5` (when far) and `7.5` (when close), creating a natural deceleration as it approaches.

##### Snap On Enable

- **Type**: `boolean`
- **Description**: If enabled, the object instantly snaps to the target position/rotation when the GameObject is activated, skipping the smooth interpolation. Prevents the object from sliding in from a stale position.

#### Position Follow Params

##### Position Follow Mode

- **Type**: `enum`
  - `None` — Position is not followed. Use this if you only want rotation following.
  - `Follow` — Smoothly follows the target position with the offset applied.

##### Min / Max Distance Allowed

- **Type**: `float` (meters)
- **Description**: The dead-zone for position following. The follower only starts moving toward the target when the distance exceeds the current threshold. The threshold ramps from `Min` to `Max` over `Time Until Threshold Reaches Max Distance` seconds of the target being still — creating the "lazy" drift-back behavior.

##### Time Until Threshold Reaches Max Distance

- **Type**: `float` (seconds)
- **Description**: How long the target needs to stay still before the maximum dead-zone distance is used. A value of `3` means after 3 seconds of no target movement, the follower needs to drift `Max Distance` before it starts catching up again.

#### Rotation Follow Params

##### Rotation Follow Mode

- **Type**: `enum`
  - `None` — Rotation is not followed.
  - `Look At` — Rotates to face the target (free on all axes). Best with main camera as target.
  - `Look At With World Up` — Same as `Look At` but locks the Y axis so the object stays upright.
  - `Follow` — Matches the target's rotation exactly (with smoothing).

##### Min / Max Angle Allowed

- **Type**: `float` (degrees)
- **Description**: The dead-zone for rotation following, equivalent to Min/Max Distance for position. The follower only rotates when the angle difference exceeds the current threshold.

##### Time Until Threshold Reaches Max Angle

- **Type**: `float` (seconds)
- **Description**: Same ramp behavior as the position equivalent, applied to the angle threshold.

#### Public Methods

##### SnapToTarget()

- **Description**: Immediately snaps the object to the current target position and rotation, bypassing all smoothing and dead-zones. Available as a button in the Inspector during Play Mode.

---

### Keep Facing To Cam

> **Add Component → MRCH → Objects → Keep Facing To Cam**

> **Legacy note**: This component still works but is superseded by **Lazy Follow** with `Position Follow Mode: None` and `Rotation Follow Mode: Look At` or `Look At With World Up`. Lazy Follow provides smoother behavior and more settings. Use `Keep Facing To Cam` only for simple cases where you don't need the extra controls.

Makes a GameObject always rotate to face the main camera. Useful for flat billboards (info panels, labels) that should always be readable from the player's point of view.

> ⚠️ If the player can walk directly onto or underneath the object, the facing direction may flip unexpectedly. In that case, enable `Lock Y Axis`.

#### Inspector Variables

##### Lock Y Axis

- **Type**: `boolean`
- **Description**: If enabled, the object only rotates horizontally (around the Y axis). The object will always stay upright, facing the camera on the horizontal plane. Recommended for most floor-level or wall-mounted panels.

##### Face To Cam On Enable

- **Type**: `boolean`
- **Description**: If enabled, the object starts facing the camera as soon as the GameObject is activated.

#### Public Methods

##### SetFaceToCam(bool)

- **Parameter**: `bool` — `true` to start facing the camera, `false` to stop.
- **Description**: Enables or disables the facing behavior at runtime. Wire `false` to freeze the object's rotation in place, and `true` to resume.

---

### Text Shining

> **Add Component → MRCH → Text → Text Shining**

> **Requires**: A `TextMeshPro - Text` or `TextMeshPro - Text (UI)` component on the same GameObject. A warning is logged in the Console if neither or both are found.

Animates the opacity of a TMP text component in a smooth cosine pulse, creating a "shining" or breathing glow effect.

#### Inspector Variables

##### cycleTime

- **Type**: `float` (seconds)
- **Description**: Duration of one full shine cycle (fade out → fade in → fade out). A value of `2.0` means the text takes 2 seconds to complete one full pulse.

##### playOnAwake

- **Type**: `boolean`
- **Description**: If enabled, the shining effect starts immediately when the GameObject is activated.

#### Public Methods

##### SetShiningText(bool)

- **Parameter**: `bool` — `true` to start the effect, `false` to stop it.
- **Description**: Starts or stops the shining animation. Stopping it does not reset the text's alpha, so you may want to pair this with a `TextFade` if you need to restore full opacity.

---

### Simple TMP Typewriter

> **Add Component → MRCH → Text → Simple TMP Typewriter**

> **Requires**: A `TextMeshPro - Text` or `TextMeshPro - Text (UI)` component on the same GameObject. A warning is logged in the Console if neither or both are found. For the typing sound, an `AudioSource` is also required on the same GameObject.

Simulates a typewriter effect by revealing text one character at a time in a TMP component.

> **Important**: Do not type your content directly into the TMP component's text field. It will be cleared when typing starts. Use the **Content to Type** field on this component instead.

#### Inspector Variables

##### Content to Type

- **Type**: `string` (multi-line text area)
- **Description**: The text content that will be typed out. Write your full text here.

##### Type Speed

- **Type**: `float` (seconds)
- **Description**: The delay between each typed character. Lower = faster typing. `0.1` means one character every 0.1 seconds.

#### Settings

##### Start New Line When Overflow

- **Type**: `boolean`
- **Description**: If enabled, the component measures the current line width before typing the next word, and inserts a line break in advance if the word would overflow. Recommended for English and most Latin-script languages. You can disable it for Chinese or other character-based scripts where every character is the same width.

##### Type On Enable

- **Type**: `boolean`
- **Description**: If enabled, typing starts automatically each time the GameObject is enabled or re-enabled — unless `Only Type For The First Time` is on and it has already been played.

##### Only Type For The First Time

- **Type**: `boolean` — Only visible when `Type On Enable` is enabled.
- **Description**: If enabled, `Type On Enable` will only trigger once. On subsequent enables, `FinishTyping()` is called immediately (showing the full text instantly). The "played" state is tracked in memory by default, and resets when the scene reloads — unless `Save Cross Scene` is also enabled.

##### Save Cross Scene

- **Type**: `boolean` — Only visible when `Only Type For The First Time` is enabled.
- **Description**: If enabled, the "already played" state is saved to `PlayerPrefs` (keyed to the GameObject's name and the scene name), so it persists across scene reloads and app restarts. Clear this by deleting PlayerPrefs or renaming the GameObject.

##### Type Sound

- **Type**: `AudioClip` (optional)
- **Description**: A sound clip played on every typed character. Requires an `AudioSource` on the same GameObject. If no `AudioSource` is found, the sound is silently skipped.

#### Public Methods

##### StartTyping()

- **Description**: Starts the typewriter effect using the `Content to Type` field. If typing is already in progress, it is interrupted and restarts from the beginning.

##### StartTyping(string content)

- **Parameter**: `string` — Custom text content to type.
- **Description**: Starts the typewriter effect with a provided string instead of the `Content to Type` field. Useful for dynamically changing text at runtime from a script.

##### FinishTyping()

- **Description**: Immediately stops the typing animation and displays the full `Content to Type` text in the TMP component.

---

### Text Fade

> **Add Component → MRCH → Text → Text Fade**

> **Requires**: A `TextMeshPro - Text` or `TextMeshPro - Text (UI)` component on the same GameObject. An error is logged and the component disables itself if no TMP component is found.

Fades a TMP text component's alpha in or out over a set duration.

#### Inspector Variables

##### Fade Duration

- **Type**: `float` (seconds)
- **Description**: The default duration for fade in and fade out transitions. Can be overridden per-call using the method parameters.

#### Public Methods

##### FadeIn(float fadeDurationParam = 0)

- **Parameter**: `float` (optional) — Override duration in seconds. If `0` or omitted, uses the `Fade Duration` inspector value.
- **Description**: Fades the text from its current alpha to fully opaque (`alpha = 1`).

##### FadeOut(float fadeDurationParam = 0)

- **Parameter**: `float` (optional) — Override duration in seconds. If `0` or omitted, uses the `Fade Duration` inspector value.
- **Description**: Fades the text from its current alpha to fully transparent (`alpha = 0`).

---

### TextMeshPro Helper

> **Add Component → MRCH → Text → TextMeshPro Helper**

> **Requires**: A `TMP_Text` component on the same GameObject (marked as Required — Unity will warn if missing).

A minimal utility component that exposes a single method for changing TMP text content from a UnityEvent.

#### Public Methods

##### ChangeContent(string content)

- **Parameter**: `string` — The new text to display.
- **Description**: Sets the TMP component's text to the provided string. Wire this to any UnityEvent and use the string parameter to update displayed text without writing a script.

---

### Touchable Manager

> **Add Component → MRCH → Interact → Touchable Manager**

> **Setup required**: The `Touchable Manager` must exist once in the scene. All objects the player can touch must: (1) have a `Collider` on them, (2) have a `Touchable Object` component on them, and (3) be assigned to the **same Unity Layer** as the `Touchable Layer` field on this component.

Manages raycast-based touch and click input for all touchable objects in the scene. Works with both touchscreen input (on device) and mouse clicks (in Editor play mode).

#### Inspector Variables

##### Touchable Layer

- **Type**: `LayerMask`
- **Description**: The Unity Layer that all touchable objects must be assigned to. Objects on other layers are ignored by the raycast. Assign the same layer to both this field and all GameObjects that carry a `Touchable Object` component.

##### Universal Touch Event

- **Type**: `UnityEvent`
- **Description**: Fires whenever **any** touchable object is successfully touched. Useful for playing a shared sound or triggering a global response.

#### Settings

##### Touch Range

- **Type**: `float`, range: 1–60 (meters)
- **Description**: The maximum raycast distance for a touch to register. Objects further than this distance will not respond.

##### Click Interval

- **Type**: `float` (seconds)
- **Description**: Minimum time required between successive touches. If the player taps again before this interval has passed, the touch is ignored and `Failed To Click Event` fires instead. Use this to prevent accidental double-taps.

##### Failed To Click Event

- **Type**: `UnityEvent`
- **Description**: Fires when a touch attempt is rejected because it occurred within the `Click Interval` cooldown. Wire a short audio cue or visual feedback here.

##### Disable Touch Of Other Objects

- **Type**: `boolean`
- **Description**: If enabled, after any object is touched the system locks further input until `Unlock()` is called. Use this to enforce a sequential interaction flow — e.g., force the player to acknowledge one object before moving to the next. Call `Unlock()` from any UnityEvent to re-enable touching.

##### Show Gizmos

- **Type**: `boolean`
- **Description**: Draws a wireframe sphere in the Scene view with radius equal to `Touch Range`, centered on this GameObject.

#### Public Methods

##### Lock()

- Disables all further touch interactions. Called automatically if `Disable Touch Of Other Objects` is enabled.

##### Unlock()

- Re-enables touch interactions after they were locked. Wire this to any `On Touch Event` on a `Touchable Object` to allow the player to proceed to the next interaction.

---

### Touchable Object

> **Add Component → MRCH → Interact → Touchable Object**

> **Setup required**: This component must be on the same GameObject as a `Collider`, and that GameObject must be assigned to the **same layer** as the `Touchable Layer` on the scene's `Touchable Manager`. A warning is shown in the Inspector (and logged to the Console) if the object is still on the Default layer.

A component that marks a GameObject as touchable and defines what happens when it is touched.

#### Inspector Variables

##### On Touch Event

- **Type**: `UnityEvent`
- **Description**: Fires when this object is successfully touched or clicked by the player.

---

### AudioController

> **Add Component → MRCH → Audio → Audio Controller**

> **Requires**: An `AudioSource` component on the same GameObject.

A component for audio fade in and fade out.

#### Inspector Variables

##### Audio Source
- **Type**: AudioSource
- **Description**: The target audio source controlled by the script. It will fetch the AudioSource on the same GameObject if null.

##### Fade Duration

- **Type**: `float` (seconds)
- **Description**: The duration used for all fade in and fade out transitions.

##### Target Volume

- **Type**: `float`
- **Range**: 0 – 1
- **Description**: The target volume level that `FadeInAudioToTargetVolume()` fades up to.

#### Public Methods

##### FadeInAudioToTargetVolume()

- **Description**: Starts playing the AudioSource from silence and fades the volume up to `Target Volume` over `Fade Duration` seconds. Use this instead of calling `AudioSource.Play()` directly when you want a smooth fade-in.

##### FadeOutAudio()

- **Description**: Fades the AudioSource volume down to zero over `Fade Duration` seconds, then stops playback. Use this instead of calling `AudioSource.Stop()` directly when you want a smooth fade-out.

##### SetVolumeTo(float)

- **Parameter**: `float` — The target volume value (0–1).
- **Description**: Smoothly fades the AudioSource volume to the specified value over `Fade Duration` seconds. Note: this does **not** update the `Target Volume` inspector field — it only affects the live playback volume.

##### StopImmediate()

- **Description**: Immediately stops any active fade and the audio source.
---

### EventBroadcaster

> **Scene setup**: This component is already placed and wired in the template scene. You do not need to add or configure it manually.

`EventBroadcaster` is the bridge between the Immersal SDK lifecycle and the rest of your MRCH scripts. It listens to Immersal's internal events (SDK initialized, localization succeeded, etc.) and re-broadcasts them as standard C# events and UnityEvents that other components — like `Move & Rotate`'s "After Localized" modes — subscribe to automatically.

The key event for MR experiences is **`OnFirstLocalized`**, which fires once when the app successfully recognizes the physical location for the first time. Components that use "After Localized" modes wait for this signal before starting. If the scene is already localized when a component enables (e.g. after a scene reload), it catches up immediately via the `HasLocalized` flag.

For typical usage, refer to `QuickStart.md`. You only need to interact with this component directly if you are building a custom script that needs to respond to localization events.

## Toast Notification

> **Add Component → MRCH → Toast → Toast Manager**

> **Scene setup**: The template scene already includes a pre-configured Canvas with a `Toast Parent` and a default toast prefab assigned. You normally do not need to set this up from scratch.

A **Toast** is a small pop-up notification that slides into view, displays a message for a few seconds, then fades out — similar to Android toast notifications. The system consists of two parts:

- **Toast Manager** — a scene-level singleton that drives the animation and exposes `ShowToast()`.
- **Default Toast Instance** — the prefab component that holds the TMP text and optional icon. It is already wired up in the template scene.

### Toast Manager

> **Singleton**: Only one `Toast Manager` should exist in a scene. If a second one is instantiated, it will destroy itself.

#### Inspector Variables

##### Toast Prefab / Scene Object

- **Description**: The toast UI GameObject to display. Can be a prefab asset or an existing scene object (e.g. the `Canvas > ToastParent > Toast` object in the template). Must have a `Default Toast Instance` (or custom `IToastInstance`) component on its root.

##### Toast Parent

- **Description**: The `RectTransform` that the toast will be parented under. Should be inside a Screen Space — Overlay Canvas. In the template scene, this is pre-assigned to `Canvas > ToastParent`.

##### Rest Anchored Position

- **Type**: `Vector2`
- **Description**: The anchored position the toast rests at when fully visible. Adjust this to change where on-screen the toast appears (e.g. top-center, bottom-center).

##### Animation Offset

- **Type**: `float`
- **Description**: The Y distance the toast travels during enter and exit animation. The toast enters from below (`restPosition - offset`) and exits upward (`restPosition + offset`).

##### Display Duration

- **Type**: `float` (seconds)
- **Description**: How long the toast stays fully visible before beginning its exit animation.

##### Enter Duration / Exit Duration

- **Type**: `float` (seconds)
- **Description**: Duration of the slide-in and slide-out animations respectively.

##### Enter Ease / Exit Ease

- **Type**: `Ease` (DOTween)
- **Description**: The animation curve used for entering and exiting. Defaults are `OutCubic` (enter) and `InCubic` (exit). Experiment with others for different feels.

#### Events

These fire at each phase of the toast animation. Wire sounds, particles, or other effects here.

- `On Toast Show` — fires when `ShowToast()` is called, before the enter animation begins.
- `On Toast Fully Visible` — fires when the toast has finished entering and is at rest.
- `On Toast Hide Start` — fires when the exit animation begins.
- `On Toast Fully Hidden` — fires when the exit animation completes and the toast is hidden.

#### Public Methods

##### ShowToast(string message)

- **Parameter**: `string` — The message to display.
- **Description**: Displays the toast with the given message. If a toast is already visible, it is immediately replaced by the new one. Call this from any UnityEvent by dragging the `Toast Manager` GameObject and selecting `ShowToast(string)`.

##### KillCurrentToast()

- **Description**: Immediately hides and cancels any active toast. Useful for dismissing a toast early.

---

### Default Toast Instance

> **Add Component → MRCH → Toast → Default Toast Instance**

> This component is already on the toast prefab in the template scene. You do not need to add it manually unless you are building a custom toast from scratch.

The `Default Toast Instance` is the visual component of the toast. It holds references to the TMP text and optional icon, and implements the `IToastInstance` interface that `Toast Manager` uses to control the display.

#### Inspector Variables

##### Message Text

- **Type**: `TextMeshProUGUI` (Required)
- **Description**: The TMP UI component that displays the toast message string.

##### Icon Image

- **Type**: `Image` (optional)
- **Description**: An optional icon displayed alongside the message. If the icon image has a sprite assigned in the Inspector, it will be shown regardless of whether an icon is passed to `ShowToast`. Leave this empty if your toast design has no icon.

#### Events

- `On Content Set` — fires when `ShowToast()` sets new content on this instance, passing the message string as a parameter. Useful for driving additional visuals tied to specific messages.
- `On Content Reset` — fires when the toast content is cleared before being reused.

---

## Tasks

### Multi Condition Event Manager

> **Add Component → MRCH → Tasks → Multi Condition Event Manager**

A task tracking system that lets you define a list of named tasks and fire events as they are completed — all from the Inspector without any code. Useful for building interactive experiences where the player must visit several locations, trigger a set of interactions, or complete a sequence of steps.

> **Setup tip**: Click the **Validate Tasks** button in the Inspector before testing. It checks for empty IDs, duplicates, and broken prerequisite references, and logs any issues to the Console.

#### Settings

##### Initialize On

- **Type**: `enum` — When the manager sets up its task list:
  - `On Enable` — Initializes (and resets all task states) whenever the GameObject is enabled or re-enabled.
  - `Start` — Initializes once on the first `Start()` call. Use this if you want the task state to survive the object being briefly disabled and re-enabled.

##### Debug Mode

- **Type**: `boolean` — If enabled, logs all task completions, resets, and any blocked or re-triggered events to the Console. Also shows a live **Runtime Status** section in the Inspector during Play Mode with each task's current state.

#### Tasks List

Each entry in the **Tasks** list defines one condition to track.

##### Task Entry Fields

- **ID** (string, required): A unique name for this task. This is the string you pass to `CompleteTask()`. Shown as the title of each task entry in the Inspector.
- **Allow Retrigger** (boolean): If enabled, calling `CompleteTask()` on an already-completed task fires its `On Task Completed` event again (useful for looping or repeated interactions). The progress count is not affected.
- **Description** (string, Editor Only): A free-text note visible only in the Inspector. Useful for documenting what action the player needs to take for this task. Stripped from builds.
- **Prerequisite Task ID** (string, optional): If set, this task will be blocked until the task with the given ID is completed first. Enforces ordering without any code.
- **On Task Completed** (UnityEvent): Fires when this specific task is completed. Wire any per-task effects here (e.g. reveal an object, play audio).

#### Events

- `On Initialized` — Fires once when the manager finishes setting up. Wire any "start of experience" logic here.
- `On All Tasks Completed` — Fires when every task in the list has been completed. Wire your finale or transition here.
- `On Progress Changed (completed, total)` — Fires every time a new task is completed, passing the current count and total. Wire this to a `Multi Condition Progress UI Helper` or any method that accepts `(int, int)`.
- `On Reset` — Fires when `ResetAll()` is called.

#### Public Methods

##### CompleteTask(string taskId)

- **Parameter**: `string` — The ID of the task to complete.
- **Description**: Marks the task as complete, fires its `On Task Completed` event and `On Progress Changed`. Blocked if the task's prerequisite is not yet complete, or if the task is already done and `Allow Retrigger` is off. Wire this to any trigger (e.g. `On Trigger First Enter` on an `Interaction Trigger`).

##### ResetTask(string taskId)

- **Parameter**: `string` — The ID of the task to reset.
- **Description**: Marks a single task as incomplete and decrements the progress count.

##### ResetAll()

- **Description**: Resets all tasks to incomplete and fires `On Reset`. The `On Progress Changed` event also fires with `(0, total)`.

---

### Multi Condition Progress UI Helper

> **Add Component → MRCH → Tasks → Multi Condition Progress UI Helper**

> **Requires**: A `TMP_Text` component to write into, and a reference to a `Multi Condition Event Manager` in the scene.

A simple display component that listens to a `Multi Condition Event Manager` and updates a TMP text with the current progress.

#### Inspector Variables

##### Condition Event Manager

- **Type**: `MultiConditionEventManager` (Required)
- **Description**: The manager to track. Drag the GameObject that has your `Multi Condition Event Manager` component here.

##### Progress Text

- **Type**: `TMP_Text` (Required)
- **Description**: The TMP text component to update. Can be on a different GameObject.

##### Progress Format

- **Type**: `string`
- **Description**: The display template. Use `{0}` for the current completed count and `{1}` for the total. Default: `Progress: {0} / {1}`. Examples: `{0} of {1} found`, `Step {0}/{1}`.

---

## Editor Tools

These components are for use in the Unity Editor only. They have no effect in a build and are intended to help you set up, navigate, and annotate your scene during development.

### XR Origin Editor Control

> **Add Component → MRCH → Edit → XR Origin Editor Control**

> **Template scene**: This component is already included on the `XR Origin` in the template. You do not need to add it manually.

> **Editor only**: All movement and UI code is stripped from builds (`#if UNITY_EDITOR`). No runtime performance cost.

Allows you to move and rotate the XR Origin (i.e., the player camera) in the Editor Play Mode using keyboard and mouse, so you can test your scene without a headset.

| Control            | Action                             |
| ------------------ | ---------------------------------- |
| W / A / S / D      | Move forward / left / back / right |
| Q / E              | Move down / up                     |
| Shift              | Hold for fast move                 |
| I / K              | Pitch camera up / down             |
| J / L              | Yaw camera left / right            |
| Right-click + drag | Free-look (mouse)                  |

A collapsible control hint overlay is shown in the bottom-right corner during Play Mode. Click **x** to collapse it to a header bar, **|||** to expand it again.

#### Inspector Variables

- **Move Speed** / **Fast Move Speed** (meters): Normal and Shift-held movement speeds.
- **Elevation Speed** (meters): Speed for Q/E vertical movement.
- **Rotation Speed** (degrees): How fast the camera rotates per input unit.
- **Show Control Hint**: Toggle the in-Play-Mode hint panel on or off.

---

### Editor Note

> **Add Component → MRCH → Edit → Editor Note**

> **Editor only**: This component and all its data exist only in the Editor. It is stripped from builds.

A scratch-pad component for annotating GameObjects in the Hierarchy. Add any number of note entries — each can reference another object and include a free-text description. Useful for leaving setup instructions, TODOs, or reminders for yourself or your team.

No fields affect runtime behavior.

---

### MR Area Label

> **Add Component → MRCH → Edit → MR Area Label**

> **Editor only**: Draws only in the Scene view. No runtime impact.

Draws a labeled wireframe box in the Scene view to mark a named area of your MR experience (e.g. "Entrance Zone", "Exhibit A"). Useful for planning spatial layouts and communicating zones to teammates.

#### Inspector Variables

- **Label Text**: The text shown above the box in the Scene view.
- **Area Size** (Vector3): The dimensions of the wireframe box.
- **Use Transform**: If enabled, the box is positioned and oriented by this GameObject's transform. If disabled, you can set a manual `Center` and `Rotation`.
- **Gizmo Color**: Color of the wireframe and label.
- **Text Offset Y**: Vertical gap between the top of the box and the label.
- **Culling Distance**: The Scene view camera distance beyond which the gizmo is hidden, to avoid clutter when zoomed out. Adjust with the **Set Culling Distance** button.

---

### Map Model

> **Add Component → MRCH → Edit → Map Model**

> **Template scene**: This component is attached to the color/visual mesh of your scanned environment in the map template.

Marks a GameObject (and all its children) as an **Editor-only map reference model**. In the Editor, the object is hidden from the Hierarchy (`HideInHierarchy`) and excluded from builds (`DontSaveInBuild`). In a build, the object deactivates itself on `Awake`.

**You do not need to manually delete or disable the environment mesh before building.** As long as `Map Model` is attached to the visual mesh, it will be automatically excluded. Just make sure this component is on the correct root object — the one containing your environment color model — and not on the actual collision/occlusion geometry that should remain in the build.

---

# Environmental Occlusion Culling

### Description

To bring better AR experience, or want your content to have a better connection to reality (Wow that’s Mixed Reality!). The environmental occlusion is essential element. In the template, Prof. Zhang wrote a out-of-box shader to use for the environmental buildings to mask the virtual content. You can find it at `Assets/Plugins/Occlusion/Occlusion_Mat.mat`, or in your MapTemplate, the invisible cubes under `XR Space>Occlusion Culling` all have been assigned with this material.

### How to enable the environmental occlusion culling

1. Make sure the shapes that mimics the surrounding buildings has been assigned with the material mentioned above.
2. The visible and culling-able objects are not using the default materials, because they are not adjustable.
3. **Go to the materials (component), then navigate to ‘Advanced Options’, adjust the ‘Sorting Priority’ higher**.
4. All done and you can test now!

# Optional: Extending and Inheriting Scripts

Most scripts in the common folders are `abstract` (inheritable) and most of their methods are `virtual` (overridable). This means you can create your own subclass that changes specific behavior without touching the originals.

The **Toast system** is a good example of where this is intended:

- **Extend `DefaultToastInstance`** — if you want to keep the basic slide-in/out animation but change how content is displayed (e.g. add a subtitle line, change colors per message type), inherit from `DefaultToastInstance` and override `SetContent()`.
- **Implement `IToastInstance` directly** — if you want a completely custom toast visual from scratch, create a new `MonoBehaviour` that implements the `IToastInstance` interface, attach it to your own prefab, and assign that prefab to `Toast Manager`. The interface requires: `RectTransform`, `CanvasGroup`, `GameObj`, `Initialize()`, `SetContent()`, and `ResetContent()`.
- **Extend `ToastManager`** — if you need to change the animation itself (e.g. scale instead of slide), inherit from `ToastManager` and override `CreateAnimationSequence()`.

For all other components (triggers, audio, text, etc.), the same pattern applies — inherit from the abstract base and override the `virtual` methods you need.

Here are some learning resources on how inheritance and overriding work in Unity C#:

* [C# Overriding in Unity! — Intermediate Scripting Tutorial](https://youtu.be/h0J4gs4DW5A?si=jgqt5dqfbGeZA4xB)
* [override (C# reference)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/override)
* [Protected, Virtual, Abstract methods — What are they and when to use them?](https://www.reddit.com/r/Unity3D/comments/5rmj0v/protected_virtual_abstract_methods_what_are_they/)

If you get stuck and need to modify a common script, feel free to ask Shengyang for help!
