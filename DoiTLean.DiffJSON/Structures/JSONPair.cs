using OutSystems.ExternalLibraries.SDK;

namespace DoiTLean.DiffJSON.Structures
{
    /// <summary>
    /// The JSON Pair struct represents a record of attribute, previous and new value for each attribute compared by the DiffJSON class
    /// </summary>
    [OSStructure(Description = "Represents an JSONPair (Attribute,Previous,New).")]
    public struct JSONPair
    {
        [OSStructureField(DataType = OSDataType.Text, Description = "The attribute in the left json", IsMandatory = true)]
        /// <summary>
        /// Represents the attribute in the original json
        /// </summary>
        public string Attribute;


        [OSStructureField(DataType = OSDataType.Text, Description = "The previous value of the attribute in the left json", IsMandatory = true)]
        /// <summary>
        /// Represents the value of attribute in the left json
        /// </summary>
        public string PreviousValue;


        [OSStructureField(DataType = OSDataType.Text, Description = "The new value of the attribute in the right json", IsMandatory = true)]
        /// <summary>
        /// Represents the value of the attribute in the right json
        /// </summary>
        public string NewValue;

        [OSStructureField(DataType = OSDataType.Boolean, Description = "True if the attribute existed in the left json. False means the attribute was added by the right json, and PreviousValue is empty because the attribute never had a value, not because its value was an empty string.", IsMandatory = false)]
        /// <summary>
        /// True if the attribute existed in the left json.
        /// </summary>
        public bool HasPreviousValue;

        [OSStructureField(DataType = OSDataType.Boolean, Description = "True if the attribute exists in the right json. False means the attribute was removed, and NewValue is empty because the attribute has no value, not because its value was set to an empty string.", IsMandatory = false)]
        /// <summary>
        /// True if the attribute exists in the right json.
        /// </summary>
        public bool HasNewValue;

        /// <summary>
        /// Constructs a JSONPair struct. hasPreviousValue/hasNewValue default to true for callers
        /// that already know both sides had a value (e.g. hand-built pairs, existing test code).
        /// </summary>
        public JSONPair(string inputAttribute, string inputLeftValue, string inputRightValue, bool hasPreviousValue = true, bool hasNewValue = true) : this()
        {
            Attribute = inputAttribute ?? string.Empty;
            PreviousValue = inputLeftValue ?? string.Empty;
            NewValue = inputRightValue ?? string.Empty;
            HasPreviousValue = hasPreviousValue;
            HasNewValue = hasNewValue;
        }
    }

}