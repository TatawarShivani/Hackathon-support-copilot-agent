# Setup Guide for Team Members

## Prerequisites

Before running the application, ensure you have:

1. **.NET 8+ SDK** installed
   ```bash
   dotnet --version
   ```
   Download from: https://dotnet.microsoft.com/download

2. **Node.js 18+** (for testing scripts)
   ```bash
   node --version
   ```
   Download from: https://nodejs.org/

3. **Git** installed
   ```bash
   git --version
   ```

4. **Atlassian CLI (ACLI)** installed
   ```bash
   acli --version
   ```
   Download from: https://developer.atlassian.com/cloud/acli/guides/install-windows/

5. **AWS CLI** installed (for Bedrock/Claude AI)
   ```bash
   aws --version
   ```
   Download from: https://aws.amazon.com/cli/

---

## Step 1: Clone the Repository

```bash
git clone <repository-url>
cd ProductionSupportBOTHackathon
```

---

## Step 2: Authenticate with JIRA

```bash
# Login to JIRA using ACLI
acli jira auth login --via-api-token
```

You'll be prompted for:
- **JIRA URL**: Your company's JIRA instance (e.g., `https://yourcompany.atlassian.net`)
- **Email**: Your JIRA email
- **API Token**: Get from https://id.atlassian.com/manage-profile/security/api-tokens

**Test the connection:**
```bash
acli jira workitem view CMSRC-8930
```

You should see incident details if authentication is successful.

---

## Step 3: Authenticate with AWS Bedrock

```bash
# Login to AWS SSO (for Claude AI via Bedrock)
aws sso login --profile as24-bedrock-readonly
```

Expected: Browser opens, you authenticate, then see "Successfully logged in"

**Verify:**
```bash
aws sts get-caller-identity --profile as24-bedrock-readonly
```

See [AWS-BEDROCK-SETUP.md](AWS-BEDROCK-SETUP.md) for detailed instructions.

---

## Step 4: Run the Application

```bash
cd support-agent
dotnet run
```

Expected output:
```
Now listening on: http://localhost:5097
Application started. Press Ctrl+C to shut down.
```

---

## Step 5: Open the UI

Open your browser and navigate to:

**http://localhost:5097/**

---

## Testing the Application

1. Enter an incident ID (e.g., `CMSRC-8930`)
2. Click "🔍 Analyze Incident"
3. You should see the incident details fetched from JIRA

---

## Troubleshooting

### Issue: "acli: command not found"
- Ensure ACLI is installed and added to your PATH
- Restart your terminal after installation

### Issue: "JIRA authentication failed"
- Run: `acli jira auth status` to check authentication
- Re-authenticate: `acli jira auth login --via-api-token`

### Issue: ".NET SDK not found"
- Install .NET 8+ from https://dotnet.microsoft.com/download
- Verify: `dotnet --version`

### Issue: "Port 5097 already in use"
- Change the port in `support-agent/Properties/launchSettings.json`
- Update the port in `support-agent/wwwroot/index.html` (line with `fetch()`)

### Issue: "Incident not found"
- Verify you have access to the JIRA project
- Try a different incident ID that you know exists
- Check ACLI can access it: `acli jira workitem view YOUR-INCIDENT-ID`

---

## Development Workflow

### Backend Changes
1. Make changes to `.cs` files
2. Stop the app (Ctrl+C)
3. Rebuild: `dotnet build`
4. Run: `dotnet run`

### Frontend Changes
1. Edit `wwwroot/index.html`
2. Refresh browser (no restart needed)

### Commit Changes
```bash
git add .
git commit -m "your message"
git push
```

---

## Project Structure

```
ProductionSupportBOTHackathon/
├── connection-tests/          # API connection test scripts
├── docs/                      # Design and planning documents
└── support-agent/             # Main application
    ├── Controllers/           # API endpoints
    ├── Models/                # Data models
    ├── Services/              # Business logic (JIRA, Claude, etc.)
    └── wwwroot/               # Frontend UI
        └── index.html         # Main UI page
```

---

## Next Steps

Once the basic setup is working:

1. **Test Claude API** - Add Claude integration for AI-powered suggestions
2. **Add SharePoint** - Integrate Microsoft Graph API
3. **Enhance UI** - Add more features to the frontend
4. **Create Mock Data** - For demo mode

---

## Need Help?

- Check `connection-tests/README.md` for API testing
- Review `docs/superpowers/specs/2026-04-01-support-copilot-design.md` for architecture
- Ask the team in your project channel
