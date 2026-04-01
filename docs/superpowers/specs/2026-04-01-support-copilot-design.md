# AI-Powered Support Copilot Agent - Design Document

**Date:** April 1, 2026  
**Hackathon Duration:** 1 Day  
**Prep Time Available:** 2 Days  
**Team Size:** 3 (2 Technical, 1 Non-Technical)

---

## Problem Statement

Support teams waste time manually analyzing incidents, searching scattered runbooks, JIRA tickets, emails, and SharePoint docs. This risks SLA breaches and inconsistent resolutions.

## Solution

AI-powered agent that takes an incident ID and provides:
- Incident details with SLA tracking
- Impacted Control-M jobs identification
- Actionable resolution steps with validation commands
- Relevant historical tickets and documentation
- SLA-aware recommendations with confidence scores

---

## Architecture

**Approach:** Monolithic .NET Web API (fastest for hackathon)

```
┌─────────────┐
│  React UI   │ (incident ID input → results display)
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

**Flow:**
1. Fetch incident from JIRA (title, description, SLA, priority)
2. Search historical JIRA tickets (keyword + relevance)
3. Search SharePoint for runbooks/Control-M job docs
4. Send context to Claude API for analysis
5. Return structured resolution guidance with validation steps

---

## Tech Stack

**Backend:** .NET 8 Web API, HttpClient, Polly (retries), Serilog  
**Frontend:** React + Bootstrap (or vanilla JS)  
**APIs:** JIRA REST, Microsoft Graph (SharePoint), Claude API  
**Mode:** Toggle between Live (real APIs) and Mock (demo data)

---

## API Contract

**Request:**
```json
POST /api/incident/analyze
{ "incidentId": "INC-12345", "mode": "live" }
```

**Response:**
```json
{
  "incidentSummary": { "title": "...", "priority": "High", "slaRemainingMinutes": 45 },
  "impactedJobs": [{ "jobName": "ABC_DAILY_LOAD", "environment": "Production" }],
  "resolutionGuidance": {
    "summary": "Job failed due to missing file...",
    "steps": [{ "stepNumber": 1, "action": "Check file", "command": "ls -l ..." }],
    "slaWarning": "Resolution needed within 45 mins",
    "confidenceScore": 0.85
  },
  "knowledgeSources": [{ "type": "JIRA", "reference": "INC-11234", "relevance": 0.92 }]
}
```

---

## Team Roles

### **Backend Dev (Technical Member 1)**
**Prep Days:**
- Setup .NET 8 Web API project
- Implement JIRA, SharePoint, Claude services
- Build Agent Orchestrator
- Create mock implementations
- Test all API integrations

**Hackathon Day:**
- Wire components together
- Integration testing & bug fixes

### **Frontend Dev (Technical Member 2)**
**Prep Days:**
- Setup React project
- Build UI components (input form, results display, loading states)
- Implement mock API calls
- Style with Bootstrap

**Hackathon Day:**
- Connect UI to backend
- Polish UX based on real data

### **Product Owner (Non-Technical Member)**
**Prep Days:**
- Create 5 realistic incident scenarios (JSON)
- Collect sample runbooks and JIRA tickets
- Write demo script and presentation slides
- Test UI and provide feedback

**Hackathon Day:**
- Lead demo presentation
- Explain business problem and impact
- Handle Q&A

---

## Prerequisites Checklist

### **Before Hackathon:**
- [ ] **Claude API Key** (console.anthropic.com) - ~$10 credits
- [ ] **JIRA API credentials** (test with Postman)
- [ ] **SharePoint API access** (Microsoft Graph or REST)
- [ ] **.NET 8 SDK** installed (`dotnet --version`)
- [ ] **Node.js 18+** installed (`node --version`)
- [ ] **Git repository** setup with all members access

### **Mock Data Created:**
- [ ] 5 incident examples (incident-samples.json)
- [ ] Historical JIRA tickets (jira-historical.json)
- [ ] SharePoint runbook excerpts (sharepoint-docs.json)
- [ ] Pre-written Claude responses (expected-resolutions.json)

### **Night Before Hackathon:**
- [ ] Backend services working individually
- [ ] Frontend UI fully functional with mock data
- [ ] All code committed to Git
- [ ] Each member can run project locally

---

## Hackathon Day Timeline

**Morning (4 hours):**
- Connect frontend to backend
- End-to-end testing
- Bug fixes

**Afternoon (4 hours):**
- Polish UI/UX
- Refine Claude prompts
- Practice demo
- Record backup video
- Prepare presentation

---

## Success Criteria

✅ Demo shows incident ID → AI-powered resolution in < 30 seconds  
✅ Identifies impacted Control-M jobs from historical data  
✅ Provides step-by-step commands with validation  
✅ Shows SLA warnings and confidence scores  
✅ Toggle between live and mock mode works smoothly  
✅ Team can explain business value and technical approach

---

## Key Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| API integration fails on demo day | Mock mode with pre-built responses |
| Claude API slow or rate-limited | Cache responses, use mock mode |
| SharePoint authentication issues | Fallback to JIRA-only mode |
| Time runs out | Prioritize mock mode polish over live integrations |

---

## Post-Hackathon Enhancements

- Email integration for searching support correspondence
- Vector DB for semantic search at scale
- User authentication and role-based access
- Feedback loop to improve AI recommendations
- Microservices architecture for production
