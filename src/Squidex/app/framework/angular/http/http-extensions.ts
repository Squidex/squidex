/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { ErrorDto } from './../../utils/error';
import { Types} from './../../utils/types';
import { Version, Versioned } from './../../utils/version';

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

    export function patchVersioned<T>(http: HttpClient, url: string, body: any, version?: Version): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version);

        return handleVersion(http.request<T>('PATCH', url, { body, observe: 'response', headers }), version);
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
        return httpRequest.pipe(map((response: HttpResponse<T>) => {
            const etag = response.headers.get('etag') || '';

            return new Versioned(new Version(etag), response);
        }));
    }
}

export const pretifyError = (message: string) => <T>(source: Observable<T>) =>
    source.pipe(catchError((response: HttpErrorResponse) => {
        let result: ErrorDto | null = null;

        if (!Types.is(response.error, Error)) {
            try {
                let errorDto = Types.isObject(response.error) ? response.error : JSON.parse(response.error);

                if (!errorDto) {
                    errorDto = { message: 'Failed to make the request.', details: [] };
                }

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

        return throwError(result);
    }));