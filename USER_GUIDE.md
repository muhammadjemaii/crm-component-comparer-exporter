# CRM Component Comparer Exporter — User Guide

## Overview

CRM Component Comparer Exporter is a .NET/C# utility that helps compare and export CRM component data. This repository includes the application code and tools to build, run, and export comparison results (including PDF export functionality).

> Note: This document provides general instructions for building and using the project. For repository-specific details (project names and paths) check the solution (.sln) and project (.csproj) files in the repository root.

## Prerequisites

- .NET SDK 6.0 or later installed (or the specific SDK version used by the project). Download from https://dotnet.microsoft.com/.
- Git (for cloning the repository).
- Optional: Visual Studio 2022 / Visual Studio Code for IDE support.

## Getting the code

1. Clone the repository:

```bash
git clone https://github.com/muhammadjemaii/crm-component-comparer-exporter.git
cd crm-component-comparer-exporter
```

2. Inspect the solution and project files to find the main executable project (look for a .sln or a project with an executable OutputType in its .csproj).

## Build

From the repository root or the directory containing the main project/solution, run:

```bash
# Build the solution
dotnet build

# Or build and restore packages in one step
dotnet restore && dotnet build
```

If you prefer an IDE, open the .sln in Visual Studio or the folder in Visual Studio Code and build from there.

## Run

If the project produces a runnable console or desktop application, run it using dotnet run (from the project's directory) or execute the produced binary from the output folder:

```bash
# From the main project directory
dotnet run --project ./src/YourMainProject/YourMainProject.csproj

# Or run the built executable (example path)
./bin/Debug/net6.0/YourMainProject
```

If the repository contains multiple projects, identify and run the project that acts as the entry point (usually a project with a Program.cs or Startup class).

## Usage

The application compares CRM components and can export reports. Typical usage patterns:

- GUI: Launch the application and use the interface to select source and target environments, run a comparison, then export results via the Export menu.
- CLI: If a command-line interface is provided, run with `--help` to see available commands and options:

```bash
dotnet run --project ./src/YourMainProject -- --help
```

Look for options such as `--source`, `--target`, `--output`, and `--format`.

## Exporting to PDF

This repository includes an Export PDF feature. How to use it:

- From the GUI: After running a comparison, choose the Export or Save option and select PDF as the output format. Configure layout options (title, include differences, include metadata) as available.
- From the CLI: If the application supports CLI exporting, use an argument such as `--export-pdf` or `--format pdf` (run `--help` to confirm exact flags). Example:

```bash
# Example CLI export (replace flags with actual ones from --help)
dotnet run --project ./src/YourMainProject -- --source envA --target envB --format pdf --output comparison-report.pdf
```

If there are additional PDF options (watermark, page size, include screenshots), consult the app's `--help` or documentation within the codebase for exact parameter names.

## Configuration

If the project uses an appsettings.json, .env, or another configuration file, set environment-specific values there. Typical configuration items:

- CRM connection strings / authentication
- Output folder for exported reports
- Feature flags for which sections to include in the export

Example (appsettings.json snippet):

```json
{
  "Crm": {
    "SourceConnection": "...",
    "TargetConnection": "..."
  },
  "Export": {
    "OutputFolder": "./exports",
    "DefaultFormat": "pdf"
  }
}
```

## Troubleshooting

- Build errors:
  - Ensure the correct .NET SDK is installed and available on PATH.
  - Run `dotnet restore` to fetch missing NuGet packages.

- Runtime errors:
  - Check configuration for missing or invalid connection strings.
  - Inspect logs (if provided) in the output or logs directory.

- PDF export problems:
  - Verify any native dependencies or third-party libraries used for PDF generation are installed and supported on your OS.
  - If the PDF is blank or incomplete, try exporting to another format (e.g., HTML) to determine if the issue is with report generation or PDF rendering.

## Tests

If the repository includes test projects, run them with:

```bash
dotnet test
```

Review the test projects to learn expected behavior and example usage scenarios.

## Contributing

- Fork the repo and create a feature branch for changes.
- Follow the repository's coding conventions and include unit tests for new functionality.
- Create a pull request describing your changes and the rationale.

## License

Check the repository root for a LICENSE file and follow its terms.

## Where to look next in this repo

- README.md (if present) — general project overview and quickstart
- The solution (.sln) and main project (.csproj) — to find the entry point
- Docs/ or /docs directory (if present) — extended documentation

---

If you'd like, I can:
- Tailor this user guide to the exact project structure (I can inspect the repository and update the guide with exact project names and commands), or
- Add screenshots or example exported PDF output and include them in the docs.
