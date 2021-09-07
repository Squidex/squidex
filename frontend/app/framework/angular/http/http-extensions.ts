/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-inner-declarations */

import { HttpClient, HttpErrorResponse, HttpEvent, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { ErrorDto, Types, Version, Versioned } from '@app/framework/internal';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export module HTTP {
    export function upload<T = any>(http: HttpClient, method: string, url: string, file: Blob, version?: Version): Observable<HttpEvent<T>> {
        const req = new HttpRequest(method, url, getFormData(file), { headers: createHeaders(version, undefined), reportProgress: true });

        return http.request<T>(req);
    }

    export function getVersioned<T = any>(http: HttpClient, url: string,
        version?: Version, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.get<T>(url, { observe: 'response', headers }));
    }

    export function postVersioned<T = any>(http: HttpClient, url: string, body: any,
        version?: Version, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.post<T>(url, body, { observe: 'response', headers }));
    }

    export function putVersioned<T = any>(http: HttpClient, url: string, body: any,
        version?: Version, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.put<T>(url, body, { observe: 'response', headers }));
    }

    export function patchVersioned<T = any>(http: HttpClient, url: string, body: any,
        version?: Version, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.request<T>('PATCH', url, { body, observe: 'response', headers }));
    }

    export function deleteVersioned<T = any>(http: HttpClient, url: string,
        version?: Version, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.delete<T>(url, { observe: 'response', headers }));
    }

    export function requestVersioned<T = any>(http: HttpClient, method: string, url: string,
        version?: Version, body?: any, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.request<T>(method, url, { observe: 'response', headers, body }));
    }

    function getFormData(file: Blob) {
        const formData = new FormData();

        formData.append('file', file);

        return formData;
    }

    function createHeaders(version: Version | undefined, customHeaders: HttpHeaders | undefined): HttpHeaders {
        customHeaders ||= new HttpHeaders();

        if (version && version.value && version.value.length > 0) {
            return customHeaders.set('If-Match', version.value);
        }

        return customHeaders;
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
        const error = parseError(response, message);

        return throwError(() => error);
    }));

export function parseError(response: HttpErrorResponse, fallback: string) {
    if (!response) {
        return response;
    }

    if (Types.is(response, ErrorDto)) {
        return response;
    }

    const { error, status } = response;

    if (status === 412) {
        return new ErrorDto(412, 'i18n:common.httpConflict', null, [], response);
    }

    if (status === 429) {
        return new ErrorDto(429, 'i18n:common.httpLimit', null, [], response);
    }

    let parsed: any;

    if (Types.isObject(error)) {
        parsed = error;
    } else if (Types.isString(error)) {
        try {
            parsed = JSON.parse(error);
        } catch (e) {
            parsed = undefined;
        }
    }

    if (parsed && Types.isString(parsed.message)) {
        return new ErrorDto(status, parsed.message, parsed.errorCode, parsed.details, response);
    }

    return new ErrorDto(500, fallback, null, [], response);
}
