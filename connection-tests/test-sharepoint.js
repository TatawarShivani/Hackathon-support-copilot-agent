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
