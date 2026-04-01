# API Connection Testing

Test all external API connections before the hackathon.

## Option 1: JIRA via Atlassian CLI (ACLI) - Recommended

**Install ACLI:**
```bash
# Download from: https://developer.atlassian.com/cloud/acli/guides/install-windows/
# After installation, verify:
acli --version
```

**Add AS24 Jira Skill (if using Claude Code):**
```bash
claude plugin marketplace add AutoScout24/claude-code-skills
claude plugin install as24-jira-acli
```

**Authenticate with Jira:**
```bash
# Option 1: API Token
acli jira auth login --via-api-token

# Option 2: Web Browser
acli jira auth login --via-web
```

**Test Connection:**
```bash
# Get current user info
acli jira user me

# Get a specific issue
acli jira issue get INC-12345

# Search for incidents
acli jira search "project = SUPPORT AND status = Open"
```

**Advantages:**
- ✅ Simpler authentication flow
- ✅ Built-in retry and error handling
- ✅ Can be used in .NET code via Process.Start
- ✅ Easier for team members to set up

---

## Option 2: Direct JIRA REST API (Node.js)

**Setup:**
```bash
# Copy template and add credentials
cp .env.template .env
# Edit .env with your JIRA_URL, JIRA_EMAIL, JIRA_API_TOKEN
```

**Run Test:**
```bash
node connection-tests/test-jira.js
```

**Get API Token:**
1. Go to https://id.atlassian.com/manage-profile/security/api-tokens
2. Click "Create API token"
3. Copy token to .env file

---

## SharePoint via Microsoft Graph API

**Setup:**
1. Register an Azure AD app at https://portal.azure.com
2. Grant permissions: Sites.Read.All
3. Create client secret
4. Add credentials to .env

**Run Test:**
```bash
node connection-tests/test-sharepoint.js
```

---

## Claude API

**Setup:**
1. Get API key from https://console.anthropic.com
2. Add CLAUDE_API_KEY to .env

**Run Test:**
```bash
node connection-tests/test-claude.js
```

---

## Recommendation for Hackathon

**Use ACLI for JIRA** - Much faster setup, less authentication hassle
**Use Graph API for SharePoint** - Standard Microsoft approach
**Use direct API for Claude** - Simple HTTP calls
