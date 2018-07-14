/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpEventType, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { filter, map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP,
    Model,
    pretifyError,
    Types,
    Version,
    Versioned
} from '@app/framework';

export class FolderPathItemDto {
    constructor(
        public readonly id: string,
        public readonly name: string
    ) {
    }
}

export class AssetsDto extends Model {
    constructor(
        public readonly total: number,
        public readonly items: AssetDto[]
    ) {
        super();
    }

    public with(value: Partial<AssetsDto>): AssetsDto {
        return this.clone(value);
    }
}

export class FolderDto extends Model {
    constructor(
        public readonly total: number,
        public readonly items: AssetDto[],
        public readonly path: FolderPathItemDto[]
    ) {
        super();
    }

    public with(value: Partial<AssetsDto>): AssetsDto {
        return this.clone(value);
    }
}

export class AssetDto extends Model {
    constructor(
        public readonly id: string,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly name: string,
        public readonly fileType: string,
        public readonly fileSize: number,
        public readonly fileVersion: number,
        public readonly mimeType: string,
        public readonly folderId: string,
        public readonly isFolder: boolean,
        public readonly isImage: boolean,
        public readonly pixelWidth: number | null,
        public readonly pixelHeight: number | null,
        public readonly url: string,
        public readonly version: Version
    ) {
        super();
    }

    public with(value: Partial<AssetDto>): AssetDto {
        return this.clone(value);
    }

    public update(update: AssetReplacedDto, user: string, version: Version, now?: DateTime): AssetDto {
        return this.with({
            ...update,
            lastModified: now || DateTime.now(),
            lastModifiedBy: user,
            version
        });
    }

    public rename(name: string, user: string, version: Version, now?: DateTime): AssetDto {
        return this.with({
            name,
            lastModified: now || DateTime.now(),
            lastModifiedBy: user,
            version
        });
    }
}

export class CreateFolderDto {
    constructor(
        public readonly name: string
    ) {
    }
}

export class UpdateAssetDto {
    constructor(
        public readonly name: string
    ) {
    }
}

export class MoveAssetDto {
    constructor(
        public readonly folderId: string
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
        private readonly analytics: AnalyticsService
    ) {
    }

