/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Version } from './../utils/version';

export class EntityCreatedDto {
    constructor(
        public readonly id: any
    ) {
    }
}

export class ErrorDto {
    public get displayMessage(): string {
        let result = this.message;

        if (this.details && this.details.length > 0) {
            const detailMessage = this.details[0];

            const lastChar = result[result.length - 1];

            if (lastChar !== '.' && lastChar !== ',') {
                result += '.';
            }

            result += ' ';
            result += detailMessage;
        }

        const lastChar = result[result.length - 1];

        if (lastChar !== '.') {
            result += '.';
        }

        return result;
    }

    constructor(
        public readonly statusCode: number,
        public readonly message: string,
        public readonly details: string[] = []
    ) {
    }
}

export module HTTP {
    export function getVersioned(http: HttpClient, url: string, version?: Version): Observable<any> {
        if (version) {
            return http.get(url, { observe: 'response', headers: new HttpHeaders().set('If-Match', version.value) })
                .do((response: HttpResponse<any>) => {
                    if (version && response.status.toString().indexOf('2') === 0 && response.headers) {
                        const etag = response.headers.get('etag');

                        if (etag) {
                            version.update(etag);
                        }
                    }
                }).map((response: HttpResponse<any>) => response.body);
        } else {
            return http.get(url, { observe: 'response' }).map((response: HttpResponse<any>) => response.body);
        }
    }

    export function postVersioned(http: HttpClient, url: string, body: any, version?: Version): Observable<any> {
        if (version) {
            return http.post(url, body, { observe: 'response', headers: new HttpHeaders().set('If-Match', version.value) })
                .do((response: HttpResponse<any>) => {
                    if (version && response.status.toString().indexOf('2') === 0 && response.headers) {
                        const etag = response.headers.get('etag');

                        if (etag) {
                            version.update(etag);
                        }
                    }
                }).map((response: HttpResponse<any>) => response.body);
        } else {
            return http.post(url, body, { observe: 'response' }).map((response: HttpResponse<any>) => response.body);
        }
    }

    export function putVersioned(http: HttpClient, url: string, body: any, version?: Version): Observable<any> {
        if (version) {
            return http.put(url, body, { observe: 'response', headers: new HttpHeaders().set('If-Match', version.value) })
                .do((response: HttpResponse<any>) => {
                    if (version && response.status.toString().indexOf('2') === 0 && response.headers) {
                        const etag = response.headers.get('etag');

                        if (etag) {
                            version.update(etag);
                        }
                    }
                }).map((response: HttpResponse<any>) => response.body);
        } else {
            return http.put(url, body, { observe: 'response' }).map((response: HttpResponse<any>) => response.body);
        }
    }

    export function deleteVersioned(http: HttpClient, url: string, version?: Version): Observable<any> {
        if (version) {
            return http.delete(url, { observe: 'response', headers: new HttpHeaders().set('If-Match', version.value) })
                .do((response: HttpResponse<any>) => {
                    if (version && response.status.toString().indexOf('2') === 0 && response.headers) {
                        const etag = response.headers.get('etag');

                        if (etag) {
                            version.update(etag);
                        }
                    }
                }).map((response: HttpResponse<any>) => response.body);
        } else {
            return http.delete(url, { observe: 'response' }).map((response: HttpResponse<any>) => response.body);
        }
    }
}

export function pretifyError(message: string): Observable<any> {
    return this.catch((response: HttpErrorResponse) => {
        let result = new ErrorDto(500, message);

        if (!(response.error instanceof Error)) {
            try {
                if (response.status === 412) {
                    result = new ErrorDto(response.status, 'Failed to make the update. Another user has made a change. Please reload.');
                } else if (response.status !== 500) {
                    result = new ErrorDto(response.status, response.error.message, response.error.details);
                }
            } catch (e) {
                result = result;
            }
        }

        return Observable.throw(result);
    });
}