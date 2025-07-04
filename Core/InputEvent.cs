namespace Core
{
    /// <summary>
    /// Generic input event used by <see cref="MappingEngine"/>
    /// to represent keyboard, mouse and analog events.
    /// </summary>
    public class InputEvent
    {
        public InputType Type { get; }
        public string Code { get; }
        public float Value { get; }

        public InputEvent(InputType type, string code, float value)
        {
            Type = type;
            Code = code;
            Value = value;
        }
    }
}
