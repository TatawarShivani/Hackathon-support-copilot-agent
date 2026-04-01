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

# Test authentication by getting current user
echo -e "\nTesting authentication..."
if acli jira user me &> /dev/null; then
    echo "✅ JIRA authentication successful!"
    acli jira user me
else
    echo "❌ JIRA authentication failed. Run:"
    echo "acli jira auth login --via-api-token"
    exit 1
fi

# Test issue retrieval
echo -e "\nTesting issue retrieval..."
TEST_ISSUE=${TEST_JIRA_ISSUE:-"INC-1"}
if acli jira issue get "$TEST_ISSUE" &> /dev/null; then
    echo "✅ Issue retrieval successful!"
    acli jira issue get "$TEST_ISSUE" --format json | head -n 20
else
    echo "⚠️  Issue $TEST_ISSUE not found (this is OK if it doesn't exist)"
fi

# Test search
echo -e "\nTesting JIRA search..."
if acli jira search "order by created DESC" --max-results 3 &> /dev/null; then
    echo "✅ JIRA search working!"
    acli jira search "order by created DESC" --max-results 3
else
    echo "❌ JIRA search failed"
    exit 1
fi

echo -e "\n✅ All ACLI JIRA tests passed!"
