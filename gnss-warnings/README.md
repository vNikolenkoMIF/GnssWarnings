# GNSS Warnings

A Unity/C# component for monitoring Global Navigation Satellite System (GNSS) conditions, tracking positioning-related issues, and presenting warnings in a mobile user interface.

> **Implementation status:** This repository contains the monitoring framework, configurable warning UI, and integration interfaces. The GNSS condition checks currently contain placeholder implementations (many return `true`), and the corrective actions are stubs. **It is not ready to provide reliable GNSS diagnostics without completing those implementations.**

## Features

- **Configurable issue monitoring:** Select monitors for each `GnssWarningsRegistry` configuration through the Unity Inspector.
- **Sampling-based detection:** Monitors accumulate failed samples over a measurement window and report an issue when failures exceed successful samples.
- **Priority layers:** Monitors can be grouped by `PriorityLayer`; an active issue can stop evaluation of subsequent layers via `InterruptNextLayers`.
- **Issue state tracking:** Inspect `LastIssuesStatuses`, query individual issues, and subscribe to `IssuesChanged`.
- **Pluggable notifications:** Register implementations of `IGnssIssuesNotifier` to react to detected and resolved issues.
- **Mobile warning UI:** Display messages for active issues, dismiss them temporarily, report affected indicators, and reposition the warning viewport with UI anchors.
- **Extensible system impacts:** Map detected issues to impact strategies, with shared tracking so an impact can be restored when its contributing issues have cleared.

## Supported issue types

The `GnssIssue` enum and monitor factory define the following issue categories:

| Issue | Intended purpose |
| --- | --- |
| `GnssDisconnected` | Detect a lost GNSS connection |
| `RtkFixBecameFloat` | Detect RTK quality falling from fixed to float |
| `RtkInvalid` | Detect invalid RTK quality |
| `FluctuatingAccuracy` | Detect unexpected changes in positioning accuracy |
| `InsufficientAccuracy` | Detect positioning accuracy outside an acceptable threshold |
| `DirectionInsufficientAccuracy` | Detect insufficient accuracy for direction calculations |
| `DataCollectionInsufficientAccuracy` | Detect accuracy insufficient for data collection |

These are **defined monitoring categories**, not fully implemented detection algorithms in the current codebase.

## Architecture

The component is split into shared monitoring logic and mobile presentation.

```text
gnss-warnings/
├── Shared/
│   ├── Prefabs/
│   │   └── MonitoringSystem.prefab
│   └── Scripts/
│       ├── Contracts/             # Issue types, registry settings, message and impact mappings
│       ├── Factories/             # Monitor and impact construction
│       ├── Impacts/               # Impact lifecycle and strategy implementations
│       ├── Messages/              # Notification contracts and message comparison
│       ├── Monitors/              # Base and issue-specific monitors
│       ├── GnssIssueMonitoringSystem.cs
│       ├── IssueMonitoringWorker.cs
│       └── MonitoringEventsManager.cs
├── Mobile/
│   ├── Prefabs/
│   │   ├── GnssMonitoringPrefab.prefab
│   │   ├── WarningMessage.prefab
│   │   └── WarningMessagesScreen.prefab
│   ├── Scripts/
│   │   ├── Messages/             # Warning manager, message view and viewport contracts
│   │   └── UI/                   # Viewport positioning helpers
│   └── Textures/
└── README.md
```

### Monitoring flow

1. `GnssIssueMonitoringSystem.Initialize(IGnssManager)` prepares factories, the worker, and the configured impact mappings.
2. `SwitchMonitoringRegistry(...)` selects the appropriate `WarningsTypeRegistry` and creates its monitors.
3. `StartMonitoring()` starts the asynchronous worker. It invokes `BeforeRangeStart`, `AfterStep`, and `AfterRangeEnd` on registered monitors.
4. Each `GnssIssueMonitor` aggregates sample results into an `IssueDetected` state.
5. The monitoring system compares current and previous issue states, applies or restores configured impacts, notifies registered observers, and raises `IssuesChanged`.
6. On mobile, `GnssWarningsManager` creates/removes warning messages and updates affected indicators.

The source currently configures the worker with a 5-second range and frequency value of 5. The implementation adjusts the sampling frequency to an odd number and uses its own delay/step loop; avoid assuming strict five-second notifications without testing the timing.

## Integration

This repository contains **Unity assets and C# source files**, not a standalone Unity project or a ready-to-install UPM package.

### Prerequisites

- A Unity project compatible with the scripts and prefabs.
- **UniTask** (`Cysharp.Threading.Tasks`).
- The host application's GNSS abstractions (notably `IGnssManager`).
- The referenced vGIS/Meemim application assemblies for logging, utilities, UI, and configuration types.

