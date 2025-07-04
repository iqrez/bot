# Agents

The **HeadlessControllerEmulator** solution is organized around modular components that work together to capture input, translate it and feed a virtual controller. Each autonomous piece is described below.

## RawInputHandler
- **Purpose**: Captures global keyboard and mouse events using the Windows Raw Input API.
- **Inputs**: WM_INPUT messages from the OS.
- **Outputs**: Raises events like `KeyDown`, `KeyUp`, `MouseMove`, `MouseButtonDown`, `MouseButtonUp` and `MouseWheel` that deliver `RawKeyEventArgs` or `RawMouse*EventArgs` to subscribers.
- **Interactions**: Registered by application forms to receive input. Other agents (e.g., `MappingEngine`, `MacroEngine`) subscribe to its events.
- **Configuration**: Call `RegisterDevices(IntPtr hwnd)` with a window handle to start receiving input.

## WootingAnalogHandler
- **Purpose**: Polls Wooting keyboard analog values via the official Analog SDK.
- **Inputs**: Wooting SDK functions.
- **Outputs**: Periodic events `AnalogValueUpdated`, `KeyPressed` and `KeyReleased` when analog thresholds are crossed.
- **Interactions**: Can feed analog values into `MappingEngine` or other logic.
- **Configuration**: Instantiate to start a background polling thread; adjust `PressThreshold` and `ReleaseThreshold` as needed.

## MouseToStickMapper
- **Purpose**: Converts raw mouse deltas into normalized thumb‑stick values.
- **Inputs**: X/Y mouse movement values.
- **Outputs**: `(short X, short Y)` stick coordinates in the range ‑32767..32767.
- **Interactions**: Used by `MappingEngine` to translate mouse motion before applying profile actions.
- **Configuration**: Exposes tuning properties for sensitivity, smoothing, acceleration and curve shape.

## MappingEngine
- **Purpose**: Central engine that applies a `Profile` to incoming `InputEvent` instances and produces controller state updates.
- **Inputs**: `InputEvent` objects created from keyboard, mouse or analog data.
- **Outputs**: Updates `IVirtualController` implementations with button, axis, trigger and D‑pad states.
- **Interactions**: Consumes events from `RawInputHandler` and `WootingAnalogHandler`; relies on `MouseToStickMapper` for mouse translation and `ProfileManager` for current mappings.
- **Configuration**: Call `ApplyProfile(Profile)` and periodically `UpdateControllerState(IVirtualController)`.

## VirtualControllerManager
- **Purpose**: Wrapper around ViGEmBus that creates and manages a virtual Xbox 360 or DualShock 4 controller.
- **Inputs**: Requests to set buttons, axes, triggers and D‑pad directions.
- **Outputs**: Sends prepared reports to the virtual device; optionally receives feedback events.
- **Interactions**: Acts as the concrete `IVirtualController` used by `MappingEngine` or other mapping code.
- **Configuration**: Instantiate with a controller type and call `ChangeControllerType` when needed.

## Profile and ProfileManager
- **Purpose**: `Profile` describes mappings from input sources to controller actions. `ProfileManager` loads/saves profiles from disk and tracks the active profile.
- **Inputs**: JSON profile files and management requests (add, clone, delete, set current).
- **Outputs**: Provides `Profile` objects to other agents; raises `ProfileChanged` events when switching profiles.
- **Interactions**: `MappingEngine` queries mappings from the current profile; UI and command‑line tools manipulate profiles through the manager.

## MacroEngine
- **Purpose**: Executes scripted button sequences (macros) triggered by input events.
- **Inputs**: Macro definitions and trigger notifications (`HandleEvent`).
- **Outputs**: Calls `IButtonSink.SetButtonState` during execution and raises `MacroStarted`, `MacroCompleted` and `MacroError` events.
- **Interactions**: Often used alongside `MappingEngine` to produce complex button behaviours.
- **Configuration**: Add macros via `AddMacro`, bind keys with `BindTrigger` and run via `TriggerMacro` or automatic triggers.

## ConfigManager and SettingsManager
- **Purpose**: Lightweight helpers for persisting JSON configuration data and application‑wide settings.
- **Inputs/Outputs**: Read and write JSON files. `SettingsManager` exposes a `Settings` object containing options such as current profile or per‑process mappings.
- **Interactions**: Used by the UI and headless application to remember configuration between runs.

## Component Interaction Overview
1. `RawInputHandler` captures keyboard/mouse input and raises events.
2. `WootingAnalogHandler` optionally supplies analog key values.
3. Event data is wrapped as `InputEvent` objects and processed by `MappingEngine`.
4. `MappingEngine` consults the active `Profile` from `ProfileManager` and may invoke `MacroEngine` triggers.
5. Translated controller states are written to a `VirtualControllerManager` instance, which forwards them to ViGEmBus.

## Developer Notes
- Build the solution with **.NET 8.0**; run either the Windows Forms UI or `HeadlessControllerEmulator` console app as needed.
- Ensure the **ViGEm bus driver** is installed for virtual controller support. The optional **Wooting analog SDK** is required for analog keyboard features.
- Profiles and settings are stored under the user’s AppData directory. Editing JSON files directly is supported, but the UI provides forms for basic management.
- When contributing, keep components loosely coupled and favor event‑based communication so agents remain easy to test independently.
