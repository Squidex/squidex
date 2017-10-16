/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Version } from './../utils/version';

export class Versioned<T> {
    constructor(
        public readonly version: Version,
        public readonly payload: T
    ) {
    }
}

function formatMessage(message: string, details?: string[]) {
    const parts: string[] = [];

    const addPart = (p: string) => {
        p = p.trim();

        const c = p[p.length - 1];

        if (c !== '.') {
            p += '.';
        }

        parts.push(p);
    };

    addPart(message);

    if (details) {
        for (let d of details) {
            addPart(d);
        }
    }

    return parts.join(' ');
}

export class ErrorDto {
    public readonly displayMessage: string;

    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly details: string[] = []
    ) {
        this.displayMessage = formatMessage(message, details);
    }
}

export module HTTP {
    export function getVersioned<T>(http: HttpClient, url: string, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.get<T>(url, { observe: 'response', headers }), version);
    }

    export function postVersioned<T>(http: HttpClient, url: string, body: any, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.post<T>(url, body, { observe: 'response', headers }), version);
    }

    export function putVersioned<T>(http: HttpClient, url: string, body: any, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.put<T>(url, body, { observe: 'response', headers }), version);
    }

    export function deleteVersioned<T>(http: HttpClient, url: string, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.delete<T>(url, { observe: 'response', headers }), version);
    }

    function createHeaders(version?: Version): HttpHeaders {
        if (version && version.value && version.value.length > 0) {
            return new HttpHeaders().set('If-Match', version.value);
        } else {
            return new HttpHeaders();
        }
    }

    function handleVersion<T>(httpRequest: Observable<HttpResponse<T>>, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        return httpRequest.map((response: HttpResponse<T>) => {
            const etag = response.headers.get('etag') || '';

            return new Versioned(new Version(etag), response);
        });
    }
}

export function pretifyError(message: string): Observable<any> {
    return this.catch((response: HttpErrorResponse) => {
        let result: ErrorDto = null;

        if (!(response.error instanceof Error)) {
            try {
                const errorDto = JSON.parse(response.error);

                if (response.status === 412) {
                    result = new ErrorDto(response.status, 'Failed to make the update. Another user has made a change. Please reload.');
                } else if (response.status !== 500) {
                    result = new ErrorDto(response.status, errorDto.message, errorDto.details);
                }
            } catch (e) {
                result = new ErrorDto(500, 'Failed to make the request.');
            }
        }

        result = result || new ErrorDto(500, message);

        return Observable.throw(result);
    });
}