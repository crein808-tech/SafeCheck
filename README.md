# SafeCheck

A simple Windows app that helps non-technical users check suspicious emails, phone calls, files, and links. Built for parents and family members who need a "second pair of eyes" when something looks off.

## Features

| Feature | What it does | Service used |
|---------|-------------|--------------|
| **Check Email** | Paste a sender's email address to see if it's associated with scams or phishing | EmailRep.io + AI fallback |
| **Check Phone** | Enter a phone number (any format) to check if it's a known scam/spam caller | AI analysis + lookup sites |
| **Check File** | Pick a downloaded file to scan with 70+ antivirus engines | VirusTotal API (free, needs key) |
| **Check Link** | Paste a URL to check if the website is safe | VirusTotal + CheckPhish fallback |
| **Ask AI** | Chat with an AI assistant about online safety questions (remembers conversation context) | Google Gemini + Groq fallback |

Results are shown as a traffic light: green (safe), yellow (caution), orange (warning), red (danger) — with plain-English recommendations.

When the primary service has no data or is unavailable, the app automatically asks AI for analysis, shows a "Search Google" button, and provides direct links to external lookup sites (Nomorobo, WhoCalledMe, IPQS, URLVoid, etc.) so the user can cross-reference.

## Requirements

- Windows 10 or 11
- .NET 8 SDK (for development) — download from https://dotnet.microsoft.com/download/dotnet/8.0
- Internet connection (for API calls)

## Quick Start

### 1. Clone / download the project

```
cd C:\dev\SafeCheck
```

### 2. Run the app

```
dotnet run --project SafeCheck.App
```

The app window will open. Email and phone checks work immediately (no API keys needed). File scanning and AI chat require API keys — see below.

### 3. Build a standalone .exe (to give to parents)

```
dotnet publish SafeCheck.App -c Release
```

The single .exe will be at:
```
SafeCheck.App\bin\Release\net8.0-windows\win-x64\publish\SafeCheck.exe
```

Copy that one file to their computer. No .NET install needed — it's self-contained.

## API Key Setup

Two features require free API keys. You only need to do this once — keys are stored encrypted on the computer.

### VirusTotal (for file and link scanning)

1. Go to https://www.virustotal.com/gui/join-us
2. Create a free account (email + password)
3. After logging in, go to your profile icon (top-right) → **API Key**
4. Copy the key
5. In SafeCheck, go to **Settings** → paste into **VirusTotal API Key** → **Save**

Free limit: 500 scans per day, 4 per minute.

### Google Gemini (for AI chat)

1. Go to https://aistudio.google.com/apikey
2. Sign in with a Google account
3. Click **Create API Key** → select any project → **Create**
4. Copy the key
5. In SafeCheck, go to **Settings** → paste into **Google Gemini API Key** → **Save**

Free limit: 1,000 questions per day.

### CheckPhish (optional, phishing detection for links)

Adds a dedicated phishing scanner as a fallback when VirusTotal has no data. Without this key, link checks still work via VirusTotal and AI.

1. Go to https://checkphish.bolster.ai/ and create a free account
2. Log in, click your profile icon (top-right) → **Profile Information**
3. Copy your API key
4. In SafeCheck, go to **Settings** → paste into **CheckPhish API Key** → **Save**

Free limit: 25 scans per day.

### Groq (optional backup AI)

Only needed if Gemini goes down. The app auto-switches to Groq when Gemini is unavailable.

1. Go to https://console.groq.com/keys
2. Create a free account
3. Click **Create API Key** → copy it
4. In SafeCheck, go to **Settings** → paste into **Groq API Key** → **Save**

## How It Works

- **Email check**: Queries EmailRep.io for the email's reputation score, whether it's blacklisted, linked to malware, etc. Falls back to AI analysis + lookup sites (IPQS, CleanTalk) when the API is unavailable or has no data.
- **Phone check**: AI-powered analysis with links to external lookup sites (Nomorobo, WhoCalledMe, Should I Answer, SpamCalls.net). The original SkipCalls API is no longer available, so the app relies on AI analysis and cross-referencing multiple lookup sites.
- **File check**: Computes the file's SHA256 hash → checks if VirusTotal already scanned it → if not, uploads the file for a fresh scan by 70+ antivirus engines. Falls back to AI analysis when VirusTotal is unavailable.
- **Link check**: Submits the URL to VirusTotal for domain reputation analysis. When VirusTotal has no data or is unavailable, falls back to CheckPhish (AI-powered phishing detection), then to AI analysis + lookup sites (CheckPhish, URLVoid, Google Safe Browsing, Sucuri SiteCheck).
- **AI chat**: Sends the question to Google Gemini with a system prompt tuned for simple, non-technical security advice. Remembers conversation context so follow-up questions work naturally. Tries multiple Gemini models (2.5-flash, 2.0-flash, 1.5-flash), then falls back to Groq (llama-3.3-70b, llama-3.1-8b, llama-4-scout) if Gemini is unavailable.
- **AI fallback**: When any check returns no data from its primary API, the app automatically asks Gemini for analysis, provides a "Search Google" button, and shows direct links to relevant external lookup sites.

API keys are stored encrypted using Windows DPAPI (Data Protection API) in `%LocalAppData%\SafeCheck\config.dat`. They never leave the computer in plain text.

The app uses saved keys from the encrypted config. No keys are included by default — each user must add their own via the Settings page.

## Project Structure

```
SafeCheck/
├── SafeCheck.sln
├── README.md
├── STATUS.md                  ← what's done, what's next
└── SafeCheck.App/
    ├── SafeCheck.App.csproj
    ├── App.xaml(.cs)
    ├── Models/                ← RiskLevel, ScanResult, LookupLink, AppConfig
    ├── Services/              ← API integrations (VirusTotal, EmailRep, etc.)
    ├── ViewModels/            ← MVVM view models for each page
    ├── Views/
    │   ├── MainWindow.xaml    ← navigation shell
    │   └── Pages/             ← HomePage, EmailCheckPage, etc.
    ├── Controls/              ← ResultCard (traffic light display)
    └── Converters/            ← Risk level → color/text converters
```

## Limitations

- Results are **not 100% accurate**. This is a second opinion, not a guarantee.
- VirusTotal free tier: 500 scans/day, 4/minute. For family use this is plenty.
- CheckPhish free tier: 25 scans/day. Used as a fallback for link checks only.
- Gemini free tier: 1,000 questions/day.
- Phone lookup relies on AI analysis and external lookup sites (the original SkipCalls API was discontinued).
- EmailRep.io may rate-limit requests — the app falls back to AI analysis when this happens.
- File uploads to VirusTotal are shared with security vendors (don't scan files containing sensitive personal data).
- External lookup site links open in the browser — their availability is outside this app's control.

What it looks like

<img width="448" height="672" alt="sf1" src="https://github.com/user-attachments/assets/c6579fa4-e8f0-43d4-ab21-da2e6d385391" />
<img width="453" height="373" alt="sf2" src="https://github.com/user-attachments/assets/0a1f6f00-8d34-4abf-8217-35db1cf6e47b" />
<img width="432" height="703" alt="sf4" src="https://github.com/user-attachments/assets/9975249a-27e9-41dd-832c-87e00ab6aed2" />
<img width="479" height="1024" alt="sf3" src="https://github.com/user-attachments/assets/3088335e-ae49-446c-822a-7331627691a9" />
