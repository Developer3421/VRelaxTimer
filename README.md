# VRelaxTimer

A lightweight Windows relaxation timer with an offline local AI text assistant and a multilingual UI.

## Features
- Relaxation timer with a simple countdown UI
- Offline / local AI text assistant (no cloud calls)
- Interface language selection (localized UI)
- User Agreement shown on first launch
  - Decline closes the app
  - After acceptance it won’t show at startup again (still accessible from the app)
- “Report AI content” window to write a complaint and save it to a text file

## Privacy
VRelaxTimer is designed to work offline.

The only data stored locally on your device is:
- the selected interface language
- whether the User Agreement was accepted

See: `PRIVACY_POLICY_EN.md`

## About the AI model
VRelaxTimer uses a local GPT‑2 model for educational and historical exploration of early local AI language models and their capabilities in the past.

## Requirements
- Windows (desktop)
- .NET (project targets `net9.0-windows`)

## Build
Open `VRelaxTimer.sln` in Visual Studio or JetBrains Rider and build/run the WPF app.

## License
See `LICENSE.txt`

