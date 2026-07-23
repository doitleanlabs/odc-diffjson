# DiffJSON - OutSystems Usage Guide

This document describes how to use the **DiffJSON** external library from inside an OutSystems
Developer Cloud (ODC) app. It does not cover the .NET source code - see [README.md](./README.md)
for that.

## Installing

1. In ODC Studio, go to your app's dependencies and add the **DiffJSON** external library
   (uploaded from `Dist/ODC-DiffJSON.zip` by whoever manages this component in your tenant).
2. The library exposes one interface, **DiffJSON**, with three actions: `Diff`, `DiffDeep`,
   `JSON_Listify`.

## Structure: `JSONPair`

All diff actions return a `List Record` of `JSONPair`:

| Field              | Type    | Description                                                                                       |
|--------------------|---------|-----------------------------------------------------------------------------------------------------|
| `Attribute`        | Text    | Name of the attribute that changed. For `DiffDeep`, nested attributes use a dotted path, e.g. `Meta.City`. |
| `PreviousValue`     | Text    | The attribute's value in `LeftJSON`. Empty if the attribute didn't exist in `LeftJSON`.              |
| `NewValue`          | Text    | The attribute's value in `RightJSON`. Empty if the attribute doesn't exist in `RightJSON`.           |
| `HasPreviousValue`  | Boolean | `False` when the attribute was **added** (it didn't exist in `LeftJSON`). Optional field - if your flow was built before this field existed, it just won't be there; nothing to migrate. |
| `HasNewValue`       | Boolean | `False` when the attribute was **removed** (it doesn't exist in `RightJSON`). Same optionality as above. |

Use `HasPreviousValue`/`HasNewValue` whenever you need to tell "attribute added/removed" apart
from "attribute value is an empty string" - `PreviousValue`/`NewValue` alone cannot make that
distinction.

## Action: `Diff`

**Inputs:** `LeftJSON` (Text), `RightJSON` (Text)
**Output:** `List Record JSONPair`

Compares two JSON objects and returns one `JSONPair` per top-level attribute that differs. If a
nested object changed internally, it is reported as a **single entry**: the parent attribute,
with `PreviousValue`/`NewValue` holding the whole nested object serialized as JSON text.

Use this when you only care whether something inside a nested object changed, not exactly what.

### Example

```
LeftJSON:  { "Id": 165, "Name": "Old Name", "Meta": { "City": "Lisbon" } }
RightJSON: { "Id": 165, "Name": "New Name", "Meta": { "City": "Porto" } }
```

Result:

| Attribute | PreviousValue           | NewValue                | HasPreviousValue | HasNewValue |
|-----------|--------------------------|--------------------------|:---:|:---:|
| `Name`    | `Old Name`               | `New Name`               | True | True |
| `Meta`    | `{"City":"Lisbon"}`      | `{"City":"Porto"}`       | True | True |

## Action: `DiffDeep`

**Inputs:** `LeftJSON` (Text), `RightJSON` (Text)
**Output:** `List Record JSONPair`

Same as `Diff`, but recurses into nested objects and reports each changed **leaf** attribute
individually, using a dot-separated path as `Attribute`.

Use this when you need to know exactly which nested field changed, not just that "something in
this object changed".

Changed arrays are still reported as a single whole-attribute entry (same as `Diff`) - array
contents are never diffed element-by-element.

### Example

Same input as above:

| Attribute    | PreviousValue | NewValue | HasPreviousValue | HasNewValue |
|--------------|---------------|----------|:---:|:---:|
| `Name`       | `Old Name`    | `New Name` | True | True |
| `Meta.City`  | `Lisbon`      | `Porto`    | True | True |

### Added/removed attribute example

```
LeftJSON:  { "Meta": { "City": "Lisbon" } }
RightJSON: { "Meta": { "City": "Lisbon", "Zip": "1000-001" } }
```

| Attribute   | PreviousValue | NewValue    | HasPreviousValue | HasNewValue |
|-------------|---------------|-------------|:---:|:---:|
| `Meta.Zip`  | *(empty)*     | `1000-001`  | **False** | True |

## Action: `JSON_Listify`

**Inputs:** `JSONIn` (Text), `Path` (Text)
**Output:** `JSONOut` (Text)

Replaces the object found at `Path` with an array of `{key, value}` pairs, one per original
property. Useful when a JSON payload has dynamic/unpredictable property names that can't be
mapped to a fixed OutSystems structure directly - listify it first, then map the resulting
`{key, value}` list.

- `Path` is a dot-separated path into the JSON (e.g. `"Meta.Custom"`). Use an empty string to
  listify the root object itself.
- If `Path` points inside an array, the listification is applied to every element of that array.

### Example

```
JSONIn: { "Id": 1, "Meta": { "City": "Lisbon", "Country": "Portugal" } }
Path:   "Meta"
```

`JSONOut`:

```json
{"Id":1,"Meta":[{"key":"City","value":"Lisbon"},{"key":"Country","value":"Portugal"}]}
```

## Error handling

All three actions throw an exception (visible in ODC as a runtime error) when:

- `LeftJSON`/`RightJSON`/`JSONIn` is empty, blank, or not valid JSON.
- For `Diff`/`DiffDeep`: `LeftJSON` or `RightJSON`'s root is not a JSON object (e.g. a JSON array
  or a plain string/number) - only objects are supported.

Wrap calls in a `Try-Catch` block if your flow needs to handle malformed input gracefully instead
of failing the whole action flow.
