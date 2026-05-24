
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
