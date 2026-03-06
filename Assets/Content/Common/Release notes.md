# MRCH Template — Release Notes

---

## v2.0 alpha

**Date**: March 3, 2026  
**By**: Shengyang  


### Overview

v2.0 is a significant rework of the template's architecture, component set, and documentation. The goals were: cleaner Inspector organization, more consistent and beginner-friendly component naming, a richer set of ready-to-use tools, and a fully rewritten documentation that covers every component with accurate, up-to-date descriptions.

> **Migration note**: You are not suggested to do a auto upgrading from 1.x. During the rework, some history variable names was not recorded (via "FormallySerilizedAs"), and asmdef was renamed as well. So, you may lose your progress in the upgrade. 

---

### Architecture Changes

- **Add Component paths reorganized**: All components are now under the `MRCH/` menu hierarchy, grouped by category. The old `MRCH-Interact` path is gone. Example: `MRCH/Interact/Interaction Trigger`, `MRCH/Audio/Audio Controller`, `MRCH/Text/Simple TMP Typewriter`. See `Documentation.md` or `Component_QuickRef.md` for the full list.

---

### New Major Components

- **Lazy Follow** (`MRCH/Objects/Lazy Follow`): Smoothly follows a target (default: main camera) with configurable lag, dead-zone thresholds, and easing. Supports position follow, LookAt rotation, and rotation matching. Supersedes `Keep Facing To Cam` for most use cases.
- **Toast Manager** + **Default Toast Instance** (`MRCH/Toast/Toast Manager`): A slide-in/hold/slide-out notification system. Call `ShowToast("message")` from any UnityEvent. Pre-configured in the template scene.
- **Multi Condition Event Manager** (`MRCH/Tasks/Multi Condition Event Manager`): A named task tracker. Define tasks by ID, complete them from triggers, and fire events on each completion or when all are done. Supports prerequisites and optional retrigger.


---

## v1.35

**Date**: October 28, 2024  
**By**: Shengyang  
**Commit Hash**: *(not recorded)*

### Added

- **Gizmos visualization** for `Interaction Trigger`, `Touch Manager`, `Move And Rotate`, and `XR Origin Editor Control`. A `Show Gizmos` toggle has been added to each component. When enabled:
    - Distance Trigger and LookAt Trigger draw a wire sphere showing their range.
    - LookAt Trigger draws a ray from the camera showing its forward direction, to help estimate the required angle.
    - Move And Rotate draws a line from the object to its Move Target.
    - Touch Manager draws a wire sphere showing the touch range.

---

## v1.3

**Date**: October 26, 2024  
**By**: Shengyang  
**Commit Hash**: `368edf5997489b7806db809ec252f55e919e3a9a`

### Added

- **`EventBroadcaster`**: Bridges the Immersal SDK lifecycle to the rest of the template. Broadcasts `Initialized` and `Reset` events from `ImmersalSDK`, and `FirstLocalized` and `SuccessfulLocalized` events from the `Localizer` component. See `QuickStart.md` for wiring instructions, or copy the updated template scene directly.
- **`XR Origin Editor Control`**: Added to the template scenes. Allows WASD movement and IJKL/right-click rotation in Play Mode for testing without a headset. See `QuickStart.md` for setup instructions.
- **`Move & Rotate`**: Added `Move For Once After Localized` and `Forth And Back After Localized` options, allowing movement to start only after the first successful Immersal localization.

### Fixed

- `Move & Rotate` was incorrectly initializing on `FirstLocalized` instead of during the Awake phase in some configurations.

---

## v1.2

**Date**: October 22, 2024  
**By**: Shengyang and Ian  
**Commit Hash**: `ba1418ec7057c2b5b998b1c3624682eda7caf4ff` / `14516e0676a665463889ab223615b401d7c3b8a9`

### Added

- **`AudioController`**: New component for fading audio in and out over a configurable duration. Provides `FadeInAudioToTargetVolume()` and `FadeOutAudio()` as smoother alternatives to `AudioSource.Play()` / `Stop()`.

---

## v1.1

**Date**: October 19, 2024  
**By**: Shengyang  
**Commit Hash**: `c2b83b2797506efc76aa6f15e02e1dd88a24c20c`

### Added

- **`XR Origin Editor Control`**: Move with WASD and rotate with IJKL or right-click drag in Editor Play Mode.
- **`Simple TMP Typewriter`**: New `Start New Line When Overflow` setting. When enabled, the typewriter inserts a line break in advance if the next word would overflow the text box width. Recommended for English text.

### Changed

- **Namespace restructure**: Abstract base classes and wrapper scripts are now organized under separate namespaces for cleaner inheritance.
- **Input System migration**: Touch and click input now uses Unity's Input System exclusively (previously mixed with the legacy Input Manager). Touch interactions can now be tested in both the Game view and the Device Simulator.
- **`MapModel`**: Now excluded from builds via `DontSaveInBuild`, reducing package size. No need to manually remove the environment mesh before building.
- More variables exposed as `protected virtual` for easier inheritance.

### Fixed

- Various minor bug fixes.

### Known Issues (at time of release)

- Some initializations in `Move & Rotate` needed to be delayed until localization, particularly when returning to initial position. *(Addressed in v1.3.)*
- Some on-screen text displayed incorrectly in certain configurations.
- The `Map Model` GameObject may not be visible in the Hierarchy. Manually re-extracting textures can resolve this.

---

## v1.0

**Date**: October 17, 2024  
**By**: Shengyang  
**Commit Hash**: `4b93f45441cbe655227272d7aca8c54fde75807b`

### Initial Release

- First complete draft of the template with core interaction scripts: `Interaction Trigger`, `Move And Rotate`, `Simple TMP Typewriter`, `Shining Text`, `Keep Facing To Cam`, `Object Toolset`, `Image Fade`, `Touchable Manager`, `Touchable Object`.
- First draft of `Documentation.md` and `QuickStart.md`.