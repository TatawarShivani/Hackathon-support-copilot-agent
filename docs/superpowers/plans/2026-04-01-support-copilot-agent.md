# Support Copilot Agent Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build AI-powered support copilot that analyzes incidents and provides resolution guidance

**Architecture:** Monolithic .NET 8 Web API with Agent Orchestrator pattern, React UI, mock/live mode toggle

**Tech Stack:** .NET 8, C#, React, Bootstrap, JIRA REST API, Microsoft Graph API, Claude API

---

## File Structure

**Backend (support-agent/):**
- `Program.cs` - API startup, DI configuration
- `appsettings.json` - Configuration
- `Models/IncidentAnalysisRequest.cs` - API request model
- `Models/IncidentAnalysisResponse.cs` - API response model
- `Models/IncidentSummary.cs` - Incident details
- `Models/ImpactedJob.cs` - Control-M job info
- `Models/ResolutionGuidance.cs` - AI resolution steps
- `Models/KnowledgeSource.cs` - Reference to JIRA/SharePoint
- `Services/IAgentOrchestrator.cs` - Orchestrator interface
- `Services/AgentOrchestrator.cs` - Main coordination logic
- `Services/IJiraService.cs` - JIRA interface
- `Services/JiraService.cs` - JIRA API client
- `Services/MockJiraService.cs` - Mock implementation
- `Services/ISharePointService.cs` - SharePoint interface
- `Services/SharePointService.cs` - SharePoint API client
- `Services/MockSharePointService.cs` - Mock implementation
- `Services/IClaudeService.cs` - Claude interface
- `Services/ClaudeService.cs` - Claude API client
- `Services/MockClaudeService.cs` - Mock implementation
- `Controllers/IncidentController.cs` - API endpoints
- `MockData/incidents.json` - Sample incidents
- `MockData/jira-tickets.json` - Historical tickets
- `MockData/sharepoint-docs.json` - Runbook content

**Frontend (support-agent-ui/):**
- `src/App.js` - Main component
- `src/components/IncidentInput.js` - Input form
- `src/components/LoadingState.js` - Progress indicators
- `src/components/IncidentSummary.js` - Incident display
- `src/components/ImpactedJobs.js` - Jobs list
- `src/components/ResolutionSteps.js` - Steps accordion
- `src/components/KnowledgeSources.js` - References
- `src/components/FeedbackSection.js` - Thumbs up/down

---

### Task 1: Connection Testing - JIRA API

**Files:**
- Create: `connection-tests/test-jira.js`

- [ ] **Step 1: Create test script for JIRA connection**

```javascript
// connection-tests/test-jira.js
const https = require('https');

const JIRA_BASE_URL = process.env.JIRA_URL || 'https://yourcompany.atlassian.net';
const JIRA_EMAIL = process.env.JIRA_EMAIL;
const JIRA_API_TOKEN = process.env.JIRA_API_TOKEN;

const auth = Buffer.from(`${JIRA_EMAIL}:${JIRA_API_TOKEN}`).toString('base64');

const options = {
  hostname: new URL(JIRA_BASE_URL).hostname,
  path: '/rest/api/3/myself',
  method: 'GET',
  headers: {
    'Authorization': `Basic ${auth}`,
    'Accept': 'application/json'
  }
};

console.log('Testing JIRA connection...');
console.log(`URL: ${JIRA_BASE_URL}`);

const req = https.request(options, (res) => {
  let data = '';
  res.on('data', (chunk) => data += chunk);
  res.on('end', () => {
    if (res.statusCode === 200) {
      const user = JSON.parse(data);
      console.log('✅ JIRA connection successful!');
      console.log(`Authenticated as: ${user.displayName} (${user.emailAddress})`);
    } else {
      console.error('❌ JIRA connection failed');
      console.error(`Status: ${res.statusCode}`);
      console.error(`Response: ${data}`);
    }
  });
});

req.on('error', (e) => {
  console.error('❌ JIRA connection error:', e.message);
});

req.end();
```

- [ ] **Step 2: Create .env file for credentials**

