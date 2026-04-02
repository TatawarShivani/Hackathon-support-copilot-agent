# AWS Bedrock Setup Guide

The application uses **AWS Bedrock** (company account) instead of direct Anthropic API.

## Prerequisites

1. **AWS CLI** installed
   ```bash
   aws --version
   ```
   Download from: https://aws.amazon.com/cli/

2. **AWS SSO configured** (should already be done if you have company access)

---

## Authentication

Before running the application, authenticate with AWS SSO:

```bash
aws sso login --profile as24-bedrock-readonly
```

**Expected output:**
```
Attempting to automatically open the SSO authorization page in your default browser...
Successfully logged into Start URL: https://...
```

**Verify authentication:**
```bash
aws sts get-caller-identity --profile as24-bedrock-readonly
```

Should show your AWS account details.

---

## Configuration

The app is pre-configured with:
- **Profile:** `as24-bedrock-readonly`
- **Region:** `eu-west-1`
- **Model:** `eu.anthropic.claude-sonnet-4-5-20250929-v1:0`

These are set in `support-agent/appsettings.json` and can be overridden with environment variables:

```bash
# Windows PowerShell
$env:AWS_PROFILE="as24-bedrock-readonly"
$env:AWS_REGION="eu-west-1"

# Linux/Mac
export AWS_PROFILE=as24-bedrock-readonly
export AWS_REGION=eu-west-1
```

---

## Running the Application

```bash
# 1. Authenticate with AWS SSO
aws sso login --profile as24-bedrock-readonly

# 2. Run the application
cd support-agent
dotnet run

# 3. Open browser
# Navigate to: http://localhost:5097
```

---

## Troubleshooting

### Issue: "Unable to load AWS credentials"
**Solution:** Re-authenticate with AWS SSO
```bash
aws sso login --profile as24-bedrock-readonly
```

### Issue: "Access Denied" or "Not authorized to perform bedrock:InvokeModel"
**Solution:** Contact AWS admin to grant Bedrock access to your profile

### Issue: "Region not available"
**Solution:** Verify the region supports Bedrock:
```bash
aws bedrock list-foundation-models --region eu-west-1
```

### Issue: "Model not found"
**Solution:** Check available models:
```bash
aws bedrock list-foundation-models --region eu-west-1 --by-provider anthropic
```

---

## Cost

Using company AWS Bedrock account - costs are covered by company budget.  
No personal API keys or credit cards needed! ✅

---

## For Team Members

Make sure you have:
1. ✅ AWS CLI installed
2. ✅ Access to `as24-bedrock-readonly` profile
3. ✅ Authenticated with `aws sso login`

If you don't have access, contact your AWS admin or IT team.
