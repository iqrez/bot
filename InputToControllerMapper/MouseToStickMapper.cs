using System;

namespace InputToControllerMapper
{
    /// <summary>
    /// Shapes available when converting mouse magnitude to stick output.
    /// </summary>
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
        private MouseToStickConfig config = new MouseToStickConfig();
        private float smoothX;
        private float smoothY;

        /// <summary>
        /// Replace the current configuration.
        /// </summary>
        public void SetConfig(MouseToStickConfig cfg)
        {
            config = cfg;
            smoothX = 0f;
            smoothY = 0f;
        }

        /// <summary>
        /// Process mouse delta and return stick axis values in range -32767..32767.
        /// </summary>
        public (short X, short Y) Map(int deltaX, int deltaY)
        {
            float x = ProcessAxis(deltaX, config.SensitivityX, ref smoothX);
            float y = ProcessAxis(deltaY, config.SensitivityY, ref smoothY);

            if (config.InvertY)
                y = -y;

            short sx = (short)Math.Clamp(x * 32767f, -32767f, 32767f);
            short sy = (short)Math.Clamp(y * 32767f, -32767f, 32767f);
            return (sx, sy);
        }

        private float ProcessAxis(int delta, float sensitivity, ref float smooth)
        {
            float value = delta * sensitivity;

            if (config.Smoothing > 0f)
            {
                smooth += (value - smooth) * config.Smoothing;
                value = smooth;
            }

            float sign = MathF.Sign(value);
            float magnitude = MathF.Abs(value);

            if (magnitude < config.Deadzone)
                return 0f;

            magnitude = (magnitude - config.Deadzone) / (1f - config.Deadzone);
            magnitude = ApplyCurve(magnitude);

            return sign * magnitude;
        }

        private float ApplyCurve(float value)
        {
            value = MathF.Min(MathF.Max(value, 0f), 1f);

            switch (config.Curve)
            {
                case StickCurveShape.Linear:
                    return value;
                case StickCurveShape.Exponential:
                    return MathF.Pow(value, config.Exponent);
                case StickCurveShape.DualZone:
                    if (value < config.DualZoneThreshold)
                    {
                        float inner = value / config.DualZoneThreshold;
                        return MathF.Pow(inner, config.Exponent) * config.DualZoneThreshold;
                    }
                    else
                    {
                        float outer = (value - config.DualZoneThreshold) / (1f - config.DualZoneThreshold);
                        return MathF.Pow(outer, config.OuterExponent) * (1f - config.DualZoneThreshold) + config.DualZoneThreshold;
                    }
                default:
                    return value;
            }
        }
    }
}
