using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LLama;
using LLama.Abstractions;
using LLama.Common;

namespace RelaxTimerApp
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private TimeSpan _time;
        private InteractiveExecutor? _executor;

        // Cache model/context to avoid reloading the gguf on each request (huge speed-up on CPU)
        private static readonly SemaphoreSlim _modelInitLock = new(1, 1);
        private static LLamaWeights? _cachedModel;
        private static LLamaContext? _cachedContext;
        private static InteractiveExecutor? _cachedExecutor;

        private const string ModelPath = "GPT-2-fine-tuned-mental-health.Q8_0.gguf";
        private const string SoundFileName = "pianosound.wav";
        private MediaPlayer? _mediaPlayer;
        private bool _isSoundPaused = false;
        private bool _affirmationGenerated = false;


        public MainWindow()
        {
            InitializeComponent();

            // Load saved language
            LocalizationHelper.Instance.CurrentLanguage = AppSettings.Instance.Language;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            _time = TimeSpan.FromMinutes(10);
            TimerText.Text = _time.ToString(@"mm\:ss");

            // Show agreement on first run
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AppSettings.Instance.AgreementAccepted)
            {
                // Force English for first-run agreement
                var previousLanguage = LocalizationHelper.Instance.CurrentLanguage;
                LocalizationHelper.Instance.CurrentLanguage = "en";

                var agreementWindow = new UserAgreementWindow(isFirstRun: true);
                agreementWindow.Owner = this;
                var result = agreementWindow.ShowDialog();

                if (result == true)
                {
                    // User accepted
                    AppSettings.Instance.AgreementAccepted = true;
                    AppSettings.Instance.Save();
                    
                    // Restore language
                    LocalizationHelper.Instance.CurrentLanguage = previousLanguage;
                }
                else
                {
                    // User declined - close application
                    Application.Current.Shutdown();
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_isInfiniteMode)
            {
                // In infinite mode, just keep the timer running
                return;
            }

            if (_time == TimeSpan.Zero)
            {
                // Don't stop the timer so we can keep playing sound
                //_timer.Stop();

                // Try playing sound with more robust approach
                try
                {
                    if (_mediaPlayer == null || _mediaPlayer.Position.TotalMilliseconds > 100) // Only restart sound if it's not already playing
                    {
                        StopSound(); // Make sure any previous sound is stopped
                        StartSound();

                        // Add notification that timer is complete
                        Dispatcher.Invoke(() => {
                            TimerText.Text = "00:00 ⏰";
                            TimerText.Foreground = Brushes.LightGreen;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Помилка звуку: {ex.Message}");
                    // Try alternative sound approach if the regular one fails
                    PlaySystemSound();
                }

                // Generate affirmation only once (now multilingual)
                if (!_affirmationGenerated)
                {
                    _ = EnsureTimerFinishedAffirmationAsync();
                }
            }
            else
            {
                _time = _time.Add(TimeSpan.FromSeconds(-1));

                // Update display format based on remaining time
                if (_time.TotalHours >= 1)
                    TimerText.Text = _time.ToString(@"hh\:mm\:ss");
                else
                    TimerText.Text = _time.ToString(@"mm\:ss");
            }
        }

        private void SetTimer20_Click(object sender, RoutedEventArgs e) => SetTimerAndStart(TimeSpan.FromMinutes(20));
        private void SetTimer30_Click(object sender, RoutedEventArgs e) => SetTimerAndStart(TimeSpan.FromMinutes(30));
        private void SetTimer40_Click(object sender, RoutedEventArgs e) => SetTimerAndStart(TimeSpan.FromMinutes(40));

        private void SetTimerCustom_Click(object sender, RoutedEventArgs e)
        {
            // Create custom timer input dialog
            var customTimerDialog = new Window
            {
                Title = LocalizationHelper.Instance["CustomTimerTitle"],
                Width = 340,
                Height = 270,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(0x10, 0x02, 0x3C), 0),
                        new GradientStop(Color.FromRgb(0x3D, 0x10, 0x33), 1)
                    }
                }
            };

            // Create dialog content
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Title
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Timer input
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Infinite checkbox
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Format hint
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

            var titleLabel = new TextBlock
            {
                Text = LocalizationHelper.Instance["SetTime"],
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var timerInput = new TextBox
            {
                Width = 200,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = 16,
                Text = "00:10:00", // Default format: hh:mm:ss
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(5)
            };

            // Add a checkbox for infinite mode
            var infiniteCheckBox = new CheckBox
            {
                Content = LocalizationHelper.Instance["InfiniteMode"],
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14
            };

            // Format hint text
            var formatHint = new TextBlock
            {
                Text = LocalizationHelper.Instance["FormatHint"],
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Create a horizontal stack panel for buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var okButton = new Button
            {
                Content = LocalizationHelper.Instance["OK"],
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };

            var cancelButton = new Button
            {
                Content = LocalizationHelper.Instance["Cancel"],
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(titleLabel, 0);
            Grid.SetRow(timerInput, 1);
            Grid.SetRow(infiniteCheckBox, 2);
            Grid.SetRow(formatHint, 3);
            Grid.SetRow(buttonPanel, 4);

            grid.Children.Add(titleLabel);
            grid.Children.Add(timerInput);
            grid.Children.Add(infiniteCheckBox);
            grid.Children.Add(formatHint);
            grid.Children.Add(buttonPanel);

            customTimerDialog.Content = grid;

            // Toggle timer input based on infinite checkbox
            infiniteCheckBox.Checked += (s, args) => { timerInput.IsEnabled = false; };
            infiniteCheckBox.Unchecked += (s, args) => { timerInput.IsEnabled = true; };

            // Add button functionality
            okButton.Click += (s, args) =>
            {
                if (infiniteCheckBox.IsChecked == true)
                {
                    // Set timer to a very large value for "infinite" mode (24 hours)
                    SetTimerAndStart(TimeSpan.FromHours(24), isInfiniteMode: true);
                    customTimerDialog.Close();
                    return;
                }

                // Try parsing the time in format hh:mm:ss
                if (TimeSpan.TryParse(timerInput.Text, out TimeSpan customTime) && 
                    customTime > TimeSpan.Zero && 
                    customTime <= TimeSpan.FromHours(10)) // Limit to 10 hours
                {
                    SetTimerAndStart(customTime);
                    customTimerDialog.Close();
                }
                else
                {
                    MessageBox.Show(LocalizationHelper.Instance["InvalidFormatMessage"], 
                                  LocalizationHelper.Instance["InvalidFormatTitle"],
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            cancelButton.Click += (s, args) => { customTimerDialog.Close(); };

            // Make Enter key press the OK button
            timerInput.KeyDown += (s, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    okButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            };

            // Focus the text input when dialog opens
            customTimerDialog.Loaded += (s, args) => { timerInput.Focus(); timerInput.SelectAll(); };

            // Show dialog
            customTimerDialog.ShowDialog();
        }

        private bool _isInfiniteMode = false;

        private void SetTimerAndStart(TimeSpan time, bool isInfiniteMode = false)
        {
            _timer.Stop();
            StopSound();
            _time = time;
            _isInfiniteMode = isInfiniteMode;
            _affirmationGenerated = false; // Reset affirmation flag
            TimerText.Foreground = Brushes.White; // Reset text color

            // Update display format based on time length
            if (time.TotalHours >= 1)
                TimerText.Text = _time.ToString(@"hh\:mm\:ss");
            else
                TimerText.Text = _time.ToString(@"mm\:ss");

            if (_isInfiniteMode)
                TimerText.Text = "∞";

            ChatStackPanel.Children.Clear();
            _timer.Start();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) { _timer.Start(); ResumeSound(); }
        private void PauseButton_Click(object sender, RoutedEventArgs e) 
        { 
            _timer.Stop(); 

            // Always stop sound completely if at timer end
            if (_time == TimeSpan.Zero)
            {
                StopSound();
                // Reset timer to allow starting a new session
                ResetButton_Click(sender, e);
            }
            else
            {
                PauseSound(); 
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            StopSound();
            _isInfiniteMode = false; // Make sure to reset infinite mode
            _affirmationGenerated = false; // Reset affirmation flag
            _time = TimeSpan.FromMinutes(10);
            TimerText.Text = _time.ToString(@"mm\:ss");
            TimerText.Foreground = Brushes.White; // Reset text color
            ChatStackPanel.Children.Clear();
        }

        private void StartSound()
        {
            try
            {
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.Volume = 1.0; // Ensure volume is at maximum

                // Try multiple paths to find the sound file
                string parentDir = Path.Combine("..", SoundFileName);
                string grandparentDir = Path.Combine("..", "..", SoundFileName);
                string executableDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SoundFileName);
                string currentDir = SoundFileName;

                string[] possiblePaths = new string[]
                {
                    currentDir,                             // Current directory
                    executableDir,                         // Executable directory
                    parentDir,                             // Parent directory
                    grandparentDir                         // Grandparent directory
                };

                bool soundLoaded = false;

                // First try to load as application resource
                try
                {
                    var resourceUri = new Uri($"pack://application:,,,/{SoundFileName}", UriKind.Absolute);
                    _mediaPlayer.Open(resourceUri);
                    soundLoaded = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Не вдалося завантажити звук як ресурс: {ex.Message}");

                    // Try all possible file paths
                    foreach (var path in possiblePaths)
                    {
                        try
                        {
                            if (File.Exists(path))
                            {
                                _mediaPlayer.Open(new Uri(Path.GetFullPath(path)));
                                soundLoaded = true;
                                Debug.WriteLine($"Звук завантажено з: {path}");
                                break;
                            }
                        }
                        catch (Exception pathEx)
                        {
                            Debug.WriteLine($"Помилка при спробі завантажити {path}: {pathEx.Message}");
                        }
                    }
                }

                // If main sound file isn't found, try alternative sound files
                if (!soundLoaded)
                {
                    Debug.WriteLine("Звуковий файл не знайдено в жодному з можливих місць. Спроба знайти альтернативні звуки.");

                    // Try to load alternative sound files (Windows standard sounds)
                    string[] alternativeSoundFiles = new string[] {
                        "Windows Notify.wav",
                        "notify.wav",
                        "Windows Exclamation.wav",
                        "tada.wav"
                    };

                    foreach (string altSound in alternativeSoundFiles)
                    {
                        try {
                            string windowsSoundPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media", altSound);
                            if (File.Exists(windowsSoundPath))
                            {
                                _mediaPlayer.Open(new Uri(windowsSoundPath));
                                soundLoaded = true;
                                Debug.WriteLine($"Завантажено альтернативний звук: {windowsSoundPath}");
                                break;
                            }
                        }
                        catch (Exception ex) {
                            Debug.WriteLine($"Помилка при завантаженні альтернативного звуку {altSound}: {ex.Message}");
                        }
                    }

                    if (!soundLoaded)
                    {
                        Debug.WriteLine("Не знайдено жодного звукового файлу. Використання системного звуку.");
                        PlaySystemSound(); // Fall back to system sound
                        return;
                    }
                }

                // Set up looping and handle errors
                _mediaPlayer.MediaEnded += (s, e) =>
                {
                    try
                    {
                        // Only loop if we're at zero time (timer completed)
                        if (_time == TimeSpan.Zero && !_isSoundPaused)
                        {
                            _mediaPlayer.Position = TimeSpan.Zero;
                            _mediaPlayer.Play();
                            Debug.WriteLine("Restarting sound loop");
                        }
                    }
                    catch (Exception endEx)
                    {
                        Debug.WriteLine($"Помилка при повторному відтворенні: {endEx.Message}");
                        PlaySystemSound();
                    }
                };

                _mediaPlayer.MediaFailed += (s, e) =>
                {
                    Debug.WriteLine($"Помилка відтворення: {e.ErrorException?.Message}");
                    PlaySystemSound();
                };

                _mediaPlayer.Play();
                _isSoundPaused = false;

                // Log sound file diagnostic information
                Debug.WriteLine("========== SOUND DIAGNOSTIC INFO ===========");
                Debug.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
                Debug.WriteLine($"Base directory: {AppDomain.CurrentDomain.BaseDirectory}");
                Debug.WriteLine($"Sound file loaded: {soundLoaded}");
                Debug.WriteLine($"Detected sound files:");
                foreach (var path in possiblePaths)
                {
                    Debug.WriteLine($"- {path}: {(File.Exists(path) ? "EXISTS" : "NOT FOUND")}");
                }
                Debug.WriteLine("============================================");

                // Ensure sound is playing by checking status after a delay
                Task.Delay(500).ContinueWith(_ =>
                {
                    if (_mediaPlayer != null && _mediaPlayer.Position == TimeSpan.Zero)
                    {
                        Debug.WriteLine("Sound might not be playing after 500ms check - falling back to system sound");
                        PlaySystemSound();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Звук не відтворено: " + ex.Message);
                PlaySystemSound();
            }
        }

        private void PauseSound()
        {
            if (_mediaPlayer != null && !_isSoundPaused)
            {
                _mediaPlayer.Pause();
                _isSoundPaused = true;
            }
        }

        private void ResumeSound()
        {
            if (_mediaPlayer != null && _isSoundPaused)
            {
                _mediaPlayer.Play();
                _isSoundPaused = false;
            }
        }

        private void StopSound()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Close();
                _mediaPlayer = null;
                _isSoundPaused = false;
            }
        }

        /// <summary>
        /// Fallback method to play a system sound if the WAV file doesn't work
        /// </summary>
        private void PlaySystemSound()
        {
            try
            {
                // Try to play a piano sound file directly using SoundPlayer instead of MediaPlayer
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pianosound.wav");
                if (File.Exists(soundPath))
                {
                    // Create a new thread for continuous sound playing
                    Task.Run(() => {
                        try
                        {
                            using (var player = new System.Media.SoundPlayer(soundPath))
                            {
                                player.PlayLooping(); // Play in a loop

                                // Keep the sound playing until timer is reset or paused
                                while (_time == TimeSpan.Zero && !_isSoundPaused)
                                {
                                    Thread.Sleep(100);
                                }

                                player.Stop();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in sound player thread: {ex.Message}");
                        }
                    });
                }
                else
                {
                    // Fallback to system sounds
                    System.Media.SystemSounds.Asterisk.Play();

                    // Play multiple times with delays to ensure it's noticeable
                    Task.Run(async () => 
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (_time != TimeSpan.Zero || _isSoundPaused) break;
                            await Task.Delay(500);
                            await Dispatcher.InvokeAsync(() => System.Media.SystemSounds.Asterisk.Play());
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка відтворення системного звуку: {ex.Message}");
            }
        }

        private async Task EnsureModelLoadedAsync()
        {
            if (_cachedExecutor != null)
                return;

            await _modelInitLock.WaitAsync();
            try
            {
                if (_cachedExecutor != null)
                    return;

                var modelParams = LLamaModelConfig.CreateModelParams(ModelPath);
                _cachedModel = LLamaWeights.LoadFromFile(modelParams);
                _cachedContext = _cachedModel.CreateContext(modelParams);
                _cachedExecutor = new InteractiveExecutor(_cachedContext);
            }
            finally
            {
                _modelInitLock.Release();
            }
        }

        private static bool LooksLikeEnglish(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;

            int letters = 0;
            int nonAsciiLetters = 0;
            foreach (var ch in text)
            {
                if (char.IsLetter(ch))
                {
                    letters++;
                    if (ch > 127) nonAsciiLetters++;
                }
            }

            // If there are no letters, treat as English-ish (numbers/emojis/etc.)
            if (letters == 0) return true;

            // If lots of non-ASCII letters (Cyrillic etc.), it's probably not English.
            return nonAsciiLetters <= Math.Max(1, letters / 10);
        }


        private async Task EnsureTimerFinishedAffirmationAsync()
        {
            if (_affirmationGenerated) return;
            _affirmationGenerated = true;

            // Generate affirmation
            string affirmation;
            try
            {
                affirmation = await GenerateEnglishCompletionAsync("Write a short supportive affirmation for someone finishing a relaxation timer. Keep it 1-2 sentences." , maxTokens: 140);
                affirmation = EnsureMinimumOneTwoSentences(affirmation, "en");
            }
            catch
            {
                affirmation = EnsureMinimumOneTwoSentences(string.Empty, "en");
            }

            AddMessageToChat(affirmation, isUser: false);
        }

        private async Task<string> GenerateEnglishCompletionAsync(string englishUserText, int maxTokens)
        {
            await EnsureModelLoadedAsync();
            _executor = _cachedExecutor;

            var prompt =
                $"{Prompts.SystemPrompt}\n\n" +
                $"{Prompts.UserPrefix}{englishUserText}\n" +
                $"{Prompts.AssistantPrefix}";

            var sb = new StringBuilder();

            var inferenceParams = InferenceOptions.GetInferenceParams(
                maxTokens: maxTokens,
                temperature: 0.85f,
                topP: 0.92f,
                topK: 60,
                repeatPenalty: 1.16f,
                repeatLastN: 256);

            await foreach (var token in _executor!.InferAsync(prompt, inferenceParams))
            {
                sb.Append(token);
                var currentText = sb.ToString();
                if (ModelExtensions.ContainsStopPattern(currentText) || HasRepeatedSequence(currentText, 10))
                    break;
            }

            var rawResponse = sb.ToString();
            var trimmedResponse = ModelExtensions.TrimAfterStopPatterns(rawResponse);
            return CleanText(trimmedResponse);
        }

        private async Task RunLocalAI(string userInput)
        {
            try
            {
                // Show user message as typed.
                AddMessageToChat($"{LocalizationHelper.Instance["User"]}: {userInput}", isUser: true);

                AddMessageToChat($"{LocalizationHelper.Instance["AI"]}: ", isUser: false, isPartial: true);

                // Send input directly to AI
                var response = await GenerateEnglishCompletionAsync(userInput, maxTokens: 240);

                // Guarantee at least 1–2 sentences
                response = EnsureMinimumOneTwoSentences(response, "en");

                UpdatePartialMessageInChat(response);
            }
            catch
            {
                // Last resort fallback
                UpdatePartialMessageInChat(GetFallbackSupportiveReply("en"));
            }
        }

        private static bool IsSupportedUiLanguage(string lang) => lang is "en" or "uk" or "tr" or "ru" or "de";

        private static string GetLanguageLabel(string lang) => lang switch
        {
            "en" => "English",
            "uk" => "Українська",
            "tr" => "Türkçe",
            "ru" => "Русский",
            "de" => "Deutsch",
            _ => lang
        };

        private static string GetFallbackSupportiveReply(string lang) => lang switch
        {
            "uk" => "Я поруч. Зараз зроби один маленький крок: зроби повільний вдих і видих. Якщо хочеш, скажи одним реченням, що саме тебе турбує.",
            "ru" => "Я рядом. Сделай один маленький шаг: медленный вдох и выдох. Если хочешь, скажи одной фразой, что именно тебя тревожит.",
            "tr" => "Yanındayım. Şimdi küçük bir adım atalım: yavaşça nefes al ve ver. İstersen, seni en çok neyin zorladığını bir cümleyle söyle.",
            "de" => "Ich bin da. Mach einen kleinen Schritt: langsam einatmen und ausatmen. Wenn du magst, sag in einem Satz, was dich gerade am meisten belastet.",
            _ => "I'm here with you. Take one small step: breathe in slowly and breathe out. If you want, tell me in one sentence what's bothering you most right now."
        };

        private static bool HasAtLeastTwoSentences(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            int count = 0;
            foreach (var ch in text)
            {
                if (ch is '.' or '!' or '?') count++;
                if (count >= 2) return true;
            }
            return false;
        }

        private static string EnsureMinimumOneTwoSentences(string text, string lang)
        {
            var cleaned = (text ?? string.Empty).Trim();
            if (cleaned.Length < 10)
                return GetFallbackSupportiveReply(lang);

            // If we have no sentence-ending punctuation, add gentle follow-up.
            if (!cleaned.Contains('.') && !cleaned.Contains('!') && !cleaned.Contains('?'))
            {
                var followUp = lang switch
                {
                    "uk" => " Якщо хочеш, опиши це одним реченням.",
                    "ru" => " Если хочешь, опиши это одним предложением.",
                    "tr" => " İstersen bunu tek bir cümleyle anlat.",
                    "de" => " Wenn du magst, beschreib es in einem Satz.",
                    _ => " If you want, describe it in one sentence."
                };
                return cleaned + "." + followUp;
            }

            // If only one sentence, append a short empathetic question.
            if (!HasAtLeastTwoSentences(cleaned))
            {
                var followUp = lang switch
                {
                    "uk" => " Що зараз було б для тебе найменш важким наступним кроком?",
                    "ru" => " Что сейчас было бы для тебя самым простым следующим шагом?",
                    "tr" => " Şu an senin için en kolay sonraki adım ne olurdu?",
                    "de" => " Was wäre jetzt der leichteste nächste Schritt für dich?",
                    _ => " What would be the easiest next step for you right now?"
                };
                return cleaned.TrimEnd() + " " + followUp;
            }

            return cleaned;
        }

        private bool IsSpam(string text)
        {
            // Simple keyword-based spam detection
            string[] spamIndicators = new[]
            {
                "http", "facebook", "reddit", "references", "related", "source", "google", "linkedin", "share",
                "click", "visit", "subscribe", "download", "free", "trial", "www.", ".com", ".org", ".net", "url",
                "website", "channel", "follow", "please like", "please subscribe"
            };
            
            string loweredText = text.ToLowerInvariant();
            foreach (var indicator in spamIndicators)
            {
                if (loweredText.Contains(indicator, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            // Check for repetitive patterns that indicate unstable output
            if (HasExcessiveRepetition(text))
                return true;
                
            return false;
        }
        
        private bool HasExcessiveRepetition(string text)
        {
            // Check for the same sentence repeating
            string[] sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length >= 3)
            {
                for (int i = 0; i < sentences.Length - 1; i++)
                {
                    string trimmed1 = sentences[i].Trim();
                    if (trimmed1.Length < 5) continue;
                    
                    int similarSentences = 0;
                    for (int j = i + 1; j < sentences.Length; j++)
                    {
                        string trimmed2 = sentences[j].Trim();
                        if (trimmed1 == trimmed2 || 
                            (trimmed1.Length > 10 && trimmed2.Contains(trimmed1.Substring(0, trimmed1.Length / 2))))
                        {
                            similarSentences++;
                        }
                    }
                    
                    if (similarSentences >= 2) return true;
                }
            }
            
            // Check for word repetition
            string[] words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 10)
            {
                var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var word in words)
                {
                    string cleanWord = word.Trim(new[] { '.', ',', ';', ':', '!', '?', '\"', '\'', '(', ')' });
                    if (cleanWord.Length <= 2) continue;
                    
                    if (!wordCounts.ContainsKey(cleanWord))
                        wordCounts[cleanWord] = 1;
                    else
                        wordCounts[cleanWord]++;
                }
                
                // If any word appears more than 30% of total words and at least 3 times, consider it repetitive
                foreach (var count in wordCounts.Values)
                {
                    if (count >= 3 && count > words.Length * 0.3)
                        return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Detects if the text contains repeated character sequences that indicate unstable output
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <param name="minSequenceLength">Minimum length of sequence to check for repetition</param>
        /// <returns>True if repeated sequences are found</returns>
        private bool HasRepeatedSequence(string text, int minSequenceLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length < minSequenceLength * 2)
                return false;
                
            // Check for repeating character patterns (like "ababababab")
            for (int seqLength = minSequenceLength; seqLength <= Math.Min(text.Length / 2, 20); seqLength++)
            {
                for (int i = 0; i <= text.Length - seqLength * 2; i++)
                {
                    string seq1 = text.Substring(i, seqLength);
                    string seq2 = text.Substring(i + seqLength, seqLength);
                    
                    if (seq1 == seq2)
                        return true;
                }
            }
            
            // Check for repetitive line patterns
            string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length >= 3)
            {
                int repetitionCount = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Trim() == lines[i-1].Trim() && !string.IsNullOrWhiteSpace(lines[i]))
                        repetitionCount++;
                }
                
                if (repetitionCount >= 2)
                    return true;
            }
            
            return false;
        }

        private void AddMessageToChat(string message, bool isUser, bool isPartial = false)
        {
            Dispatcher.Invoke(() =>
            {
                // Create the chat bubble using the helper class
                var chatBubble = ChatMessageHelper.CreateChatBubble(message, isUser, isPartial);

                // Add to the chat panel
                ChatStackPanel.Children.Insert(0, chatBubble);
            });
        }

        private void UpdatePartialMessageInChat(string newText)
        {
            Dispatcher.Invoke(() =>
            {
                if (ChatStackPanel.Children.Count > 0)
                {
                    // First try with ChatMessageHelper
                    if (ChatMessageHelper.UpdatePartialMessage(ChatStackPanel, newText))
                        return;

                    // Fall back to direct approach if helper doesn't work
                    if (ChatStackPanel.Children[0] is Border border && border.Name == "PartialAIResponseContainer")
                    {
                        if (border.Child is TextBlock textBlock)
                        {
                            textBlock.Text = newText;
                            return;
                        }
                    }

                    // If all else fails, create a new message
                    AddMessageToChat(newText, false, true);
                }
                else
                {
                    // If no messages exist yet, create a new one
                    AddMessageToChat(newText, false, true);
                }
            });
        }

        private string CleanText(string text)
        {
            string[] bannedWords = new[]
            {
                "Twitter", "Facebook", "Reddit", "Tumblr", "Google", "LinkedIn",
                "Pinterest", "Pocket", "Telegram", "WhatsApp", "Skype", "SHARE", "Related", "http", "https"
            };

            foreach (var word in bannedWords)
                text = text.Replace(word, "", StringComparison.OrdinalIgnoreCase);

            var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();
            string lastWord = "";

            foreach (var word in words)
            {
                string clean = word.Trim('.', ',', '!', '?', ':', ';');
                if (clean.Length < 2) continue;
                if (string.Equals(clean, lastWord, StringComparison.OrdinalIgnoreCase)) continue;
                result.Append(clean).Append(' ');
                lastWord = clean;
            }

            return result.ToString().Trim();
        }

        private void SendChat_Click(object sender, RoutedEventArgs e)
        {
            var input = UserInputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                UserInputTextBox.Text = "";
                _ = RunLocalAI(input);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendChat_Click(sender, e);
                e.Handled = true;
            }
        }

        private void OpenLanguageSelection_Click(object sender, RoutedEventArgs e)
        {
            var window = new LanguageSelectionWindow();
            window.Owner = this;
            window.ShowDialog();
            
            // Save language after selection
            AppSettings.Instance.Language = LocalizationHelper.Instance.CurrentLanguage;
            AppSettings.Instance.Save();
        }

        private void OpenUserAgreement_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserAgreementWindow(isFirstRun: false);
            window.Owner = this;
            window.ShowDialog();
        }

        private void OpenReportContent_Click(object sender, RoutedEventArgs e)
        {
            var window = new ReportContentWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}

