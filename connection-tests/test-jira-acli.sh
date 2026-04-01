#!/bin/bash
# Test JIRA connection using Atlassian CLI

echo "Testing JIRA connection via ACLI..."

# Check if ACLI is installed
if ! command -v acli &> /dev/null; then
    echo "❌ ACLI not found. Install from:"
    echo "https://developer.atlassian.com/cloud/acli/guides/install-windows/"
    exit 1
fi

echo "✅ ACLI installed: $(acli --version)"

# Test authentication by checking auth status
echo -e "\nTesting authentication..."
if acli jira auth status &> /dev/null; then
    echo "✅ JIRA authentication successful!"
    acli jira auth status
else
    echo "❌ JIRA authentication failed. Run:"
    echo "acli jira auth login --via-api-token"
    exit 1
fi

# Test issue retrieval
echo -e "\nTesting issue retrieval..."
TEST_ISSUE=${TEST_JIRA_ISSUE:-"CMSRC-8930"}
if acli jira workitem view "$TEST_ISSUE" &> /dev/null; then
    echo "✅ Issue retrieval successful!"
    acli jira workitem view "$TEST_ISSUE"
else
    echo "⚠️  Issue $TEST_ISSUE not found (this is OK if it doesn't exist)"
fi

# Test search
echo -e "\nTesting JIRA search..."
if acli jira workitem search "order by created DESC" --limit 3 &> /dev/null; then
    echo "✅ JIRA search working!"
    acli jira workitem search "order by created DESC" --limit 3
else
    echo "❌ JIRA search failed"
    exit 1
fi

echo -e "\n✅ All ACLI JIRA tests passed!"
