# SafeCheck — Status Tracker

## Completed

- [x] Project setup (C# WPF, .NET 8, solution structure)
- [x] Main window with frame-based page navigation
- [x] Home page with 5 feature buttons + Settings
- [x] Email check — EmailRep.io integration (no auth needed)
- [x] Phone number check — SkipCalls API integration (no auth needed)
- [x] File scan — VirusTotal API v3 (hash lookup + file upload + polling)
- [x] Link/URL check — VirusTotal API v3 URL scanning
- [x] AI chat — Google Gemini integration with multi-model retry
- [x] AI fallback — Groq with multi-model retry (auto-switches when Gemini fails)
- [x] Settings page with PasswordBox fields for API keys
- [x] DPAPI-encrypted API key storage (Windows Credential Manager)
- [x] Hardcoded default API keys (parents get zero-setup experience)
- [x] ResultCard control with traffic-light risk display
- [x] Risk level converters (color, text, emoji)
- [x] MVVM architecture (ViewModels for each page)
- [x] AI fallback on no results — phone, email, and link checks auto-query Gemini when primary API has no data
- [x] "Search Google" button on result cards — opens browser with targeted scam search
- [x] Detailed error messages — AI errors now show actual failure reason instead of generic message
- [x] Multi-model retry — Gemini tries 3 models, Groq tries 3 models before giving up
- [x] Phone number accepts any format — (555) 123-4567, 555-123-4567, 5551234567, +15551234567
- [x] Clean build (0 errors, 0 warnings)
- [x] Single-file publish configuration (self-contained .exe)
- [x] AI chat conversation memory — follow-up questions retain context
- [x] Fixed Gemini model list (removed non-existent gemini-3.5-flash, replaced with gemini-1.5-flash)
- [x] AI fallback on ALL check types — email, phone, file, and link all fall back to AI when primary API fails
- [x] External lookup site buttons on result cards (Nomorobo, WhoCalledMe, IPQS, URLVoid, Sucuri, etc.)
- [x] Fixed EmailRep rate-limit handling (429 now triggers AI fallback instead of "no internet" error)
- [x] Fixed SkipCalls API (service discontinued — app now uses AI analysis + lookup sites)
- [x] Fixed CanAsk/CanScan button enable/disable reliability on all pages
- [x] Larger default window size (580x820) to prevent content cutoff
- [x] "Search Google" button now appears on all check types including file

## Not Yet Done

- [ ] App icon (currently uses default WPF icon)
- [ ] Scan history / log of past checks (SQLite cache)
- [ ] Full email body analysis (paste entire email, extract links, check each)
- [ ] "Paste from clipboard" button on input fields
- [ ] Loading spinner animation (currently just text)
- [ ] Enter key to submit on email/phone/link pages
- [ ] Auto-scroll chat in Ask AI page
- [ ] Keyboard shortcut hints
- [ ] Installer (MSIX or Inno Setup) — currently just a standalone .exe
- [ ] Auto-update check against GitHub releases
- [ ] Dark mode support
- [ ] Error logging to file (%AppData%\SafeCheck\logs\)

## Known Limitations

- Phone lookup relies on AI analysis + external sites (SkipCalls API was discontinued)
- EmailRep may rate-limit or not have data on brand-new email addresses — falls back to AI
- VirusTotal file upload has a ~650MB limit
- File scans can take 30-120 seconds for first-time uploads
- Gemini responses depend on Google's content filtering (may refuse some prompts)

## Free API Limits

| Service | Limit | Resets |
|---------|-------|--------|
| VirusTotal | 500 scans/day, 4/min | Daily at midnight UTC |
| EmailRep | Unlimited (fair use, may rate-limit) | — |
| Google Gemini | 1,000 req/day | Daily |
| Groq | 14,400 req/day | Daily |