    public getAssets(appName: string, take: number, skip: number, query?: string, ids?: string[]): Observable<AssetsDto> {
        let fullQuery = '';

        if (ids) {
            fullQuery = `ids=${ids.join(',')}`;
        } else {
            const queries: string[] = [];

            if (query && query.length > 0) {
                queries.push(`$filter=contains(fileName,'${encodeURIComponent(query)}')`);
            }

            queries.push(`$top=${take}`);
            queries.push(`$skip=${skip}`);

            fullQuery = queries.join('&');
        }

        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets?${fullQuery}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.items;

                    return new AssetsDto(body.total, items.map(item => {
                        const assetUrl = this.apiUrl.buildUrl(`api/assets/${item.id}`);

                        return new AssetDto(
                            item.id,
                            item.createdBy,
                            item.lastModifiedBy,
                            DateTime.parseISO_UTC(item.created),
                            DateTime.parseISO_UTC(item.lastModified),
                            item.name,
                            item.fileType,
                            item.fileSize,
                            item.fileVersion,
                            item.mimeType,
                            item.folderId,
                            item.isFolder,
                            item.isImage,
                            item.pixelWidth,
                            item.pixelHeight,
                            assetUrl,
                            new Version(item.version.toString()));
                    }));
                }),
                pretifyError('Failed to load assets. Please reload.'));
    }

    public uploadFile(appName: string, file: File, folderId: string, user: string, now: DateTime): Observable<number | AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets`);

        const req = new HttpRequest('POST', url, getFormData(file), {
            reportProgress: true
        });

        return this.http.request<any>(req).pipe(
                filter(event =>
                     event.type === HttpEventType.UploadProgress ||
                     event.type === HttpEventType.Response),
                map(event => {
                    if (event.type === HttpEventType.UploadProgress) {
                        const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                        return percentDone;
                    } else if (Types.is(event, HttpResponse)) {
                        const response: any = event.body;
                        const assetUrl = this.apiUrl.buildUrl(`api/assets/${response.id}`);

                        now = now || DateTime.now();

                        const dto =  new AssetDto(
                            response.id,
                            user,
                            user,
                            now,
                            now,
                            response.name,
                            response.fileType,
                            response.fileSize,
                            response.fileVersion,
                            response.mimeType,
                            folderId,
                            false,
                            response.isImage,
                            response.pixelWidth,
                            response.pixelHeight,
                            assetUrl,
                            new Version(event.headers.get('etag')!));

                        return dto;
                    } else {
                        throw 'Invalid';
                    }
                }),
                tap(dto => {
                    this.analytics.trackEvent('Asset', 'Uploaded', appName);
                }),
                pretifyError('Failed to upload asset. Please reload.'));
    }

    public postFolder(appName: string, request: CreateFolderDto, folderId: string, user: string, now: DateTime): Observable<number | AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets`);

        return HTTP.postVersioned<any>(this.http, url, request).pipe(
                map(response => {
                    const body = response.payload.body;

                    now = now || DateTime.now();

                    const dto =  new AssetDto(
                        body.id,
                        user,
                        user,
                        now,
                        now,
                        request.name,
                        '',
                        0,
                        0,
                        '',
                        folderId,
                        true,
                        false,
                        null,
                        null,
                        '',
                        response.version);

                    return dto;
                }),
                tap(dto => {
                    this.analytics.trackEvent('Asset', 'FolderCreated', appName);
                }),
                pretifyError('Failed to create folder. Please reload.'));
    }

    public getAsset(appName: string, id: string): Observable<AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    const assetUrl = this.apiUrl.buildUrl(`api/assets/${body.id}`);

                    return new AssetDto(
                        body.id,
                        body.createdBy,
                        body.lastModifiedBy,
                        DateTime.parseISO_UTC(body.created),
                        DateTime.parseISO_UTC(body.lastModified),
                        body.name,
                        body.fileType,
                        body.fileSize,
                        body.fileVersion,
                        body.mimeType,
                        body.folderId,
                        body.isFolder,
                        body.isImage,
                        body.pixelWidth,
                        body.pixelHeight,
                        assetUrl,
                        response.version);
                }),
                pretifyError('Failed to load assets. Please reload.'));
    }

    public replaceFile(appName: string, id: string, file: File, version: Version): Observable<number | Versioned<AssetReplacedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}/content`);

        const req = new HttpRequest('PUT', url, getFormData(file), {
            headers: new HttpHeaders({
                'If-Match': version.value
            }),
            reportProgress: true
        });

        return this.http.request(req).pipe(
                filter(event =>
                    event.type === HttpEventType.UploadProgress ||
                    event.type === HttpEventType.Response),
                map(event => {
                    if (event.type === HttpEventType.UploadProgress) {
                        const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                        return percentDone;
                    } else if (Types.is(event, HttpResponse)) {
                        const response: any = event.body;

                        const replaced =  new AssetReplacedDto(
                            response.fileSize,
                            response.fileVersion,
                            response.mimeType,
                            response.isImage,
                            response.pixelWidth,
                            response.pixelHeight);

                        return new Versioned(new Version(event.headers.get('etag')!), replaced);
                    } else {
                        throw 'Invalid';
                    }
                }),
                tap(() => {
                    this.analytics.trackEvent('Assets', 'Replaced', appName);
                }),
                pretifyError('Failed to replace asset. Please reload.'));
    }

    public deleteAsset(appName: string, id: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Assets', 'Deleted', appName);
                }),
                pretifyError('Failed to delete asset. Please reload.'));
    }

    public moveAsset(appName: string, id: string, dto: MoveAssetDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}/move`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Assets', 'Moved', appName);
                }),
                pretifyError('Failed to move asset. Please reload.'));
    }

    public putAsset(appName: string, id: string, dto: UpdateAssetDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Assets', 'Updated', appName);
                }),
                pretifyError('Failed to delete asset. Please reload.'));
    }
}

function getFormData(file: File) {
    const formData = new FormData();

    formData.append('file', file);

    return formData;
}