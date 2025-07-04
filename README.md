# Input To Controller Mapper

This project provides a Windows application for converting mouse and keyboard input into virtual gamepad actions. It includes a core library for processing input events and a Windows Forms UI for configuring mappings.

## Prerequisites

- **.NET 8.0 SDK**
- **Windows 10/11** (required by the Windows Forms UI)
- **ViGEm bus driver** – needed for creating virtual controllers
- **Wooting analog SDK** – required if using analog features with supported keyboards

The NuGet packages used by the project are restored automatically. A key dependency is `Nefarius.ViGEm.Client`.

## Build

1. Clone the repository
2. Restore dependencies and build the solution file

```bash
dotnet build InputToControllerMapper.sln
```

## Run

Launch the application from the `InputToControllerMapper` project:

```bash
dotnet run --project InputToControllerMapper
```

### Headless controller emulator

For a minimal, non-UI version run:

```bash
dotnet run --project HeadlessControllerEmulator
```

## Tests

Unit tests live under the `Tests` folder and can be run with:

```bash
dotnet test
```

The test suite builds a small native stub using `gcc`, so make sure a C
compiler is available on your system.