Create `.env` file (add to .gitignore):
```bash
JIRA_URL=https://yourcompany.atlassian.net
JIRA_EMAIL=your-email@company.com
JIRA_API_TOKEN=your-api-token-here
```

- [ ] **Step 3: Run test**

```bash
node connection-tests/test-jira.js
```

Expected: "✅ JIRA connection successful!"

- [ ] **Step 4: Test ticket retrieval**

Add to `test-jira.js` after line 34:

```javascript
// Test fetching a specific issue
function testGetIssue() {
  const issueKey = process.env.TEST_JIRA_ISSUE || 'INC-1';
  
  const issueOptions = {
    hostname: new URL(JIRA_BASE_URL).hostname,
    path: `/rest/api/3/issue/${issueKey}`,
    method: 'GET',
    headers: {
      'Authorization': `Basic ${auth}`,
      'Accept': 'application/json'
    }
  };

  console.log('\nTesting issue retrieval...');
  
  const req = https.request(issueOptions, (res) => {
    let data = '';
    res.on('data', (chunk) => data += chunk);
    res.on('end', () => {
      if (res.statusCode === 200) {
        const issue = JSON.parse(data);
        console.log('✅ Issue retrieval successful!');
        console.log(`Issue: ${issue.key} - ${issue.fields.summary}`);
        console.log(`Status: ${issue.fields.status.name}`);
        console.log(`Priority: ${issue.fields.priority.name}`);
      } else {
        console.error('❌ Issue retrieval failed');
        console.error(`Status: ${res.statusCode}`);
      }
    });
  });

  req.on('error', (e) => console.error('❌ Error:', e.message));
  req.end();
}

// Call after successful auth
setTimeout(testGetIssue, 1000);
```

- [ ] **Step 5: Run full test**

```bash
TEST_JIRA_ISSUE=INC-12345 node connection-tests/test-jira.js
```

Expected: Both auth and issue retrieval succeed

- [ ] **Step 6: Commit**

```bash
git add connection-tests/test-jira.js .gitignore
git commit -m "test: add JIRA API connection test script"
```

---

### Task 2: Connection Testing - SharePoint API

**Files:**
- Create: `connection-tests/test-sharepoint.js`

- [ ] **Step 1: Create test script for SharePoint**

```javascript
// connection-tests/test-sharepoint.js
const https = require('https');

const TENANT_ID = process.env.AZURE_TENANT_ID;
const CLIENT_ID = process.env.AZURE_CLIENT_ID;
const CLIENT_SECRET = process.env.AZURE_CLIENT_SECRET;
const SHAREPOINT_SITE = process.env.SHAREPOINT_SITE_ID;

// Get access token
function getAccessToken() {
  return new Promise((resolve, reject) => {
    const tokenData = new URLSearchParams({
      client_id: CLIENT_ID,
      scope: 'https://graph.microsoft.com/.default',
      client_secret: CLIENT_SECRET,
      grant_type: 'client_credentials'
    }).toString();

    const options = {
      hostname: 'login.microsoftonline.com',
      path: `/${TENANT_ID}/oauth2/v2.0/token`,
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Content-Length': tokenData.length
      }
    };

    console.log('Getting Microsoft Graph access token...');
    
    const req = https.request(options, (res) => {
      let data = '';
      res.on('data', (chunk) => data += chunk);
      res.on('end', () => {
        if (res.statusCode === 200) {
          const token = JSON.parse(data);
          console.log('✅ Token acquired successfully');
          resolve(token.access_token);
        } else {
          console.error('❌ Token acquisition failed');
          console.error(`Status: ${res.statusCode}`);
          console.error(`Response: ${data}`);
          reject(new Error('Token acquisition failed'));
        }
      });
    });

    req.on('error', reject);
    req.write(tokenData);
    req.end();
  });
}

// Test SharePoint site access
function testSiteAccess(accessToken) {
  const options = {
    hostname: 'graph.microsoft.com',
    path: `/v1.0/sites/${SHAREPOINT_SITE}`,
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Accept': 'application/json'
    }
  };

  console.log('\nTesting SharePoint site access...');
  
  const req = https.request(options, (res) => {
    let data = '';
    res.on('data', (chunk) => data += chunk);
    res.on('end', () => {
      if (res.statusCode === 200) {
        const site = JSON.parse(data);
        console.log('✅ SharePoint access successful!');
        console.log(`Site: ${site.displayName}`);
        console.log(`URL: ${site.webUrl}`);
      } else {
        console.error('❌ SharePoint access failed');
        console.error(`Status: ${res.statusCode}`);
        console.error(`Response: ${data}`);
      }
    });
  });

  req.on('error', (e) => console.error('❌ Error:', e.message));
  req.end();
}

getAccessToken()
  .then(testSiteAccess)
  .catch(console.error);
```

