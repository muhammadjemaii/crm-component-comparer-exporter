---
name: release-checklist
description: "Run the full release build, package the XrmToolBox plugin with ILMerge, verify outputs, and summarize release artifacts."
argument-hint: "Build configuration: Debug or Release (default: Release)"
agent: agent
---

Run the full release checklist for the CRM Component Comparer & Exporter plugin.

Build configuration: **$ARGUMENTS** (default to `Release` if not provided).

## Steps

### 1. Build the solution

Run the MSBuild build task:
```
msbuild RioCanada.Crm.ComponentExportComparer.sln /property:GenerateFullPaths=true /t:build /p:Configuration=$ARGUMENTS /consoleloggerparameters:NoSummary
```

- If there are **error CS** lines, stop and report them. Do not proceed.
- Warn about any **warning CS** lines but continue.

### 2. Package with ILMerge

Run the bundle script from the plugin directory:
```
cd RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin
BundlePackage.bat $ARGUMENTS
```

- If exit code is non-zero, report the error output and stop.

### 3. Verify output artifacts

Check that the following files exist and report their sizes:

| File | Expected location |
|---|---|
| Merged DLL | `RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin\bin\$ARGUMENTS\Merged\RioCanada.Crm.ComponentComparerExporter.dll` |
| Core DLL | `RioCanada.Crm.ComponentExportComparer.Core\bin\$ARGUMENTS\RioCanada.Crm.ComponentExportComparer.Core.dll` |
| Plugin DLL | `RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin\bin\$ARGUMENTS\RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin.dll` |

Report each file: path, size in KB, last modified timestamp.

### 4. Summarize

Print a release summary:
- Build configuration used
- Build result (succeeded / failed)
- ILMerge result (succeeded / failed)  
- List of artifacts with sizes
- Any warnings encountered

---

**Constraints:**
- Do not push to source control or deploy — this is a local packaging step only
- Do not modify any source files
- If `$ARGUMENTS` is empty, use `Release`
