using NUnit.Framework;
using OutSystems.ExternalLibraries.SDK;
using JSONPair = DoiTLean.DiffJSON.Structures.JSONPair;
using DoiTLean.DiffJSON;

namespace DoiTLean.DiffJSON.UnitTests;

public class DiffJSONTests {

    /// <summary>
    /// Tests if the JSONPair constructor correctly creates the JSONPair struct
    /// </summary>
    [Test]
    public void JSONPairStructureIsCorrectlyCreatedWhenGivenPair() {
        var pairStruct = new JSONPair("A1","1","2");
        Assert.That(pairStruct.Attribute, Is.EqualTo("A1"));
        Assert.That(pairStruct.PreviousValue, Is.EqualTo("1"));
        Assert.That(pairStruct.NewValue, Is.EqualTo("2"));
    }

    /// <summary>
    /// Tests if the DiffJSON is working correctly
    /// </summary>
    [Test]
    public void JSONDiffIsCorrectlyCreatedWhenGivenTwoEqualsJSON()
    {
        string JSON1 = """
                {
                  "Id": 165,
                  "RefNumber": 1224,
                  "DateCreated": "2025-10-01T16:23:50Z",
                  "DateModified": "2025-10-01T16:23:50Z",
                  "CreatedUserId": "8548c888-fba4-43ff-bec0-bcb91866c1d9",
                  "ModifiedUserId": "ba986cd5-915e-40e5-80cd-f3100c4f5542",
                  "EstimateId": 70,
                  "Name": "Going to turn you into an assembly",
                  "Description": "descriptions",
                  "Estimator": "ed86ffd9-1c13-4ed3-809b-a4916c7e2ed8",
                  "DateStarted": "2025-10-01",
                  "LaborEstimate": 17785.71428571,
                  "MaterialEstimate": 1623.88571428,
                  "LocationId": 477
                }
                """;
        string JSON2 = """
                {
                  "Id": 165,
                  "RefNumber": 1224,
                  "DateCreated": "2025-10-01T16:23:50Z",
                  "DateModified": "2025-10-01T16:23:50Z",
                  "CreatedUserId": "8548c888-fba4-43ff-bec0-bcb91866c1d9",
                  "ModifiedUserId": "ba986cd5-915e-40e5-80cd-f3100c4f5542",
                  "EstimateId": 70,
                  "Name": "Going to turn you into an assembly",
                  "Description": "descriptions",
                  "Estimator": "ed86ffd9-1c13-4ed3-809b-a4916c7e2ed8",
                  "DateStarted": "2025-10-01",
                  "LaborEstimate": 17785.71428571,
                  "MaterialEstimate": 1623.88571428,
                  "LocationId": 477
                }
                """;

        List<JSONPair> jSONPairs = new DiffJSON().Diff(JSON1, JSON2);
        Assert.That(jSONPairs.Count, Is.EqualTo(0));
    }


    /// <summary>
    /// Tests if the DiffJSON is working correctly
    /// </summary>
    [Test]
    public void JSONDiffIsCorrectlyCreatedWhenGivenTwoDifferentJSON()
    {
        string JSON1 = """
                {
                  "Id": 165,
                  "RefNumber": 1224,
                  "DateCreated": "2025-10-01T16:23:50Z",
                  "DateModified": "2025-10-01T16:23:50Z",
                  "CreatedUserId": "8548c888-fba4-43ff-bec0-bcb91866c1d9",
                  "ModifiedUserId": "ba986cd5-915e-40e5-80cd-f3100c4f5542",
                  "EstimateId": 70,
                  "Name": "Going to turn you into an assembly",
                  "Description": "descriptions",
                  "Estimator": "ed86ffd9-1c13-4ed3-809b-a4916c7e2ed8",
                  "DateStarted": "2025-10-01",
                  "LaborEstimate": 17785.71428571,
                  "MaterialEstimate": 1623.88571428,
                  "LocationId": 477
                }
                """;
        string JSON2 = """
                {
                  "Id": 166,
                  "RefNumber": 1224,
                  "DateCreated": "2025-10-01T16:23:50Z",
                  "DateModified": "2025-10-01T16:23:50Z",
                  "CreatedUserId": "8548c888-fba4-43ff-bec0-bcb91866c1d9",
                  "ModifiedUserId": "ba986cd5-915e-40e5-80cd-f3100c4f5542",
                  "EstimateId": 70,
                  "Name": "Going to turn you into an assembly",
                  "Description": "descriptions",
                  "Estimator": "ed86ffd9-1c13-4ed3-809b-a4916c7e2ed8",
                  "DateStarted": "2025-10-01",
                  "LaborEstimate": 17785.71428571,
                  "MaterialEstimate": 1623.88571428,
                  "LocationId": 477
                }
                """;

        List<JSONPair> jSONPairs = new DiffJSON().Diff(JSON1, JSON2);
        Assert.That(jSONPairs.Count, Is.EqualTo(1));
    }

