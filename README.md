# odc-diffjson

.NET external library for OutSystems Developer Cloud (ODC) that compares two JSON documents and
returns the differences between them, plus a helper to reshape JSON objects with dynamic property
names into something OutSystems can map to a static structure.

## Getting started

This asset is the source code for the OutSystems External Logic **DiffJSON**, which:

- Compares two JSON objects and returns a list of the attributes that differ (`Diff` / `DiffDeep`).
- Converts an object with dynamic/unknown property names into a list of `{key, value}` pairs
  (`JSON_Listify`), so it can be mapped to an OutSystems structure.

See [DOC.md](./DOC.md) for OutSystems-side usage (actions, parameters, structures, examples).

## Requirements

- .NET 10 SDK
- Targets `net10.0`

## Project structure

```
DoiTLean.DiffJSON/                 Class library published as the ODC external library
  IDiffJSON.cs                     Public interface - OutSystems actions (OSAction/OSInterface attributes)
  DiffJSON.cs                      Implementation of Diff, DiffDeep and JSON_Listify
  Structures/JSONPair.cs           OSStructure returned by Diff/DiffDeep
  resources/                      Icons embedded in the assembly and referenced by the OS attributes
  generate_upload_package.ps1     Publishes the library and zips it for upload to ODC
DoiTLean.DiffJSON.UnitTests/       NUnit test project
Dist/                              Output of generate_upload_package.ps1 (zip ready to upload)
```

## Building and testing

```bash
dotnet build DoiTLean.DiffJSON/DoiTLean.DiffJSON.sln
dotnet test DoiTLean.DiffJSON.UnitTests/DoiTLean.DiffJSON.UnitTests.csproj
```

## Packaging for OutSystems

`generate_upload_package.ps1` (run from `DoiTLean.DiffJSON/`) publishes the library for
`linux-x64` and zips the output into `Dist/ODC-DiffJSON.zip`, ready to upload as an external
library in ODC:

```powershell
./generate_upload_package.ps1
```

## Core concepts

### `Diff` vs `DiffDeep`

Both compare a `LeftJSON` (previous) and `RightJSON` (new) document and return a list of
`JSONPair` records. They differ in how they handle nested objects:

- **`Diff`** reports a changed nested object as a single entry: the whole parent attribute,
  serialized as the full previous/new object.
- **`DiffDeep`** recurses into nested objects and reports each changed leaf attribute
  individually, using a dot-separated path as `Attribute` (e.g. `Meta.City`).

Changed arrays are always reported as a single whole-attribute entry by both methods - the
array-diff format (element moves, deletions) is not walked. Use `Diff` when a nested change
should just read as "this object changed"; use `DiffDeep` when you need attribute-level
granularity inside nested objects.

### `JSONPair`

| Field             | Type    | Meaning                                                                 |
|-------------------|---------|--------------------------------------------------------------------------|
| `Attribute`       | Text    | Attribute name (dot-separated path for `DiffDeep`)                       |
| `PreviousValue`   | Text    | Value in `LeftJSON`; empty if the attribute didn't exist there           |
| `NewValue`        | Text    | Value in `RightJSON`; empty if the attribute doesn't exist there         |
| `HasPreviousValue`| Boolean | `false` when the attribute was added (didn't exist in `LeftJSON`)        |
| `HasNewValue`     | Boolean | `false` when the attribute was removed (doesn't exist in `RightJSON`)    |

`HasPreviousValue`/`HasNewValue` are optional fields kept for backward compatibility: existing
consumers built before these fields existed keep working unchanged, and `PreviousValue`/
`NewValue` alone still behave exactly as before.

### `JSON_Listify`

Given a JSON path (dot-separated, e.g. `"Meta.Custom"`), replaces the object found there with an
array of `{key, value}` pairs - one per original property. Useful for JSON payloads with
attribute names that aren't known upfront, which OutSystems structures can't represent directly.

## Known limitations

- Only JSON objects are supported as the root of `LeftJSON`/`RightJSON` for `Diff`/`DiffDeep`
  (a JSON array root is rejected with a clear error).
- Array contents are never diffed element-by-element; a changed array is always reported as a
  single whole-attribute change.

## OutSystems Links

- [ODC Documentation](https://success.outsystems.com/documentation/outsystems_developer_cloud/)
- [ODC External Logic](https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_external_logic/)
- [ODC External Libraries SDK](https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_external_logic/external_libraries_sdk_readme/)
