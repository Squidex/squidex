/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-inner-declarations */

import { HttpClient, HttpErrorResponse, HttpEvent, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { ErrorDto, getActualVersion, Types, Versioned, VersionOrTag, VersionTag } from '@app/framework/internal';

export module HTTP {
    export type UploadFile = File | { url: string; name: string };

    export function upload<T = any>(http: HttpClient, method: string, url: string, file: UploadFile,
        version?: VersionOrTag): Observable<HttpEvent<T>> {
        const req = new HttpRequest(method, url, getFormData(file), { headers: createHeaders(version, undefined), reportProgress: true });

        return http.request<T>(req);
    }

    export function getVersioned<T = any>(http: HttpClient, url: string,
        version?: VersionOrTag, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.get<T>(url, { observe: 'response', headers }));
    }

    export function postVersioned<T = any>(http: HttpClient, url: string, body: any,
        version?: VersionOrTag, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.post<T>(url, body, { observe: 'response', headers }));
    }

    export function putVersioned<T = any>(http: HttpClient, url: string, body: any,
        version?: VersionOrTag, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.put<T>(url, body, { observe: 'response', headers }));
    }

    export function patchVersioned<T = any>(http: HttpClient, url: string, body: any,
        version?: VersionOrTag, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.request<T>('PATCH', url, { body, observe: 'response', headers }));
    }

    export function deleteVersioned<T = any>(http: HttpClient, url: string,
        version?: VersionOrTag, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.delete<T>(url, { observe: 'response', headers }));
    }

    export function requestVersioned<T = any>(http: HttpClient, method: string, url: string,
        version?: VersionOrTag, body?: any, customHeaders?: HttpHeaders): Observable<Versioned<HttpResponse<T>>> {
        const headers = createHeaders(version, customHeaders);

        return handleVersion(http.request<T>(method, url, { observe: 'response', headers, body }));
    }

    function getFormData(file: UploadFile) {
        const formData = new FormData();

        const untyped = file as any;
        if (Types.isObject(untyped) && Types.isString(untyped['name']) && Types.isString(untyped['url'])) {
            formData.append('url', untyped.url);
            formData.append('name', untyped.name);
        } else {
            formData.append('file', untyped);
        }

        return formData;
    }

    function createHeaders(version: VersionOrTag | undefined, customHeaders: HttpHeaders | undefined): HttpHeaders {
        customHeaders ||= new HttpHeaders();

        const actualVersion = getActualVersion(version);
        if (actualVersion) {
            return customHeaders.set('If-Match', `${actualVersion}`);
        }

        return customHeaders;
    }

    function handleVersion<T>(httpRequest: Observable<HttpResponse<T>>): Observable<Versioned<HttpResponse<T>>> {
        return httpRequest.pipe(map((response: HttpResponse<T>) => {
            const etag = response.headers.get('etag') || '';

            return { version: new VersionTag(etag), payload: response };
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

    if (parsed && Types.isString(parsed.message) && parsed.message) {
        return new ErrorDto(status, parsed.message, parsed.errorCode, parsed.details, response);
    }

    return new ErrorDto(500, fallback, null, [], response);
}