- [ ] **Step 2: Add SharePoint credentials to .env**

```bash
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
SHAREPOINT_SITE_ID=your-site-id
```

- [ ] **Step 3: Run test**

```bash
node connection-tests/test-sharepoint.js
```

Expected: "✅ SharePoint access successful!"

- [ ] **Step 4: Commit**

```bash
git add connection-tests/test-sharepoint.js
git commit -m "test: add SharePoint/Graph API connection test"
```

---

### Task 3: Connection Testing - Claude API

**Files:**
- Create: `connection-tests/test-claude.js`

- [ ] **Step 1: Create test script for Claude API**

```javascript
// connection-tests/test-claude.js
const https = require('https');

const CLAUDE_API_KEY = process.env.CLAUDE_API_KEY;

const data = JSON.stringify({
  model: 'claude-3-5-sonnet-20241022',
  max_tokens: 100,
  messages: [{
    role: 'user',
    content: 'Respond with "Connection successful" if you receive this.'
  }]
});

const options = {
  hostname: 'api.anthropic.com',
  path: '/v1/messages',
  method: 'POST',
  headers: {
    'x-api-key': CLAUDE_API_KEY,
    'anthropic-version': '2023-06-01',
    'content-type': 'application/json',
    'Content-Length': data.length
  }
};

console.log('Testing Claude API connection...');

const req = https.request(options, (res) => {
  let responseData = '';
  res.on('data', (chunk) => responseData += chunk);
  res.on('end', () => {
    if (res.statusCode === 200) {
      const response = JSON.parse(responseData);
      console.log('✅ Claude API connection successful!');
      console.log(`Model: ${response.model}`);
      console.log(`Response: ${response.content[0].text}`);
      console.log(`Tokens used: ${response.usage.input_tokens} in, ${response.usage.output_tokens} out`);
    } else {
      console.error('❌ Claude API connection failed');
      console.error(`Status: ${res.statusCode}`);
      console.error(`Response: ${responseData}`);
    }
  });
});

req.on('error', (e) => {
  console.error('❌ Claude API error:', e.message);
});

req.write(data);
req.end();
```

- [ ] **Step 2: Add Claude API key to .env**

```bash
CLAUDE_API_KEY=sk-ant-your-key-here
```

- [ ] **Step 3: Run test**

```bash
node connection-tests/test-claude.js
```

Expected: "✅ Claude API connection successful!"

- [ ] **Step 4: Commit**

```bash
git add connection-tests/test-claude.js
git commit -m "test: add Claude API connection test"
```

---

### Task 4: .NET Project Setup

**Files:**
- Create: `support-agent/` directory with .NET Web API project

- [ ] **Step 1: Create .NET Web API project**

```bash
dotnet new webapi -n support-agent
cd support-agent
```

- [ ] **Step 2: Add required NuGet packages**

```bash
dotnet add package Microsoft.Graph
dotnet add package Azure.Identity
dotnet add package Polly
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Anthropic.SDK --version 0.3.0
```

- [ ] **Step 3: Verify project builds**

```bash
dotnet build
```

Expected: "Build succeeded"

- [ ] **Step 4: Create directory structure**

```bash
mkdir -p Models Services Controllers MockData
```

- [ ] **Step 5: Commit**

```bash
cd ..
git add support-agent/
git commit -m "feat: scaffold .NET Web API project with dependencies"
```

---

Plan continues with Models, Services, Controllers, Frontend, and Integration tasks. 

Save and commit this initial phase?
