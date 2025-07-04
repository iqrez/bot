using System;

namespace InputToControllerMapper
{
    /// <summary>
    /// Type of curve used when mapping mouse input to stick magnitude.
    /// </summary>
    public enum StickCurve
    {
        Linear,
        Exponential,
        DualZone
    }

    /// <summary>
    /// Configuration options for <see cref="MouseToStickMapper"/>.
    /// </summary>
    public class MouseToStickConfig
    {
        public float SensitivityX { get; set; } = 1f;
        public float SensitivityY { get; set; } = 1f;
        public float Deadzone { get; set; } = 0f;
        public float Smoothing { get; set; } = 0f;
        public bool InvertY { get; set; }
        public StickCurve Curve { get; set; } = StickCurve.Linear;
        public float Exponent { get; set; } = 1.5f;         // used for Exponential and DualZone curves
        public float OuterExponent { get; set; } = 1.0f;     // used for DualZone curves
        public float DualZoneThreshold { get; set; } = 0.5f; // 0..1
    }
}
