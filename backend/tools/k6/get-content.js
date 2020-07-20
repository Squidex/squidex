import http from 'k6/http';
import { check } from 'k6';
import { variables, getBearerToken } from './shared.js';

export const options = {
    stages: [
        { duration: "2m", target: 500 },
        { duration: "2m", target: 0 },
    ],
    thresholds: {
        'http_req_duration': ['p(99)<300'], // 99% of requests must complete below 300ms
    }
};

export function setup() {
    const token = getBearerToken('ci-semantic-search');

    return { token };
}

export default function (data) {
    const url = `${variables.serverUrl}/api/content/ci-semantic-search/test/5d648f76-7ae9-4141-a325-0c31ed155e5c`;

    const response = http.get(url, {
        headers: {
            Authorization: `Bearer ${data.token}`
        }
    });

    check(response, {
        'is status 200': (r) => r.status === 200,
    });
} 