    /// <summary>
    /// An attribute present only in the right json must be reported with HasPreviousValue = false,
    /// not just an empty PreviousValue (which would be indistinguishable from a real empty string).
    /// </summary>
    [Test]
    public void JSONPairMarksAttributeAsAddedWhenOnlyInRightJSON()
    {
        string json1 = """{ "Id": 1 }""";
        string json2 = """{ "Id": 1, "Name": "new" }""";

        List<JSONPair> jSONPairs = new DiffJSON().Diff(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Name"));
        Assert.That(jSONPairs[0].HasPreviousValue, Is.False);
        Assert.That(jSONPairs[0].PreviousValue, Is.EqualTo(""));
        Assert.That(jSONPairs[0].HasNewValue, Is.True);
        Assert.That(jSONPairs[0].NewValue, Is.EqualTo("new"));
    }

    /// <summary>
    /// An attribute present only in the left json must be reported with HasNewValue = false,
    /// not just an empty NewValue (which would be indistinguishable from a real empty string).
    /// </summary>
    [Test]
    public void JSONPairMarksAttributeAsRemovedWhenOnlyInLeftJSON()
    {
        string json1 = """{ "Id": 1, "Name": "old" }""";
        string json2 = """{ "Id": 1 }""";

        List<JSONPair> jSONPairs = new DiffJSON().Diff(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Name"));
        Assert.That(jSONPairs[0].HasPreviousValue, Is.True);
        Assert.That(jSONPairs[0].PreviousValue, Is.EqualTo("old"));
        Assert.That(jSONPairs[0].HasNewValue, Is.False);
        Assert.That(jSONPairs[0].NewValue, Is.EqualTo(""));
    }

    /// <summary>
    /// A nested object change is reported as the whole parent attribute changing, serialized
    /// as the full before/after object - not a nested key-by-key diff. See DiffJSON.Diff comments.
    /// </summary>
    [Test]
    public void JSONDiffReportsWholeParentAttributeWhenNestedObjectChanges()
    {
        string json1 = """{ "Id": 1, "Meta": { "City": "Lisbon" } }""";
        string json2 = """{ "Id": 1, "Meta": { "City": "Porto" } }""";

        List<JSONPair> jSONPairs = new DiffJSON().Diff(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Meta"));
        Assert.That(jSONPairs[0].PreviousValue, Does.Contain("Lisbon"));
        Assert.That(jSONPairs[0].NewValue, Does.Contain("Porto"));
    }

    /// <summary>
    /// A changed array attribute is reported as the whole attribute changing, same as nested objects.
    /// </summary>
    [Test]
    public void JSONDiffReportsWholeAttributeWhenArrayValueChanges()
    {
        string json1 = """{ "Id": 1, "Tags": ["a", "b"] }""";
        string json2 = """{ "Id": 1, "Tags": ["a", "c"] }""";

        List<JSONPair> jSONPairs = new DiffJSON().Diff(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Tags"));
    }

    /// <summary>
    /// Unlike Diff(), DiffDeep() reports a nested modified attribute using a dot-separated path,
    /// with the value coming straight from the delta leaf, not the whole parent object.
    /// </summary>
    [Test]
    public void DiffDeepReportsNestedModifiedAttributeWithDottedPath()
    {
        string json1 = """{ "Id": 1, "Meta": { "City": "Lisbon" } }""";
        string json2 = """{ "Id": 1, "Meta": { "City": "Porto" } }""";

        List<JSONPair> jSONPairs = new DiffJSON().DiffDeep(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Meta.City"));
        Assert.That(jSONPairs[0].PreviousValue, Is.EqualTo("Lisbon"));
        Assert.That(jSONPairs[0].NewValue, Is.EqualTo("Porto"));
    }

    /// <summary>
    /// DiffDeep() recurses through more than one level of nesting.
    /// </summary>
    [Test]
    public void DiffDeepReportsDoublyNestedModifiedAttributeWithDottedPath()
    {
        string json1 = """{ "Meta": { "Address": { "City": "Lisbon" } } }""";
        string json2 = """{ "Meta": { "Address": { "City": "Porto" } } }""";

        List<JSONPair> jSONPairs = new DiffJSON().DiffDeep(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Meta.Address.City"));
    }

    /// <summary>
    /// DiffDeep() marks a nested attribute added only in the right json as HasPreviousValue = false.
    /// </summary>
    [Test]
    public void DiffDeepMarksNestedAttributeAsAddedWhenOnlyInRightJSON()
    {
        string json1 = """{ "Meta": { "City": "Lisbon" } }""";
        string json2 = """{ "Meta": { "City": "Lisbon", "Zip": "1000-001" } }""";

        List<JSONPair> jSONPairs = new DiffJSON().DiffDeep(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Meta.Zip"));
        Assert.That(jSONPairs[0].HasPreviousValue, Is.False);
        Assert.That(jSONPairs[0].HasNewValue, Is.True);
        Assert.That(jSONPairs[0].NewValue, Is.EqualTo("1000-001"));
    }

    /// <summary>
    /// DiffDeep() marks a nested attribute removed from the right json as HasNewValue = false.
    /// </summary>
    [Test]
    public void DiffDeepMarksNestedAttributeAsRemovedWhenOnlyInLeftJSON()
    {
        string json1 = """{ "Meta": { "City": "Lisbon", "Zip": "1000-001" } }""";
        string json2 = """{ "Meta": { "City": "Lisbon" } }""";

        List<JSONPair> jSONPairs = new DiffJSON().DiffDeep(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Meta.Zip"));
        Assert.That(jSONPairs[0].HasPreviousValue, Is.True);
        Assert.That(jSONPairs[0].PreviousValue, Is.EqualTo("1000-001"));
        Assert.That(jSONPairs[0].HasNewValue, Is.False);
    }

    /// <summary>
    /// DiffDeep() still reports a changed array as a single whole-attribute entry, same as Diff().
    /// </summary>
    [Test]
    public void DiffDeepReportsWholeAttributeWhenArrayValueChanges()
    {
        string json1 = """{ "Id": 1, "Tags": ["a", "b"] }""";
        string json2 = """{ "Id": 1, "Tags": ["a", "c"] }""";

        List<JSONPair> jSONPairs = new DiffJSON().DiffDeep(json1, json2);

        Assert.That(jSONPairs.Count, Is.EqualTo(1));
        Assert.That(jSONPairs[0].Attribute, Is.EqualTo("Tags"));
    }

    /// <summary>
    /// Malformed JSON must fail with a clear ArgumentException rather than an opaque parser error.
    /// </summary>
    [Test]
    public void DiffThrowsArgumentExceptionWhenLeftJSONIsMalformed()
    {
        Assert.Throws<ArgumentException>(() => new DiffJSON().Diff("not json", """{ "Id": 1 }"""));
    }

    /// <summary>
    /// A JSON array root is rejected, since this library only understands top-level object attributes.
    /// </summary>
    [Test]
    public void DiffThrowsArgumentExceptionWhenRootIsNotAnObject()
    {
        Assert.Throws<ArgumentException>(() => new DiffJSON().Diff("""[1,2,3]""", """{ "Id": 1 }"""));
    }

    /// <summary>
    /// Empty/whitespace input is rejected explicitly instead of failing deep inside JToken.Parse.
    /// </summary>
    [Test]
    public void DiffThrowsArgumentExceptionWhenInputIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new DiffJSON().Diff("", """{ "Id": 1 }"""));
    }

    /// <summary>
    /// JSON_Listify turns an object's properties into a list of {key, value} pairs.
    /// </summary>
    [Test]
    public void JSONListifyConvertsRootObjectIntoKeyValueArray()
    {
        string json = """{ "A": 1, "B": "two" }""";

        string result = new DiffJSON().JSON_Listify(json, "");

        Assert.That(result, Is.EqualTo("""[{"key":"A","value":1},{"key":"B","value":"two"}]"""));
    }

    /// <summary>
    /// JSON_Listify only listifies the object found at Path, leaving the rest untouched.
    /// </summary>
    [Test]
    public void JSONListifyConvertsOnlyObjectAtGivenPath()
    {
        string json = """{ "Id": 1, "Meta": { "A": 1, "B": 2 } }""";

        string result = new DiffJSON().JSON_Listify(json, "Meta");

        Assert.That(result, Is.EqualTo("""{"Id":1,"Meta":[{"key":"A","value":1},{"key":"B","value":2}]}"""));
    }

    /// <summary>
    /// When Path points into an array, listification applies to every element in the array.
    /// </summary>
    [Test]
    public void JSONListifyAppliesToEveryArrayElementAlongPath()
    {
        string json = """{ "Items": [ { "A": 1 }, { "A": 2 } ] }""";

        string result = new DiffJSON().JSON_Listify(json, "Items");

        Assert.That(result, Is.EqualTo("""{"Items":[[{"key":"A","value":1}],[{"key":"A","value":2}]]}"""));
    }

    /// <summary>
    /// Empty/whitespace input is rejected explicitly instead of failing deep inside JToken.Parse.
    /// </summary>
    [Test]
    public void JSONListifyThrowsArgumentExceptionWhenInputIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new DiffJSON().JSON_Listify("", ""));
    }

}