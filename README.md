# 🤖 AI-Powered Support Copilot Agent

An intelligent support agent that analyzes production incidents and provides actionable resolution guidance using AI.

## Quick Start

```bash
# 1. Install ACLI and authenticate with JIRA
acli jira auth login --via-api-token

# 2. Run the application
cd support-agent
dotnet run

# 3. Open browser
# Navigate to: http://localhost:5097
```

## What It Does

- 📋 Fetches incident details from JIRA
- 🔍 Searches historical tickets for similar issues
- 📚 Retrieves relevant runbooks from SharePoint
- 🤖 Uses AI (Claude) to suggest resolution steps
- ⏰ Provides SLA-aware recommendations
- ✅ Includes validation commands and confidence scores

## Documentation

- **[Setup Guide](SETUP.md)** - Complete setup instructions for team members
- **[Design Document](docs/superpowers/specs/2026-04-01-support-copilot-design.md)** - Architecture and requirements
- **[Connection Tests](connection-tests/README.md)** - API testing instructions

## Architecture

```
┌─────────────┐
│  React UI   │ ← You are here (http://localhost:5097)
└──────┬──────┘
       │ HTTP
┌──────▼──────────────────────────────┐
│    ASP.NET Core Web API             │
│  ┌──────────────────────────────┐   │
│  │  Agent Orchestrator Service  │   │
│  └─┬────────┬────────┬──────────┘   │
│    │        │        │               │
│  ┌─▼──┐  ┌─▼────┐ ┌─▼────────┐      │
│  │JIRA│  │Share │ │  Claude  │      │
│  │Svc │  │Point │ │  Service │      │
│  └────┘  │ Svc  │ └──────────┘      │
│          └──────┘                    │
└─────────────────────────────────────┘
```

## Tech Stack

- **Backend**: .NET 8 Web API, C#
- **Frontend**: HTML, JavaScript, Bootstrap
- **APIs**: JIRA (via ACLI), Microsoft Graph, Claude API
- **Tools**: Atlassian CLI, Git

## Team Roles

- **Backend Developer**: API services, JIRA/Claude integration
- **Frontend Developer**: UI components, user experience
- **Product Owner**: Demo scenarios, presentation, mock data

## Status

✅ JIRA connection working  
✅ Backend API built  
✅ UI created and functional  
🚧 Claude AI integration (next)  
🚧 SharePoint search (next)  
⏳ Mock mode for demo (pending)

## License

Internal hackathon project
