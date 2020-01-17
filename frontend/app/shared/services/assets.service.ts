/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, filter, map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    ErrorDto,
    hasAnyLink,
    HTTP,
    Metadata,
    pretifyError,
    Resource,
    ResourceLinks,
    ResultSet,
    Types,
    Version,
    Versioned
} from '@app/framework';

const SVG_PREVIEW_LIMIT = 10 * 1024;

import { encodeQuery, Query } from './../state/query';

export class AssetsDto extends ResultSet<AssetDto> {
    public get canCreate() {
        return hasAnyLink(this._links, 'create');
    }
}

export class AssetDto {
    public readonly _meta: Metadata = {};

    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canPreview: boolean;
    public readonly canUpdate: boolean;
    public readonly canUpload: boolean;
    public readonly canMove: boolean;

    public get isDuplicate() {
        return this._meta && this._meta['isDuplicate'] === 'true';
    }

    public get contentUrl() {
        return this._links['content'].href;
    }

    constructor(links: ResourceLinks, meta: Metadata,
        public readonly id: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly fileName: string,
        public readonly fileHash: string,
        public readonly fileType: string,
        public readonly fileSize: number,
        public readonly fileVersion: number,
        public readonly isProtected: boolean,
        public readonly parentId: string,
        public readonly mimeType: string,
        public readonly type: string,
        public readonly metadataText: string,
        public readonly metadata: any,
        public readonly slug: string,
        public readonly tags: ReadonlyArray<string>,
        public readonly version: Version
    ) {
        this.canPreview = this.type === 'Image' || (this.mimeType === 'image/svg+xml' && this.fileSize < SVG_PREVIEW_LIMIT);

        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canUpload = hasAnyLink(links, 'upload');
        this.canMove = hasAnyLink(links, 'move');

        this._meta = meta;
    }

    public fullUrl(apiUrl: ApiUrlConfig) {
        return apiUrl.buildUrl(this.contentUrl);
    }
}

export class AssetFoldersDto extends ResultSet<AssetFolderDto> {
    public get canCreate() {
        return hasAnyLink(this._links, 'create');
    }
}

export class AssetFolderDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canUpdate: boolean;
    public readonly canMove: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly folderName: string,
        public readonly parentId: string,
        public readonly version: Version
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canMove = hasAnyLink(links, 'move');
    }
}

export interface AnnotateAssetDto {
    readonly fileName?: string;
    readonly isProtected?: boolean;
    readonly slug?: string;
    readonly tags?: ReadonlyArray<string>;
    readonly metadata?: { [key: string]: any };
}

export interface CreateAssetFolderDto {
    readonly parentId?: string;
    readonly folderName: string;
}

export interface RenameAssetFolderDto {
    readonly folderName: string;
}

export interface MoveAssetItemDto {
    readonly parentId?: string;
}

