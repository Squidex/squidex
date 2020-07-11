import { check } from 'k6';
import http from 'k6/http';

export const options = {
    stages: [
        { duration: "2m", target: 300 },
        { duration: "2m", target: 300 },
        { duration: "2m", target: 0 },
    ],
    thresholds: {
        'http_req_duration': ['p(99)<300'], // 99% of requests must complete below 300ms
    }
};

export default function () {
    const url = `https://test-api.k6.io/`;

    const response = http.get(url);

    check(response, {
        'is status 200': (r) => r.status === 200,
    });
}