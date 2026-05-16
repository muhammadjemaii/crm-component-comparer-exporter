# CRM Component Comparer & Exporter — Project Guidelines

A .NET Framework 4.8 XrmToolBox plugin for comparing and exporting Dynamics 365 (v9.2 online) components.
See [README.md](../README.md) for feature overview and usage.

## Architecture

Two projects in the solution:

| Project | Purpose |
|---|---|
| `RioCanada.Crm.ComponentExportComparer.Core` | CRM metadata extraction, comparison, serialization, export logic |
| `RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin` | Windows Forms XrmToolBox plugin UI layer |

The plugin project depends on Core. Core has no UI dependencies.

**Core layout:**
- `Models/` — CRM component data models (`Solution.cs`, `WebResource.cs`, `SystemForm.cs`, etc.)
- `Models/Transform/` — XML serialization models for Ribbon, SiteMap, FormXml, Metadata
- `Extensions/` — Extension methods (`EntityExtension.cs`, `OrganizationServiceExtension.cs`, `MetadataExtension.cs`, etc.)
- `Utilities/` — Helpers (`SerializeUtility.cs`, `Helper.cs`)
- `Exporter.cs` / `ExportService.cs` / `DataTransformer.cs` — main orchestration and transformation

**Plugin layout:**
- `MyPlugin.cs` — MEF-exported plugin entry point (do not rename)
- `MyPluginControl.cs` — main UI control, inherits `PluginBase`
- `Comparision/` — UI controls for comparison result display
- Form classes follow `*Control.cs` / `*Form.cs` / `Form*.cs` pattern with `.Designer.cs` partners

## Build

```
msbuild /property:GenerateFullPaths=true /t:build /consoleloggerparameters:NoSummary
```

Or use the VS Code **build** task (`Ctrl+Shift+B`).

To package the XrmToolBox plugin after building, run:
```
RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin\BundlePackage.bat
```

There are no automated tests in this solution. Build success is the primary validation.

## Conventions

- **Namespaces** mirror folder paths: `RioCanada.Crm.ComponentExportComparer.Core.Models`, `.Extensions`, `.Utilities`
- **Partial classes**: all Windows Forms controls have a `.Designer.cs` counterpart — never hand-edit the designer file
- **CRM SDK version**: `Microsoft.CrmSdk.CoreAssemblies 9.0.2.60` — use `IOrganizationService` and `OrganizationServiceContext` idioms
- **Logging**: use `Logger.cs` in Core for any diagnostic output; avoid `Console.WriteLine` in library code
- **ILMerge** is used to produce a single-assembly distribution — avoid adding dependencies that conflict with XrmToolBox's own assembly set
- Target framework is **.NET 4.8** — do not use APIs unavailable on .NET Framework 4.8
