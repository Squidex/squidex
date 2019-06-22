/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import {
    ErrorDto,
    Types,
    Version,
    Versioned
} from '@app/framework/internal';

export module HTTP {
    export function getVersioned<T = any>(http: HttpClient, url: string, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.get<T>(url, { observe: 'response', headers }));
    }

    export function postVersioned<T = any>(http: HttpClient, url: string, body: any, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.post<T>(url, body, { observe: 'response', headers }));
    }

    export function putVersioned<T = any>(http: HttpClient, url: string, body: any, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.put<T>(url, body, { observe: 'response', headers }));
    }

    export function patchVersioned<T = any>(http: HttpClient, url: string, body: any, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.request<T>('PATCH', url, { body, observe: 'response', headers }));
    }

    export function deleteVersioned<T = any>(http: HttpClient, url: string, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.delete<T>(url, { observe: 'response', headers }));
    }

    export function requestVersioned<T = any>(http: HttpClient, method: string, url: string, version?: Version, body?: any): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.request<T>(method, url, { observe: 'response', headers, body }));
    }

    function createHeaders(version?: Version): HttpHeaders {
        if (version && version.value && version.value.length > 0) {
            return new HttpHeaders().set('If-Match', version.value);
        } else {
            return new HttpHeaders();
        }
    }

    function handleVersion<T>(httpRequest: Observable<HttpResponse<T>>): Observable<Versioned<HttpResponse<T>>> {
        return httpRequest.pipe(map((response: HttpResponse<T>) => {
            const etag = response.headers.get('etag') || '';

            return { version: new Version(etag), payload: response };
        }));
    }
}

export const pretifyError = (message: string) => <T>(source: Observable<T>) =>
    source.pipe(catchError((response: HttpErrorResponse) => {
        if (Types.is(response, ErrorDto)) {
            return throwError(response);
        }

        let result: ErrorDto | null = null;

        if (!Types.is(response.error, Error)) {
            try {
                let errorDto = Types.isObject(response.error) ? response.error : JSON.parse(response.error);

                if (!errorDto) {
                    errorDto = { message: 'Failed to make the request.', details: [] };
                }

                if (response.status === 412) {
                    result = new ErrorDto(response.status, 'Failed to make the update. Another user has made a change. Please reload.', [], response);
                } else if (response.status !== 500) {
                    result = new ErrorDto(response.status, errorDto.message, errorDto.details, response);
                }
            } catch (e) {
                result = new ErrorDto(500, 'Failed to make the request.', [], response);
            }
        }

        result = result || new ErrorDto(500, message);

        return throwError(result);
    }));