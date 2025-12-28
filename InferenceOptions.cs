using System;
using LLama.Common;

namespace RelaxTimerApp
{
    public static class InferenceOptions
    {
        /// <summary>
        /// Configures sampling parameters for the inference.
        /// Tries to use as many knobs as LLamaSharp exposes; falls back gracefully if properties don't exist.
        /// </summary>
        public static InferenceParams GetInferenceParams(
            int maxTokens = 256,
            float temperature = 0.85f,
            float topP = 0.92f,
            int topK = 50,
            float repeatPenalty = 1.12f,
            int repeatLastN = 256)
        {
            var p = new InferenceParams
            {
                MaxTokens = maxTokens
            };

            // These properties exist in newer LLamaSharp versions; set them via reflection to stay compatible.
            TrySet(p, "Temperature", temperature);
            TrySet(p, "TopP", topP);
            TrySet(p, "TopK", topK);
            TrySet(p, "RepeatPenalty", repeatPenalty);
            TrySet(p, "RepeatLastN", repeatLastN);

            // Common stop patterns (if AntiPrompts exists)
            TrySet(p, "AntiPrompts", ModelExtensions.StopPatterns);

            return p;
        }

        private static void TrySet<T>(InferenceParams target, string propertyName, T value)
        {
            var prop = target.GetType().GetProperty(propertyName);
            if (prop == null || !prop.CanWrite)
                return;

            try
            {
                // Basic attempt; handles exact type match and some numeric conversions.
                if (value is IConvertible && prop.PropertyType != typeof(string))
                {
                    var converted = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    prop.SetValue(target, converted);
                }
                else
                {
                    prop.SetValue(target, value);
                }
            }
            catch
            {
                // Ignore – different LLamaSharp versions expose different knobs.
            }
        }
    }
}