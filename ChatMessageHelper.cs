using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace RelaxTimerApp
{
    /// <summary>
    /// Helper class for creating and managing chat message UI elements
    /// </summary>
    public static class ChatMessageHelper
    {
        /// <summary>
        /// Creates a chat message bubble UI element
        /// </summary>
        /// <param name="message">Message text to display</param>
        /// <param name="isUser">True if this is a user message, false for AI</param>
        /// <param name="isPartial">True if this is a partial message (still being generated)</param>
        /// <returns>A Border element containing the message</returns>
        public static Border CreateChatBubble(string message, bool isUser, bool isPartial = false)
        {
            // Create text element
            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(12, 8, 12, 8),
                MaxWidth = 350,
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = isUser ? FontWeights.SemiBold : FontWeights.Normal,
                FontSize = 13
            };

            // Set name for partial response text blocks
            if (isPartial)
            {
                textBlock.Name = "PartialAIResponse";
            }

            // Create the chat bubble container
            var border = new Border
            {
                Child = textBlock,
                CornerRadius = isUser ? new CornerRadius(16, 16, 0, 16) : new CornerRadius(16, 16, 16, 0),
                Background = isUser ? new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x00)) : 
                                      new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)),
                Margin = isUser ? new Thickness(40, 4, 4, 4) : new Thickness(4, 4, 40, 4),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Effect = new DropShadowEffect
                {
                    ShadowDepth = 2,
                    BlurRadius = 5,
                    Opacity = 0.3
                }
            };

            // Set name for partial response containers
            if (isPartial)
            {
                border.Name = "PartialAIResponseContainer";
            }

            return border;
        }

        /// <summary>
        /// Updates the text in a partial response message
        /// </summary>
        /// <param name="container">The panel containing the chat messages</param>
        /// <param name="newText">The new text to display</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        public static bool UpdatePartialMessage(Panel container, string newText)
        {
            if (container.Children.Count == 0)
                return false;

            // First try finding a border with our partial response by name
            foreach (var child in container.Children)
            {
                if (child is Border border && border.Name == "PartialAIResponseContainer")
                {
                    // Get the text block inside
                    if (border.Child is TextBlock textBlock)
                    {
                        textBlock.Text = newText;
                        return true;
                    }
                }
            }

            // As a fallback, try the first child directly
            if (container.Children[0] is Border firstBorder)
            {
                if (firstBorder.Child is TextBlock firstTextBlock)
                {
                    firstTextBlock.Text = newText;
                    return true;
                }
            }

            return false;
        }
    }
}
