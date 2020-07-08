import { check } from 'k6';
import http from 'k6/http';
import { variables, getBearerToken } from './shared.js';

export let options = {
    stages: [
        { duration: "2m", target: 200 },
        { duration: "2m", target: 200 },
        { duration: "2m", target: 0 },
    ],
    thresholds: {
        'http_req_duration': ['p(99)<1500'], // 99% of requests must complete below 1.5s
    }
};


export function setup() {
    const token = getBearerToken();

    return { token };
}

export default function (data) {
    const url = `${variables.serverUrl}/api/apps/${variables.appName}/clients`;

    const response = http.get(url, {
        headers: {
            Authorization: `Bearer ${data.token}`
        }
    });

    check(response, {
        'is status 200': (r) => r.status === 200,
    });
}