#nullable enable
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Nefarius.ViGEm.Client.Targets.Xbox360
{
    public enum Xbox360Button { A }
    public enum Xbox360Axis { LX }
    public enum Xbox360Slider { LeftTrigger }

    public interface IXbox360Controller
    {
        void Connect();
        void Disconnect();
        void Dispose();
        void SubmitReport();
        void SetAxisValue(Xbox360Axis axis, short value);
        void SetButtonState(Xbox360Button button, bool pressed);
        void SetSliderValue(Xbox360Slider slider, byte value);
    }
}

namespace Nefarius.ViGEm.Client
{
    using Nefarius.ViGEm.Client.Targets.Xbox360;
    public class ViGEmClient : System.IDisposable
    {
        public void Dispose() { }
        public IXbox360Controller CreateXbox360Controller() => throw new System.NotImplementedException();
    }
}
