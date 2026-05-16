---
name: add-component-type
description: "Scaffold all files needed to support a new CRM component type for comparison and export."
argument-hint: "Component name and content type, e.g. 'Dashboard json' or 'Report binary' or 'BusinessRule xml'"
agent: agent
---

Scaffold support for a new CRM component type using this input: **$ARGUMENTS**

Parse the input as: `<ComponentName> <ContentType>` where ContentType is one of:
- `json` — component content is serialized as formatted JSON (like SystemForm `form.json`)
- `xml` — component content is formatted XML string (like Workflow `xaml.xml`, SystemForm `form.xml`)
- `binary` — component content is Base64-decoded bytes (like WebResource binary file)
- `text` — component content is a plain string (like Workflow `clientdata.txt`)
- `metadata-only` — no content file, only `metadata.json` (simplest case)

If ContentType is omitted, default to `metadata-only`.

Implement **all** of the following steps:

## 1. Model — `Core/Models/<ComponentName>.cs`

Create a class that:
- Is `internal` (not public), in namespace `RioCanada.Crm.ComponentExportComparer.Core.Models`
- Inherits from `Microsoft.Xrm.Sdk.Entity`
- Has `[EntityLogicalName(EntityLogicalName)]` attribute
- Defines `public const string EntityLogicalName = "<logical_name>";`
- Has two constructors: `()` and `(Guid id)`
- Exposes relevant CRM attribute properties using the `Get<T>()` / `Set()` extension pattern from `EntityExtension.cs`
- Has a static `FindByNames(OrganizationService service, IEnumerable<string> patterns, IEnumerable<Guid> solutionIds)` method using `QueryExpression` and `service.GetBigData<T>()`
- Uses `Utilities.Helper.ApplyPatternFilter` and `Utilities.Helper.ApplySolutionFilter` where applicable

## 2. Query Request — `Core/Models/ArgumentQueryRequest.cs`

Add:
- A `public List<string> <ComponentName>Patterns { get; set; } = new List<string>();` property
- A `case "<componentname>":` branch in the `Parse()` switch statement that populates the new list

## 3. Query Response — `Core/Models/ArgumentQueryResponse.cs`

Add:
- A `public List<<ComponentName>> <ComponentName>s { get; set; } = new List<<ComponentName>>();` property
- A block in `Build()` (or equivalent method) that calls `<ComponentName>.FindByNames(...)` when `argumentQuery.<ComponentName>Patterns.Count > 0` and adds results via `DistinctBy(x => x.Id)`

## 4. Exporter — `Core/Exporter.cs`

Add:
- A `private void Export<ComponentName>(List<<ComponentName>> items, int weight)` method:
  - Set `CurrentLabel`
  - Create an `IndexLineItem` folder entry
  - Batch-fetch full data using `this.Service.GetData<T>()`
  - Always call `HandleOutFile` for `metadata.json` using `SerializeUtility.SerializeJson(record.GetMetadataObject(IncludeAllProperty))`
  - Based on ContentType, also call `HandleOutFile` for the content file:
    - `json` → `SerializeUtility.FormatJson(record.<ContentProperty>)`, string output, `IndexItemType.FileJson`
    - `xml` → `SerializeUtility.FormatXml(record.<ContentProperty>)`, string output, `IndexItemType.FileXml`
    - `binary` → `Convert.FromBase64String(record.<ContentProperty>)`, byte[] output, `IndexItemType.File`
    - `text` → `record.<ContentProperty>`, string output, `IndexItemType.File`
    - `metadata-only` → no extra file
  - Set the correct `ContentType = IndexLineItemContentType.<ComponentName>` on the `IndexLineItem`
- A call to `this.Export<ComponentName>(ArgumentQueryResponse.<ComponentName>s, <Weight>)` in `Execute()`

## 5. Content Type Constant — `Core/Models/IndexLineItemContentType` (or wherever the enum/constants are defined)

Add `<ComponentName>` as a new value.

## 6. UI Routing — `XrmToolBoxPlugin/Comparision/ComparisionView.cs`

Add an `if` block routing the new `ContentType`:
- If content is `json` or `metadata-only`: reuse `FormIndexItemComparision` with an appropriate generator (or `null` for raw JSON diff)
- If content is `xml`, `binary`, or `text`: reuse `FormIndexItemComparision` with raw text diff, or create a dedicated `Form<ComponentName>Comparision.cs` form if custom display is needed
- Follow the existing `if (item.ContentType == IndexLineItemContentType.X)` pattern

## 7. README — `README.md`

Add `<ComponentName>` to the **Supported components** list with a note on what pattern field to use (schema name, display name, or unique name).

---

**Constraints:**
- Target framework is .NET 4.8 — no APIs unavailable in .NET Framework 4.8
- Use `Logger.cs` for diagnostic output — no `Console.WriteLine`
- Use `Microsoft.CrmSdk.CoreAssemblies 9.0.2.60` idioms (`IOrganizationService`, `QueryExpression`, `ColumnSet`)
- Do not edit `.Designer.cs` files
- After scaffolding, run the build task to confirm no compile errors: `msbuild /property:GenerateFullPaths=true /t:build /consoleloggerparameters:NoSummary`
