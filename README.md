# crm-component-comparer-exporter

CRM Component Comparer and Exporter tool will help developers, system administrators, and business analysts to compare two Dynamics CRM environments and export components to manage version history.

**Major features**

-	Powerful Query Editor - define multiple queries, select the whole solution, pick and choose the components for comparison 
-	List view showing the components that are different, drill down, filter what is modified and what is not
-	Log view to show the progress 
-	Visually see the difference using built-in file comparer tool
-	Export components to manage version history

This tool was tested on Microsoft Dynamics 365 9.2 online version.

## Getting Started

-	Select the source and target environments
-	Click on the ‘Add’ button to define the query 
- Alternatively select previously exported zip file in order to compare with previous version instead of target environment

![Home Screen](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/home-screen.png?raw=true)

-	Select the solution to compare. And click the ‘Ok’ button. All components within the solution will be selected 

![Query Editor 1](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/query-editor-1.png?raw=true)

-	Alternatively, you can also select specific components using the name, schema name and wild cards.

![Query Editor 2](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/query-editor-2.png?raw=true)

-	Click the ‘Options’ button on the main screen to select additional settings. Ribbons are not included by default when comparing the tables. Select this option If you want to compare the ribbons.

![Query Options](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/query-options.png?raw=true)

-	Tool supports multiple queries. 
-	To Edit/Remove select the query and click on Edit/Remove button.
-	Click on the ‘compare’ button. 

![Compare Screen](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/compare-screen.png?raw=true)

-	Compare Result will pop up to show the list of files. You can drill down to view the files. 

![Compare Result 1](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/compare-result-1.png?raw=true)

  -	**Unchanged** – No changes are found 
  -	**Modified** – The component is different 
  -	**Only in source** – Component is found the source but not in the target environment (if the comparison is on a solution, it means the component is not found in the solution)
  -	**Only in target** – Component is found in target but not in the source environment (if the comparison is on a solution, it means the component is not found in the solution)

  > Use filter option on bottom left to update the filter on the screen.

<br>

-	Double click on a row to drilldown or open file comparison tool.
-	If you close the compare result screen accidentally, open it again by clicking the ‘View Last Result’ button on the main screen.

![Compare Result 2](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/compare-result-2.png?raw=true)

-	The difference is shown in for a security role 

![File Comparision View 1](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/file-comparision-view-1.png?raw=true)

-	The difference is shown in for a web resource 

![File Comparision View 2](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/file-comparision-view-2.png?raw=true)

**Export components (Source environment only)**

-	Export CRM components to save in file system.
-	Export helps to manage version history for single environment.

