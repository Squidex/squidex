/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient, HttpErrorResponse, HttpEventType, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    LocalCacheService,
    HTTP,
    Version,
    Versioned
} from 'framework';

export class AssetsDto {
    constructor(
        public readonly total: number,
        public readonly items: AssetDto[]
    ) {
    }
}

export class AssetDto {
    constructor(
        public readonly id: string,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly fileName: string,
        public readonly fileType: string,
        public readonly fileSize: number,
        public readonly fileVersion: number,
        public readonly mimeType: string,
        public readonly isImage: boolean,
        public readonly pixelWidth: number | null,
        public readonly pixelHeight: number | null,
        public readonly version: Version
    ) {
    }

    public update(update: AssetReplacedDto, user: string, version: Version, now?: DateTime): AssetDto {
        return new AssetDto(
            this.id,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.fileName,
            this.fileType,
            update.fileSize,
            update.fileVersion,
            update.mimeType,
            update.isImage,
            update.pixelWidth,
            update.pixelHeight,
            version);
    }

    public rename(name: string, user: string, version: Version, now?: DateTime): AssetDto {
        return new AssetDto(
            this.id,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            name,
            this.fileType,
            this.fileSize,
            this.fileVersion,
            this.mimeType,
            this.isImage,
            this.pixelWidth,
            this.pixelHeight,
            version);
    }
}

export class UpdateAssetDto {
    constructor(
        public readonly fileName: string
    ) {
    }
}

export class AssetReplacedDto {
    constructor(
        public readonly fileSize: number,
        public readonly fileVersion: number,
        public readonly mimeType: string,
        public readonly isImage: boolean,
        public readonly pixelWidth: number | null,
        public readonly pixelHeight: number | null
    ) {
    }
}

@Injectable()
export class AssetsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
        private readonly localCache: LocalCacheService
    ) {
    }

    public getAssets(appName: string, take: number, skip: number, query?: string, mimeTypes?: string[], ids?: string[]): Observable<AssetsDto> {
        const queries: string[] = [];

        if (mimeTypes && mimeTypes.length > 0) {
            queries.push(`mimeTypes=${mimeTypes.join(',')}`);
        }

        if (ids && ids.length > 0) {
            queries.push(`ids=${ids.join(',')}`);
        }

        if (query && query.length > 0) {
            queries.push(`query=${query}`);
        }

        queries.push(`take=${take}`);
        queries.push(`skip=${skip}`);

        const fullQuery = queries.join('&');

        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets?${fullQuery}`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.items;

                    return new AssetsDto(body.total, items.map(item => {
                        return new AssetDto(
                            item.id,
                            item.createdBy,
                            item.lastModifiedBy,
                            DateTime.parseISO_UTC(item.created),
                            DateTime.parseISO_UTC(item.lastModified),
                            item.fileName,
                            item.fileType,
                            item.fileSize,
                            item.fileVersion,
                            item.mimeType,
                            item.isImage,
                            item.pixelWidth,
                            item.pixelHeight,
                            new Version(item.version.toString()));
                    }));
                })
                .pretifyError('Failed to load assets. Please reload.');
    }

    public uploadFile(appName: string, file: File, user: string, now: DateTime): Observable<number | AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets`);

        const req = new HttpRequest('POST', url, getFormData(file), {
            reportProgress: true
        });

        return this.http.request<any>(req)
                .filter(event =>
                     event.type === HttpEventType.UploadProgress ||
                     event.type === HttpEventType.Response)
                .map(event => {
                    if (event.type === HttpEventType.UploadProgress) {
                        const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                        return percentDone;
                    } else if (event instanceof HttpResponse) {
                        const response: any = event.body;

                        now = now || DateTime.now();

                        const dto =  new AssetDto(
                            response.id,
                            user,
                            user,
                            now,
                            now,
                            response.fileName,
                            response.fileType,
                            response.fileSize,
                            response.fileVersion,
                            response.mimeType,
                            response.isImage,
                            response.pixelWidth,
                            response.pixelHeight,
                            new Version(event.headers.get('etag')));

                        this.localCache.set(`asset.${dto.id}`, dto, 5000);

                        return dto;
                    }
                })
                .do(dto => {
                    this.analytics.trackEvent('Asset', 'Uploaded', appName);
                })
                .pretifyError('Failed to upload asset. Please reload.');
    }

    public getAsset(appName: string, id: string): Observable<AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    return new AssetDto(
                        body.id,
                        body.createdBy,
                        body.lastModifiedBy,
                        DateTime.parseISO_UTC(body.created),
                        DateTime.parseISO_UTC(body.lastModified),
                        body.fileName,
                        body.fileType,
                        body.fileSize,
                        body.fileVersion,
                        body.mimeType,
                        body.isImage,
                        body.pixelWidth,
                        body.pixelHeight,
                        response.version);
                })
                .catch(error => {
                    if (error instanceof HttpErrorResponse && error.status === 404) {
                        const cached = this.localCache.get(`asset.${id}`);

                        if (cached) {
                            return Observable.of(cached);
                        }
                    }

                    return Observable.throw(error);
                })
                .pretifyError('Failed to load assets. Please reload.');
    }

    public replaceFile(appName: string, id: string, file: File, version: Version): Observable<number | Versioned<AssetReplacedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}/content`);

        const req = new HttpRequest('PUT', url, getFormData(file), {
            headers: new HttpHeaders({
                'If-Match': version.value
            }),
            reportProgress: true
        });

        return this.http.request(req)
                .filter(event =>
                    event.type === HttpEventType.UploadProgress ||
                    event.type === HttpEventType.Response)
                .map(event => {
                    if (event.type === HttpEventType.UploadProgress) {
                        const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                        return percentDone;
                    } else if (event instanceof HttpResponse) {
                        const response: any = event.body;

                        const replaced =  new AssetReplacedDto(
                            response.fileSize,
                            response.fileVersion,
                            response.mimeType,
                            response.isImage,
                            response.pixelWidth,
                            response.pixelHeight);

                        return new Versioned(new Version(event.headers.get('etag')), replaced);
                    }
                })
                .do(() => {
                    this.analytics.trackEvent('Analytics', 'Replaced', appName);
                })
                .pretifyError('Failed to replace asset. Please reload.');
    }

    public deleteAsset(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .do(() => {
                    this.analytics.trackEvent('Analytics', 'Deleted', appName);

                    this.localCache.remove(`asset.${id}`);
                })
                .pretifyError('Failed to delete asset. Please reload.');
    }

    public putAsset(appName: string, id: string, dto: UpdateAssetDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .do(() => {
                    this.analytics.trackEvent('Analytics', 'Updated', appName);
                })
                .pretifyError('Failed to delete asset. Please reload.');
    }
}

function getFormData(file: File) {
    const formData = new FormData();

    formData.append('file', file);

    return formData;
}