@Injectable()
export class AssetsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getTags(appName: string): Observable<{ [name: string]: number }> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/tags`);

        return this.http.get<{ [name: string]: number }>(url);
    }

    public getAssets(appName: string, take: number, skip: number, query?: Query, tags?: ReadonlyArray<string>, ids?: ReadonlyArray<string>, parentId?: string): Observable<AssetsDto> {
        let fullQuery = '';

        if (ids) {
            fullQuery = `ids=${ids.join(',')}`;
        } else {
            const queryObj: Query = {};

            const filters: any[] = [];

            if (query && query.fullText && query.fullText.length > 0) {
                filters.push({ path: 'fileName', op: 'contains', value: query.fullText });
            }

            if (tags) {
                for (let tag of tags) {
                    if (tag && tag.length > 0) {
                        filters.push({ path: 'tags', op: 'eq', value: tag });
                    }
                }
            }

            if (filters.length > 0) {
                queryObj.filter = { and: filters };
            }

            if (take > 0) {
                queryObj.take = take;
            }

            if (skip > 0) {
                queryObj.skip = skip;
            }

            fullQuery = `q=${encodeQuery(queryObj)}`;

            if (parentId) {
                fullQuery += `&parentId=${parentId}`;
            }
        }

        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets?${fullQuery}`);

        return this.http.get<{ total: number, items: any[], folders: any[] } & Resource>(url).pipe(
            map(({ total, items, _links }) => {
                const assets = items.map(item => parseAsset(item));

                return new AssetsDto(total, assets, _links);
            }),
            pretifyError('Failed to load assets. Please reload.'));
    }

    public getAssetFolders(appName: string, parentId?: string): Observable<AssetFoldersDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/folders?parentId=${parentId}`);

        return this.http.get<{ total: number, items: any[], folders: any[] } & Resource>(url).pipe(
            map(({ total, items, _links }) => {
                const assetFolders = items.map(item => parseAssetFolder(item));

                return new AssetFoldersDto(total, assetFolders, _links);
            }),
            pretifyError('Failed to load asset folders. Please reload.'));
    }

    public getAsset(appName: string, id: string): Observable<AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                const body = payload.body;

                return parseAsset(body);
            }),
            pretifyError('Failed to load assets. Please reload.'));
    }

    public postAssetFile(appName: string, file: File, parentId?: string): Observable<number | AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets?parentId=${parentId}`);

        return HTTP.upload(this.http, 'POST', url, file).pipe(
            filter(event =>
                event.type === HttpEventType.UploadProgress ||
                event.type === HttpEventType.Response),
            map(event => {
                if (event.type === HttpEventType.UploadProgress) {
                    const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                    return percentDone;
                } else if (Types.is(event, HttpResponse)) {
                    return parseAsset(event.body);
                } else {
                    throw 'Invalid';
                }
            }),
            catchError((error: any) => {
                if (Types.is(error, HttpErrorResponse) && error.status === 413) {
                    return throwError(new ErrorDto(413, 'Asset is too big.'));
                } else {
                    return throwError(error);
                }
            }),
            tap(value => {
                if (!Types.isNumber(value)) {
                    this.analytics.trackEvent('Asset', 'Uploaded', appName);
                }
            }),
            pretifyError('Failed to upload asset. Please reload.'));
    }

    public putAssetFile(appName: string, resource: Resource, file: File, version: Version): Observable<number | AssetDto> {
        const link = resource._links['upload'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.upload(this.http, link.method, url, file, version).pipe(
            filter(event =>
                event.type === HttpEventType.UploadProgress ||
                event.type === HttpEventType.Response),
            map(event => {
                if (event.type === HttpEventType.UploadProgress) {
                    const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                    return percentDone;
                } else if (Types.is(event, HttpResponse)) {
                    return parseAsset(event.body);
                } else {
                    throw 'Invalid';
                }
            }),
            catchError(error => {
                if (Types.is(error, HttpErrorResponse) && error.status === 413) {
                    return throwError(new ErrorDto(413, 'Asset is too big.'));
                } else {
                    return throwError(error);
                }
            }),
            tap(value => {
                if (!Types.isNumber(value)) {
                    this.analytics.trackEvent('Asset', 'Replaced', appName);
                }
            }),
            pretifyError('Failed to replace asset. Please reload.'));
    }

    public postAssetFolder(appName: string, dto: CreateAssetFolderDto): Observable<AssetFolderDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/folders`);

        return HTTP.postVersioned(this.http, url, dto).pipe(
            map(({ payload }) => {
                return parseAssetFolder(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('AssetFolder', 'Updated', appName);
            }),
            pretifyError('Failed to create asset folder. Please reload.'));
    }

    public putAsset(appName: string, resource: Resource, dto: AnnotateAssetDto, version: Version): Observable<AssetDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseAsset(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Asset', 'Updated', appName);
            }),
            pretifyError('Failed to update asset. Please reload.'));
    }

    public putAssetFolder(appName: string, resource: Resource, dto: RenameAssetFolderDto, version: Version): Observable<AssetFolderDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseAssetFolder(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('AssetFolder', 'Updated', appName);
            }),
            pretifyError('Failed to update asset folder. Please reload.'));
    }

    public putAssetItemParent(appName: string, resource: Resource, dto: MoveAssetItemDto, version: Version): Observable<Versioned<any>> {
        const link = resource._links['move'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            tap(() => {
                this.analytics.trackEvent('Asset', 'Moved', appName);
            }),
            pretifyError('Failed to move asset. Please reload.'));
    }

    public deleteAssetItem(appName: string, asset: Resource, version: Version): Observable<Versioned<any>> {
        const link = asset._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Asset', 'Deleted', appName);
            }),
            pretifyError('Failed to delete asset. Please reload.'));
    }
}

function parseAsset(response: any) {
    return new AssetDto(response._links, response._meta,
        response.id,
        DateTime.parseISO_UTC(response.created), response.createdBy,
        DateTime.parseISO_UTC(response.lastModified), response.lastModifiedBy,
        response.fileName,
        response.fileHash,
        response.fileType,
        response.fileSize,
        response.fileVersion,
        response.isProtected,
        response.parentId,
        response.mimeType,
        response.type,
        response.metadataText,
        response.metadata,
        response.slug,
        response.tags || [],
        new Version(response.version.toString()));
}

function parseAssetFolder(response: any) {
    return new AssetFolderDto(response._links,
        response.id,
        response.folderName,
        response.parentId,
        new Version(response.version.toString()));
}