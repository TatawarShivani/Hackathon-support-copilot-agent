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
