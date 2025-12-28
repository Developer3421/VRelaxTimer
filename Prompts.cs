namespace RelaxTimerApp
{
    internal static class Prompts
    {
        // System prompt applied to every request.
        // Keep it short and simple so it works even for small/fine-tuned models.
        internal const string SystemPrompt =
            "You are a calm, supportive mental health assistant.\n" +
            "You give short, clear, empathetic answers.\n" +
            "You never hallucinate facts.\n" +
            "You speak in simple language.";

        // A light wrapper to steer GPT-2 style text completion into an assistant-ish format.
        internal const string AssistantPrefix = "Assistant: ";
        internal const string UserPrefix = "User: ";
    }
}

