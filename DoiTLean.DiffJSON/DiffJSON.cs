using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using JsonDiffPatchDotNet;
using System.Text;
using DoiTLean.DiffJSON.Structures;
using Newtonsoft.Json;
using System.IO;

namespace DoiTLean.DiffJSON {
    /// <summary>
    ///  The DiffJSON interface defines the methods for parsing two json and returns a list of JSONPairs with the previous and new values for each difference found.
    /// </summary>
    public class DiffJSON : IDiffJSON {

        // JsonDiffPatch holds no per-call state (we never touch its Options), so a single
        // instance can be safely reused/shared across concurrent Diff() calls instead of
        // allocating one per call.
        private static readonly JsonDiffPatch Jdp = new JsonDiffPatch();

        /// <summary>
        ///  Parses Left and Right JSON and returns a list of JSONPairs with the previous and new values for each difference found
        /// </summary>
        public List<JSONPair> Diff(string leftJSON, string rightJSON)
        {
            List<JSONPair> resultList = new List<JSONPair>();

            var left = ParseJsonObject(leftJSON, nameof(leftJSON));
            var right = ParseJsonObject(rightJSON, nameof(rightJSON));

            // Delta describing how to turn "left" into "right" (JsonDiffPatch's own before/after
            // values inside the delta are not used here - only the changed property NAMES matter,
            // since the actual previous/new values are re-read from the source documents below).
            JToken delta = Jdp.Diff(left, right);
            if (delta is null || delta.Type == JTokenType.Null)
                return resultList;

            // Only look at direct properties of the delta. This intentionally skips JsonDiffPatch's
            // array-diff metadata (e.g. the "_t" marker and underscore-prefixed keys), which would
            // otherwise be misreported as changed attributes if either JSON's root were an array.
            foreach (JProperty child in delta.Children<JProperty>())
            {
                if (child.Name == "_t" || child.Name.StartsWith("_"))
                    continue;

                var (hasPrevious, previousValue) = TryGetValueFromJTOKEN(left, child.Name);
                var (hasNew, newValue) = TryGetValueFromJTOKEN(right, child.Name);
                resultList.Add(new JSONPair(child.Name, previousValue, newValue, hasPrevious, hasNew));
            }

            return resultList;
        }

        /// <summary>
        /// Parses a JSON document and ensures its root is an object, since this library only
        /// understands top-level attribute/value pairs. Throws a clear ArgumentException instead
        /// of letting a confusing InvalidCastException surface later for arrays/scalars, or an
        /// opaque JsonReaderException for malformed input.
        /// </summary>
        private static JToken ParseJsonObject(string json, string paramName)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Value cannot be null, empty or whitespace.", paramName);

            JToken token;
            try
            {
                token = JToken.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                throw new ArgumentException($"Value is not valid JSON: {ex.Message}", paramName, ex);
            }

            if (token.Type != JTokenType.Object)
                throw new ArgumentException($"Value must be a JSON object, but was a {token.Type}.", paramName);

            return token;
        }

        /// <summary>
        /// Looks up a top-level property value by name. The returned "exists" flag lets callers
        /// tell an added/removed attribute (no value on one side) apart from an attribute whose
        /// actual value happens to be an empty string.
        /// </summary>
        private static (bool Exists, string Value) TryGetValueFromJTOKEN(JToken tokenObj, string key)
        {
            // Only plain objects expose named properties; anything else (e.g. an array root) has none.
            if (tokenObj is JObject obj && obj.TryGetValue(key, out JToken? value))
                return (true, value?.ToString() ?? string.Empty);

            return (false, string.Empty);
        }

        /// <summary>
        /// Same purpose as Diff(), but walks nested objects recursively and reports each changed
        /// leaf attribute on its own, using a dot-separated path as Attribute (e.g. "Meta.City")
        /// instead of the single "whole parent object changed" entry Diff() produces. Prefer this
        /// when consumers need attribute-level granularity inside nested objects; prefer Diff()
        /// when a nested change should just be treated as "this whole object changed".
        /// Changed arrays are still reported as a whole, same as Diff() - JsonDiffPatch's
        /// array-diff format (index moves, underscore-prefixed deletions) is not walked.
        /// </summary>
        public List<JSONPair> DiffDeep(string leftJSON, string rightJSON)
        {
            List<JSONPair> resultList = new List<JSONPair>();

            var left = ParseJsonObject(leftJSON, nameof(leftJSON));
            var right = ParseJsonObject(rightJSON, nameof(rightJSON));

            JToken delta = Jdp.Diff(left, right);
            if (delta is null || delta.Type == JTokenType.Null)
                return resultList;

            CollectDeepDiffs(left, right, delta, string.Empty, resultList);
            return resultList;
        }

