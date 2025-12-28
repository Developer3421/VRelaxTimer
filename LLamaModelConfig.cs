using System;
using System.IO;
using LLama.Common;

namespace RelaxTimerApp
{
    /// <summary>
    /// Configuration and utility methods for loading the LLama model
    /// </summary>
    public static class LLamaModelConfig
    {
        /// <summary>
        /// Default model path
        /// </summary>
        public const string DefaultModelPath = "GPT-2-fine-tuned-mental-health.Q8_0.gguf";

        /// <summary>
        /// Creates model parameters with appropriate settings
        /// </summary>
        /// <param name="modelPath">Path to the model file</param>
        /// <returns>Configured ModelParams</returns>
        public static ModelParams CreateModelParams(string modelPath = DefaultModelPath)
        {
            // Ensure model exists and log helpful error if not
            if (!File.Exists(modelPath))
            {
                var currentDir = Directory.GetCurrentDirectory();
                var message = $"Model file not found at {Path.GetFullPath(modelPath)}. " + 
                              $"Current directory is {currentDir}. " +
                              $"Please ensure the model file is in the correct location.";

                Console.WriteLine(message);
                throw new FileNotFoundException(message, modelPath);
            }

            // Many GPT-2 exports are trained/used with ~1024 context. A too-large context can waste RAM and slow CPU inference.
            var p = new ModelParams(modelPath)
            {
                ContextSize = 1024,  // Smaller context for GPT-2 compatibility
                GpuLayerCount = 0    // CPU only for compatibility
            };

            // Try to use more CPU threads if the current LLamaSharp version exposes it.
            TrySet(p, "Threads", Math.Max(1, Environment.ProcessorCount - 1));

            return p;
        }

        private static void TrySet<T>(ModelParams target, string propertyName, T value)
        {
            var prop = target.GetType().GetProperty(propertyName);
            if (prop == null || !prop.CanWrite)
                return;

            try
            {
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
                // Ignore – property may not exist in this version.
            }
        }
    }
}