These application-specific dependencies are not included in this repository. The Unity editor version and complete dependency versions are not specified here.

### Setup

1. Copy the `gnss-warnings` assets into the Unity project, preserving the `.meta` files and adjusting asset paths/namespaces only if necessary.
2. Make sure the required UniTask and host-application assemblies are available.
3. Add the shared `MonitoringSystem` prefab, or attach `GnssIssueMonitoringSystem` to a suitable GameObject.
4. Configure `Registered Monitors` in the Inspector:
   - Select a `GnssWarningsRegistry` value.
   - Add the desired `GnssIssue` entries.
   - Set their `PriorityLayer`, `InterruptNextLayers`, and `RunningLoopRange`.
5. Configure `Issues Impacts Mapping` for the issues being monitored. Available mapping types are `NoImpact`, `DirectionCorrector`, and `AllCorrectors`. The latter two strategies currently have no operational correction logic.
6. For mobile UI, add the warning prefabs, configure the `GnssWarningsManager` message catalog and serialized UI references, then register it as a notifier.
7. Supply the host application's `IGnssManager` instance and select a monitoring registry before starting monitoring.

### Minimal API example

The following illustrates the public API from an integration component. It assumes the monitor system and GNSS manager have already been supplied by the host application, and the selected registry is configured in the Inspector.

```csharp
using Assets._Project.Components.GnssMonitoring.Shared.Scripts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;
using Assets._vGISComponents.Core.Shared.Scripts.WebAPI.vGISCompaniesAPI.ViewModels.ApplicationOptions;

public class GnssMonitoringIntegration
{
    private readonly GnssIssueMonitoringSystem _monitoring;

    public GnssMonitoringIntegration(
        GnssIssueMonitoringSystem monitoring,
        IGnssManager gnssManager,
        GnssWarningsRegistry registry)
    {
        _monitoring = monitoring;
        _monitoring.Initialize(gnssManager);
        _monitoring.IssuesChanged += HandleIssuesChanged;
        _monitoring.SwitchMonitoringRegistry(registry, continueMonitoring: false);
    }

    public void Start() => _monitoring.StartMonitoring();

    public void Stop() => _monitoring.StopMonitoring();

    public void Dispose()
    {
        _monitoring.StopMonitoring();
        _monitoring.IssuesChanged -= HandleIssuesChanged;
    }

    private void HandleIssuesChanged()
    {
        foreach (var issue in _monitoring.MonitoringIssues)
        {
            bool active = _monitoring.IsIssueActive(issue);
            // Update application-specific state.
        }
    }
}
```

To connect an external UI or notification channel, implement `IGnssIssuesNotifier` and call `RegisterNotifier(...)` after initialization. Use `Unregister(...)` when the notifier is no longer needed.

## Mobile warning behavior

The mobile implementation uses `GnssWarningsManager` to map issue types to `MessageInfo` configurations and instantiate `WarningMessage` prefabs. A user can dismiss a warning temporarily; the manager attempts to show dismissed messages again while their issues remain active. The configured restore interval is 30 seconds.

`WarningMessagesAnchor` allows screens to reposition or lock the warnings viewport using `Place`, `Offset`, and `IsLocked`.

## Extending the component

**Add a new GNSS condition:**

1. Add a value to `GnssIssue`.
2. Implement a monitor derived from `GnssIssueMonitor`, including its real `IsIssueDetectedNow()` logic.
3. Add the new issue-to-monitor mapping in `GnssMonitorFactory.Create(...)`.
4. Configure the monitor in the relevant Inspector registry.
5. Add a `MessageInfo` entry and an impact mapping where required.

**Add a corrective action:** Implement `IGnssIssueImpact` or derive from `GnssIssueImpact`, add the strategy to the factory and associated configuration, and implement actual `MakeImpact()` and `StopImpact()` behavior. The shared base class tracks issues that require the same impact.

## Current limitations

- GNSS detector methods contain TODOs/placeholders and should be replaced with actual `IGnssManager`-based checks before use.
- Direction-corrector and all-corrector impact strategies have placeholder methods rather than operational integration.
- This repository does not contain a complete Unity project, documented automated tests, or all external dependencies.
- Monitor priority groups are stored in a dictionary; explicitly verify evaluation ordering if strict priority execution is required.
- The monitoring worker's timing and cancellation behavior, and the mobile dismissal timeout, should be tested before production integration.

## License

No license file is included in this repository. Contact the repository owner for permission and usage terms.
