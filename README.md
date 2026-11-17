# GNSS Warnings

**GNSS Warnings** is a modular Unity/C# component designed to monitor GNSS positioning quality, identify potential reliability issues, and communicate warnings to users through a mobile interface.

It provides a configurable monitoring framework for applications that depend on accurate GNSS and RTK positioning.

## Key Features

- **GNSS health monitoring** — monitors connection status, RTK quality, and positioning accuracy.
- **Configurable detection** — organizes issue monitors into configurable registries and priority layers.
- **Sample-based evaluation** — evaluates measurements over time rather than relying on a single reading.
- **Event-driven notifications** — reports changes in detected issues to registered listeners.
- **Mobile warning UI** — displays contextual warnings and supports temporary dismissal and flexible screen positioning.
- **Extensible architecture** — separates monitoring, notification, and corrective-action responsibilities.

## Monitored Conditions

The component defines monitors for:

- GNSS connection loss
- RTK FIX-to-FLOAT transitions
- Invalid RTK status
- Fluctuating positioning accuracy
- Insufficient positioning accuracy
- Insufficient direction accuracy
- Insufficient accuracy for data collection

## Project Structure

| Module | Responsibility |
| --- | --- |
| **Shared** | Monitoring lifecycle, issue detection framework, configuration, and impact strategies |
| **Mobile** | Warning messages, user notifications, and UI positioning |

## Technology

**Unity · C# · UniTask · GNSS / RTK**

The component is designed for integration into an existing Unity application and relies on application-specific GNSS interfaces and supporting assemblies.

> **Development status:** The monitoring architecture and warning UI are included. Individual GNSS checks and corrective actions still contain placeholder implementations and require completion before production use.
