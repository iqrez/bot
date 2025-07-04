using System;

namespace InputToControllerMapper
{
    public enum StickCurveShape
    {
        Linear,
        Exponential,
        DualZone
    }

    /// <summary>
    /// Translates raw mouse movement into normalized stick values.
    /// </summary>
    public class MouseToStickMapper
    {
        public float SensitivityX { get; set; } = 1f;
        public float SensitivityY { get; set; } = 1f;
        public float Deadzone { get; set; } = 0f;
        public float Acceleration { get; set; } = 0f;
        public float Smoothing { get; set; } = 0f;
        public bool InvertY { get; set; }
        public StickCurveShape Curve { get; set; } = StickCurveShape.Linear;
        public float Exponent { get; set; } = 1.5f;          // used for Exponential and DualZone curves
        public float OuterExponent { get; set; } = 1.0f;      // used for DualZone curves
        public float DualZoneThreshold { get; set; } = 0.5f;  // 0..1

        private float smoothX;
        private float smoothY;

        /// <summary>
        /// Process mouse delta and return stick axis values in range -32767..32767.
        /// </summary>
        public (short X, short Y) Map(int deltaX, int deltaY)
        {
            float x = ProcessAxis(deltaX, SensitivityX, ref smoothX);
            float y = ProcessAxis(deltaY, SensitivityY, ref smoothY);

            if (InvertY)
                y = -y;

            short sx = (short)Math.Clamp(x * 32767f, -32767f, 32767f);
            short sy = (short)Math.Clamp(y * 32767f, -32767f, 32767f);
            return (sx, sy);
        }

        private float ProcessAxis(int delta, float sensitivity, ref float smooth)
        {
            float value = delta * sensitivity;

            if (Acceleration > 0f)
                value *= 1f + MathF.Abs(value) * Acceleration;

            if (Smoothing > 0f)
            {
                smooth += (value - smooth) * Smoothing;
                value = smooth;
            }

            float sign = MathF.Sign(value);
            float magnitude = MathF.Abs(value);

            if (magnitude < Deadzone)
                return 0f;

            magnitude = (magnitude - Deadzone) / (1f - Deadzone);
            magnitude = ApplyCurve(magnitude);

            return sign * magnitude;
        }

        private float ApplyCurve(float value)
        {
            value = MathF.Min(MathF.Max(value, 0f), 1f);

            switch (Curve)
            {
                case StickCurveShape.Linear:
                    return value;
                case StickCurveShape.Exponential:
                    return MathF.Pow(value, Exponent);
                case StickCurveShape.DualZone:
                    if (value < DualZoneThreshold)
                    {
                        float inner = value / DualZoneThreshold;
                        return MathF.Pow(inner, Exponent) * DualZoneThreshold;
                    }
                    else
                    {
                        float outer = (value - DualZoneThreshold) / (1f - DualZoneThreshold);
                        return MathF.Pow(outer, OuterExponent) * (1f - DualZoneThreshold) + DualZoneThreshold;
                    }
                default:
                    return value;
            }
        }
    }
}