        /// <summary>
        /// Recursively walks a JsonDiffPatch delta tree, appending one JSONPair per changed leaf
        /// attribute. "prefix" accumulates the dot-separated path down to the current nesting level.
        /// </summary>
        private static void CollectDeepDiffs(JToken left, JToken right, JToken delta, string prefix, List<JSONPair> results)
        {
            foreach (JProperty child in delta.Children<JProperty>())
            {
                if (child.Name == "_t")
                    continue; // array-type marker, not an attribute

                string attribute = prefix.Length == 0 ? child.Name : $"{prefix}.{child.Name}";

                if (child.Value is JArray change)
                {
                    // A JsonDiffPatch leaf delta: [newValue] = added, [oldValue, newValue] = modified,
                    // [oldValue, 0, 0] = deleted. The 3-element "moved"/"text-diff" op codes are not
                    // expected here since array contents are handled separately below, and text-diff
                    // mode is never enabled on the JsonDiffPatch instance used above.
                    results.Add(BuildLeafChange(attribute, change));
                    continue;
                }

                if (child.Value is JObject nested)
                {
                    if (nested["_t"]?.ToString() == "a")
                    {
                        // Array contents changed. Same tradeoff as Diff(): report the whole
                        // attribute as changed instead of walking the array-diff format.
                        var (hasPrevious, previousValue) = TryGetValueFromJTOKEN(left, child.Name);
                        var (hasNew, newValue) = TryGetValueFromJTOKEN(right, child.Name);
                        results.Add(new JSONPair(attribute, previousValue, newValue, hasPrevious, hasNew));
                    }
                    else
                    {
                        JToken leftChild = (left as JObject)?[child.Name] ?? new JObject();
                        JToken rightChild = (right as JObject)?[child.Name] ?? new JObject();
                        CollectDeepDiffs(leftChild, rightChild, nested, attribute, results);
                    }
                }
            }
        }

        private static JSONPair BuildLeafChange(string attribute, JArray change)
        {
            return change.Count switch
            {
                1 => new JSONPair(attribute, string.Empty, ToStringOrEmpty(change[0]), hasPreviousValue: false, hasNewValue: true),
                2 => new JSONPair(attribute, ToStringOrEmpty(change[0]), ToStringOrEmpty(change[1]), hasPreviousValue: true, hasNewValue: true),
                _ => new JSONPair(attribute, ToStringOrEmpty(change[0]), string.Empty, hasPreviousValue: true, hasNewValue: false),
            };
        }

        private static string ToStringOrEmpty(JToken token) =>
            token is null || token.Type == JTokenType.Null ? string.Empty : token.ToString();

        /// <summary>
        /// Replaces the object located at Path with an array of {key, value} pairs, one per
        /// original property. Useful to turn JSON objects with dynamic/unknown property names
        /// into a shape OutSystems can map to a static structure (a list of records).
        /// </summary>
        public string JSON_Listify(string JSONIn, string Path)
        {
            if (string.IsNullOrWhiteSpace(JSONIn))
                throw new ArgumentException("Value cannot be null, empty or whitespace.", nameof(JSONIn));

            if (Path is null)
                throw new ArgumentNullException(nameof(Path));

            JToken parsed;
            try
            {
                parsed = JToken.Parse(JSONIn);
            }
            catch (JsonReaderException ex)
            {
                throw new ArgumentException($"Value is not valid JSON: {ex.Message}", nameof(JSONIn), ex);
            }

            string[] path = Path.Trim().Split('.');
            JToken root = Inner_Listify(parsed, path, 0);

            StringBuilder sb = new StringBuilder();

            using (JsonWriter json = new JsonTextWriter(new StringWriter(sb)))
            {
                json.Formatting = Formatting.None;
                json.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                root.WriteTo(json);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Walks "root" following "path" segment by segment, and once the target location is
        /// reached, converts the object found there into a JArray of {key, value} pairs.
        /// Arrays encountered along the way are recursed into element-by-element, since the same
        /// path applies to every item.
        /// </summary>
        private JToken Inner_Listify(JToken root, string[] path, int index)
        {

            if (root.Type == JTokenType.Array)
            {
                // if we're at an array, simply apply to all elements
                JArray arr = (JArray)root;
                for (int i = 0; i < arr.Count; i++)
                {
                    arr[i] = Inner_Listify(arr[i], path, index);
                }
                return arr;
            }

            if (path.Length == index)
            {
                // nothing to do if we're not at an object
                if (root.Type != JTokenType.Object)
                    return root;

                // do the listification
                JObject obj = (JObject)root;
                JArray res = new JArray();
                foreach (JProperty p in obj.Properties())
                {
                    JObject tmp = new JObject();
                    tmp["key"] = p.Name;
                    tmp["value"] = p.Value;
                    res.Add(tmp);
                }
                return res;

            }
            else
            {
                // empty path segments (e.g. from a leading/trailing '.') are simply skipped
                if (path[index].Equals(""))
                    return Inner_Listify(root, path, index + 1);

                // path[index] != ""
                if (root.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)root;
                    JToken? r = obj[path[index]];

                    if (r == null || r.Type == JTokenType.Null)
                        return root;

                    obj[path[index]] = Inner_Listify(r, path, index + 1);
                    return root;
                }

                return root;
            }
        }

    }
}