![Export Screen](https://github.com/vinoddsouza/crm-component-comparer-exporter/blob/main/screenshots/export-screen.png?raw=true)

## Query

### Query pattern

Pattern should follow wildcard format. See https://support.microsoft.com/en-us/office/use-wildcards-in-queries-and-parameters-in-access-ec057a45-78b1-4d16-8c20-242cde582e0b

### Query format

```
[Component]=[pattern1,[pattern2,[pattern3,...]]];[AnotherComponent]=[pattern1,[pattern2,[pattern3,...]]]
```

### Query Example

```
Table=contact,account,sale*;WebResource=contact.js
```

> Hint: Use `*` to include all component

> Hint: `Solution=[pattern1,[pattern2,...]]` will apply additional filter on query

> Note: In query pattern enter unique name of component. In case component doesn't support unique name than use display name.

### Supported components

- Table - Pattern should contains schema name of table.
- WebResource - Pattern should content schema name of webresource.
- PluginStep - Pattern should content display name of step.
- Choice - Pattern should content schema name of Choice.
- Dashboard - Pattern should content display name of dashboard.
- SiteMap - Pattern should content display name of sitemap.
- SecurityRole - Pattern should content display name of security role.
- Workflow - Pattern should content display name of workflow.
- BusinessRule - Pattern should content display name of businessrule.
- Action - Pattern should content display name of action.
- BusinessProcessFlow - Pattern should content display name of business process flow.
- ModelDrivenApp - Pattern should content display name of model driven app.

## Technical analysis summary

### Solution structure

The solution is organized into two .NET Framework 4.8 projects:

- `RioCanada.Crm.ComponentExportComparer.Core`
  - Core export and transformation logic
  - Component query parsing and retrieval
  - Index generation (`index.json`) for comparison support
- `RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin`
  - WinForms/XrmToolBox user interface
  - Connection and settings management
  - Export and compare workflow orchestration

### Architecture overview

- The **Core** project encapsulates CRM service access, metadata/component export, progress reporting, and deterministic data transformation (XML/JSON normalization).
- The **Plugin** project focuses on UI interactions, persisted configuration, and execution control.
- This split provides good separation of concerns and helps keep business logic reusable.

### Key strengths

- Clear layering between UI and domain logic.
- Broad support for Dataverse/Dynamics components.
- Progress tracking and logging for long-running operations.
- ZIP + index-based flow enables repeatable offline comparisons.

### Risks / technical debt observed

- Legacy project format (non-SDK-style `.csproj` with `packages.config`).
- Large dependency/reference surface in .NET Framework projects.
- Some exception handling can be improved (e.g., prefer preserving original stack traces).
- Naming inconsistency (`Comparision`) impacts readability and discoverability.
- No automated test project detected in the current solution.

### Recommended next steps

1. Reliability pass
   - Standardize exception handling and error propagation.
2. Dependency hygiene
   - Audit and remove unused packages/references.
3. Add tests
   - Start with unit tests around transformation and export preparation logic.
4. Incremental modernization
   - Convert to SDK-style while staying on `net48` first, then evaluate migration to modern .NET.

---

## Build Fixes & Maintenance Log

### [Fix] Migrate `XrmToolBoxPlugin` from `packages.config` to `PackageReference`

**Date:** 2025  
**Branch:** `dev-commun`  
**Affected project:** `RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin`

#### Problem

The build was failing with the following error:

```
C:\...\.nuget\packages\microsoft.windows.sdk.contracts\10.0.28000.1839\build\Microsoft.Windows.SDK.Contracts.targets(4,5):
error : Must use PackageReference
```

Certain NuGet packages — notably `Microsoft.Web.WebView2` and `ILMerge` — ship MSBuild `.targets`/`.props` files that **require** the modern `PackageReference` format. Using the legacy `packages.config` format causes these packages to fail the build explicitly.

Additional warnings were also present:

| Warning | Location | Description |
|---------|----------|-------------|
| CS0168 | `ExportService.cs` (line 107) | Variable `ex` declared but never used |
| CS0168 | `FormFileViewer.cs` (line 431) | Variable `ex` declared but never used |
| CS0169 | `FormFileViewer.Designer.cs` (line 250) | Field `toolStripMenuItem1` never used |
| Assembly conflict | `app.config` | `Microsoft.IdentityModel.Clients.ActiveDirectory` version mismatch (5.2.9 → 5.3.0) |

#### Solution Applied

The `XrmToolBoxPlugin` project was migrated from `packages.config` to `PackageReference` format:

1. **Removed** the legacy `ILMerge.props` `<Import>` from the top of the `.csproj`.
2. **Replaced** all 31 `<Reference>` elements that had `<HintPath>` entries pointing to `..\packages\` with equivalent `<PackageReference>` elements (same package IDs and versions).
3. **Removed** the `<None Include="packages.config" />` entry from the `.csproj`.
4. **Removed** the old `Microsoft.Web.WebView2.targets` `<Import>` (now auto-injected by NuGet via `PackageReference`).
5. **Removed** the `EnsureNuGetPackageBuildImports` validation target (no longer needed).
6. **Deleted** the `packages.config` file from the project directory.

#### Packages migrated (31 total)

| Package | Version |
|---------|---------|
| DockPanelSuite | 3.0.6 |
| DockPanelSuite.ThemeVS2015 | 3.0.6 |
| ILMerge | 3.0.41 |
| jacobslusser.ScintillaNET | 3.6.3 |
| Menees.Common | 5.1.2 |
| Menees.Diffs | 5.1.2 |
| Menees.Diffs.Windows.Forms | 5.1.2 |
| Menees.Windows | 5.1.2 |
| Menees.Windows.Forms | 5.1.2 |
| Microsoft.CrmSdk.CoreAssemblies | 9.0.2.49 |
| Microsoft.CrmSdk.Deployment | 9.0.2.34 |
| Microsoft.CrmSdk.Workflow | 9.0.2.49 |
| Microsoft.CrmSdk.XrmTooling.CoreAssembly | 9.1.1.32 |
| Microsoft.CrmSdk.XrmTooling.WpfControls | 9.1.1.32 |
| Microsoft.CSharp | 4.7.0 |
| Microsoft.IdentityModel | 7.0.0 |
| Microsoft.IdentityModel.Clients.ActiveDirectory | 5.2.9 |
| Microsoft.Web.WebView2 | 1.0.1343.22 |
| Microsoft.Web.Xdt | 3.1.0 |
| MscrmTools.Xrm.Connection | 1.2023.6.56 |
| Newtonsoft.Json | 13.0.1 |
| System.IO.Compression.ZipFile | 4.3.0 |
| System.Net.Http | 4.3.4 |
| System.Private.Uri | 4.3.2 |
| System.Security.Cryptography.Algorithms | 4.3.1 |
| System.Security.Cryptography.Cng | 5.0.0 |
| System.Security.Cryptography.Encoding | 4.3.0 |
| System.Security.Cryptography.Pkcs | 5.0.1 |
| System.Security.Cryptography.Primitives | 4.3.0 |
| System.Security.Cryptography.X509Certificates | 4.3.2 |
| XrmToolBoxPackage | 1.2023.10.67 |

#### Result

Build succeeded for both projects (`Release Any CPU`).

> **Note:** The `Core` project still uses `packages.config`. It should be migrated in the same manner if it ever pulls in packages that require `PackageReference`.

### Scope of this analysis

This summary is based on repository structure and key implementation files in the current workspace.

<!--
### Additional settings

- IncludeSystemWebresource - By adding this boolean property in query will allow to add system webresource (Not recommended).
- IncludeAllProperty - By adding this boolean property in query will allow to export all property of component (Not recommended).

### Query in CLI vs XrmToolBox

In CLI we need to build query as per doc but in XrmToolBox we can take benifits of QueryEditor.

## Export components

1. Select directory
2. Check `Delete directory`, if you wish to delete existing directory before export.
3. Click on `Export`.


## Compore component

This functionality support only for XrmToolBox.

1. Select Target
2. Click on `Compare`

### Configure compare tool

1. Click on setting
2. Go to Compare tab
3. Select compare tool (Right now support DiffMerge only)
4. Enter DiffMerge executable file path
-->

### Credit
Icon - https://www.flaticon.com/free-icon/ab-testing_4661446
Menees.Diffs https://github.com/menees/Diff.Net
