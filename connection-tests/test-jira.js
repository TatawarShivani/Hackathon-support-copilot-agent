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
      testGetIssue();
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
