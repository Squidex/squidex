import http from 'k6/http';

export const variables = {
    appName: getValue('APP__NAME', 'integration-tests'),
    clientId: getValue('CLIENT__ID', 'root'),
    clientSecret: getValue('CLIENT__SECRET', 'xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0='),
    serverUrl: getValue('SERVER__URL', 'https://localhost:5001')
};

let bearerToken = null;

export function getBearerToken(appName) {
    if (!bearerToken) {
        const adminToken = getToken(variables.clientId, variables.clientSecret);

        const clientsUrl = `${variables.serverUrl}/api/apps/${appName}/clients`;

        const clientsResponse = http.get(clientsUrl, {
            headers: {
                Authorization: `Bearer ${adminToken}`
            }
        });

        const clientsJson = JSON.parse(clientsResponse.body);
        const client = clientsJson.items[0];

        const clientId = `${appName}:${client.id}`;
        const clientSecret = client.secret;

        console.log(`Using ${clientId} / ${clientSecret}`);

        bearerToken = getToken(clientId, clientSecret);
    }

    return bearerToken;
}

function getToken(clientId, clientSecret) {
    const tokenUrl = `${variables.serverUrl}/identity-server/connect/token`;

    const tokenResponse = http.post(tokenUrl, {
        grant_type: 'client_credentials',
        client_id: clientId,
        client_secret: clientSecret,
        scope: 'squidex-api'
    }, {
        responseType: 'text'
    });

    if (tokenResponse.status !== 200) {
        throw new Error('Invalid response.');
    }

    const tokenJson = JSON.parse(tokenResponse.body);

    return tokenJson.access_token;
}

function getValue(key, fallback) {
    const result = __ENV[key] || fallback;

    return result;
}