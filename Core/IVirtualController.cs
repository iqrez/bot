namespace Core
{
    /// <summary>
    /// Abstracts the controller implementation used by <see cref="MappingEngine"/>.
    /// </summary>
    public interface IVirtualController
    {
        void SetButtonState(string button, bool pressed);
        void SetAxisValue(string axis, short value);
        void SetTriggerValue(string trigger, byte value);
        void SetDPadState(string direction, bool pressed);
        void Submit();
    }
}
