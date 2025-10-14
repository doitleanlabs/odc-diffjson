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

}