import http from 'k6/http';

export const variables = {
    appName: getValue('APP__NAME', 'integration-tests'),
    clientId: getValue('CLIENT__ID', 'root'),
    clientSecret: getValue('CLIENT__SECRET', 'xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0='),
    serverUrl: getValue('SERVER__URL', 'https://localhost:5001')
};

let bearerToken = null;

export function getBearerToken() {
    if (!bearerToken) {
        const url = `${variables.serverUrl}/identity-server/connect/token`;

        const response = http.post(url, {
            grant_type: 'client_credentials',
            client_id: variables.clientId,
            client_secret: variables.clientSecret,
            scope: 'squidex-api'
        });

        const json = JSON.parse(response.body);

        bearerToken = json.access_token;
    }

    return bearerToken;
}

function getValue(key, fallback) {
    const result = __ENV[key] || fallback;

    return result;
}