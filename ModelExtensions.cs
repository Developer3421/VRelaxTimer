using System;
using System.Collections.Generic;
using System.Text;
using LLama;
using LLama.Common;

namespace RelaxTimerApp
{
    /// <summary>
    /// Provides extension methods and utilities for enhancing model behavior
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// List of patterns that indicate the model should stop generating
        /// </summary>
        public static readonly string[] StopPatterns = new[]
        {
            "User:",
            "\n\nUser:",
            "\n\n\n",
            "<end>",
            "Human:",
            "Question:"
        };
        
        /// <summary>
        /// Checks if text contains any stop patterns that indicate generation should end
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <returns>True if a stop pattern is found</returns>
        public static bool ContainsStopPattern(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            foreach (var pattern in StopPatterns)
            {
                if (text.Contains(pattern))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Cleans up a generated response by removing any text after stop patterns
        /// </summary>
        /// <param name="text">The raw generated text</param>
        /// <returns>Cleaned text without content after stop patterns</returns>
        public static string TrimAfterStopPatterns(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
                
            int earliestStopIndex = text.Length;
            
            foreach (var pattern in StopPatterns)
            {
                int index = text.IndexOf(pattern);
                if (index >= 0 && index < earliestStopIndex)
                    earliestStopIndex = index;
            }
            
            if (earliestStopIndex < text.Length)
                return text.Substring(0, earliestStopIndex).Trim();
                
            return text;
        }
    }
}