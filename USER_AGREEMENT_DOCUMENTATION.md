# User Agreement Documentation

## Overview
The User Agreement explains the purpose of the application, data storage practices, and the AI model used. Users must accept this agreement on first run.

## Content Sections

### 1. Application Purpose
**All Languages**: States that the application is designed to help users take time for themselves, relax, and breathe. Includes disclaimer that it's not a substitute for professional help.

### 2. Data Storage
**Key Points**:
- Only stored data: Selected interface language
- Storage location: Local device only (no cloud, no servers)
- File location: `%APPDATA%\VRelaxTimer\settings.json`
- No personal information collected
- No usage statistics or telemetry

**Purpose**: Transparency about minimal data collection

### 3. AI Model Information
**Key Points**:
- Model: GPT-2 (historical/legacy AI model)
- Purpose: Educational - to familiarize users with early local AI
- Historical significance: Important milestone in accessible AI
- Local execution: Runs entirely on user's device
- No internet connection required for AI features

**Purpose**: Inform users about the technology and its educational value

## Full Text by Language

### English
```
By using this application, you agree to take a moment for yourself, 
relax, and breathe. This tool is designed to assist with mental 
well-being but is not a substitute for professional help.

DATA STORAGE: The only data stored by this application is your 
selected interface language preference, saved locally on your device.

AI MODEL: This application uses GPT-2, a historical AI model, for 
educational purposes to familiarize users with the history and 
capabilities of early local AI models. GPT-2 represents an important 
milestone in the development of accessible, locally-run AI technology.
```

### Ukrainian (Українська)
```
Використовуючи цей додаток, ви погоджуєтесь приділити час собі, 
розслабитися і дихати. Цей інструмент створений для підтримки 
ментального благополуччя, але не замінює професійну допомогу.

ЗБЕРЕЖЕННЯ ДАНИХ: Єдині дані, що зберігаються цим додатком - це 
ваша поточна назва мови інтерфейсу, збережена локально на вашому 
пристрої.

МОДЕЛЬ ШІ: Цей додаток використовує GPT-2, історичну модель ШІ, 
з освітньою метою для ознайомлення користувачів з історією та 
можливостями ранніх локальних моделей ШІ. GPT-2 представляє 
важливу віху в розвитку доступних локальних технологій штучного 
інтелекту.
```

### German (Deutsch)
```
Mit der Nutzung dieser Anwendung verpflichten Sie sich, sich einen 
Moment Zeit für sich selbst zu nehmen, sich zu entspannen und zu 
atmen. Dieses Tool dient der Unterstützung des mentalen Wohlbefindens, 
ersetzt aber keine professionelle Hilfe.

DATENSPEICHERUNG: Die einzigen von dieser Anwendung gespeicherten 
Daten sind Ihre ausgewählte Schnittstellensprache, die lokal auf 
Ihrem Gerät gespeichert wird.

KI-MODELL: Diese Anwendung verwendet GPT-2, ein historisches 
KI-Modell, zu Bildungszwecken, um Benutzer mit der Geschichte und 
den Fähigkeiten früher lokaler KI-Modelle vertraut zu machen. GPT-2 
stellt einen wichtigen Meilenstein in der Entwicklung zugänglicher, 
lokal ausgeführter KI-Technologie dar.
```

### Turkish (Türkçe)
```
Bu uygulamayı kullanarak, kendinize bir an ayırmayı, rahatlamayı ve 
nefes almayı kabul ediyorsunuz. Bu araç zihinsel sağlığa yardımcı 
olmak için tasarlanmıştır ancak profesyonel yardımın yerine geçmez.

VERİ DEPOLAMA: Bu uygulama tarafından depolanan tek veri, cihazınızda 
yerel olarak kaydedilen seçili arayüz dili tercihinizdir.

YZ MODELİ: Bu uygulama, kullanıcıları erken dönem yerel YZ 
modellerinin tarihi ve yetenekleri ile tanıştırmak için eğitim 
amaçlı tarihi bir YZ modeli olan GPT-2 kullanır. GPT-2, erişilebilir, 
yerel olarak çalışan YZ teknolojisinin gelişiminde önemli bir 
kilometre taşını temsil eder.
```

### Russian (Русский)
```
Используя это приложение, вы соглашаетесь уделить время себе, 
расслабиться и дышать. Этот инструмент предназначен для поддержки 
психического благополучия, но не заменяет профессиональную помощь.

ХРАНЕНИЕ ДАННЫХ: Единственные данные, хранимые этим приложением - 
это выбранный вами язык интерфейса, сохраненный локально на вашем 
устройстве.

МОДЕЛЬ ИИ: Это приложение использует GPT-2, историческую модель ИИ, 
в образовательных целях для ознакомления пользователей с историей 
и возможностями ранних локальных моделей ИИ. GPT-2 представляет 
важную веху в развитии доступных локально работающих технологий 
искусственного интеллекта.
```

## Privacy Highlights

### What We Store
✅ Language preference (e.g., "en", "uk", "de")
✅ Agreement acceptance status (true/false)

### What We DON'T Store
❌ Personal information (name, email, etc.)
❌ Chat history or conversations
❌ Usage statistics
❌ Device information
❌ IP address or location
❌ Timestamps of usage
❌ Any cloud or server data

## Technical Details

### Storage Format
```json
{
  "Language": "uk",
  "AgreementAccepted": true
}
```

### File Location
- Windows: `C:\Users\[Username]\AppData\Roaming\VRelaxTimer\settings.json`
- Size: ~50 bytes
- Format: JSON (plain text)

### GPT-2 Model
- **File**: `GPT-2-fine-tuned-mental-health.Q8_0.gguf`
- **Size**: Varies by quantization
- **Execution**: Local CPU/GPU (no internet required)
- **Privacy**: All processing happens locally
- **Purpose**: Educational demonstration of local AI capabilities

## Educational Value

### Why GPT-2?
1. **Historical Importance**: One of the first publicly available large language models
2. **Accessibility**: Can run on consumer hardware without cloud services
3. **Privacy**: Demonstrates feasibility of local AI processing
4. **Learning**: Shows evolution of AI technology from 2019 to present
5. **Limitations**: Educational to understand current AI improvements

### Learning Objectives
- Understand early AI capabilities
- Experience local AI processing
- Appreciate privacy benefits of local models
- Compare with modern AI systems
- Historical context of AI development

## Compliance & Legal

### GDPR Compliance
- ✅ Minimal data collection
- ✅ Local storage only
- ✅ User control over data
- ✅ No third-party sharing
- ✅ Clear disclosure in agreement

### User Rights
- Right to know what data is stored
- Right to delete data (delete settings file)
- Right to export data (readable JSON)
- Right to decline (app closes if declined)

## Best Practices

### For Users
1. Read the agreement carefully
2. Understand data storage is minimal and local
3. Know that AI model is educational/historical
4. Accept only if comfortable with terms
5. Can delete settings file anytime to reset

### For Developers
1. Keep agreement text clear and concise
2. Update if data practices change
3. Ensure all translations are accurate
4. Test agreement display in all languages
5. Make agreement accessible on first run

## Future Considerations

### Potential Updates
- [ ] Version number in agreement
- [ ] Last updated date
- [ ] Option to view agreement history
- [ ] Export agreement as PDF
- [ ] Multi-version support for different models
- [ ] Detailed privacy policy link

