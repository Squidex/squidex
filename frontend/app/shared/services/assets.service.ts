/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, ErrorDto, getLinkUrl, hasAnyLink, HTTP, Metadata, pretifyError, Resource, ResourceLinks, ResultSet, StringHelper, Types, Version, Versioned } from '@app/framework';
import { Observable, throwError } from 'rxjs';
import { catchError, filter, map, tap } from 'rxjs/operators';
import { encodeQuery, Query } from './../state/query';
import { AuthService } from './auth.service';

const SVG_PREVIEW_LIMIT = 10 * 1024;

const MIME_TIFF = 'image/tiff';
const MIME_SVG = 'image/svg+xml';

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
        return getLinkUrl(this, 'content');
    }

    public get fileNameWithoutExtension() {
        const index = this.fileName.lastIndexOf('.');

        if (index > 0) {
            return this.fileName.substr(0, index);
        } else {
            return this.fileName;
        }
    }

    constructor(links: ResourceLinks, meta: Metadata,
        public readonly id: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version,
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
    ) {
        this.canPreview =
            (this.mimeType !== MIME_TIFF && this.type === 'Image') ||
            (this.mimeType === MIME_SVG && this.fileSize < SVG_PREVIEW_LIMIT);

        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canUpload = hasAnyLink(links, 'upload');
        this.canMove = hasAnyLink(links, 'move');

        this._meta = meta;
    }

    public fullUrl(apiUrl: ApiUrlConfig, authService?: AuthService) {
        let url = apiUrl.buildUrl(this.contentUrl);

        if (this.isProtected && authService && authService.user) {
            url = StringHelper.appendToUrl(url, 'access_token', authService.user.accessToken);
        }

        return url;
    }
}

export class AssetFoldersDto extends ResultSet<AssetFolderDto> {
    constructor(total: number, items: ReadonlyArray<AssetFolderDto>,
        public readonly path: ReadonlyArray<AssetFolderDto>,
        links?: ResourceLinks,
    ) {
        super(total, items, links);
    }

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
        public readonly version: Version,
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canMove = hasAnyLink(links, 'move');
    }
}

type Tags = readonly string[];

type AssetFolderScope = 'PathAndItems' | 'Path' | 'Items';
type AssetMetadata = { [key: string]: any };

export type AssetCompletions =
    ReadonlyArray<{ path: string; description: string; type: string }>;

export type AnnotateAssetDto =
    Readonly<{ fileName?: string; isProtected?: boolean; slug?: string; tags?: Tags; metadata?: AssetMetadata }>;

export type CreateAssetFolderDto =
    Readonly<{ folderName: string } & MoveAssetItemDto>;

export type RenameAssetFolderDto =
    Readonly<{ folderName: string }>;

export type MoveAssetItemDto =
    Readonly<{ parentId?: string }>;

export type AssetQueryDto =
    Readonly<{ ids?: Tags; maxLength?: number; parentId?: string; query?: Query; skip?: number; tags?: Tags; take?: number }>;

@Injectable()
export class AssetsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getTags(appName: string): Observable<{ [name: string]: number }> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/tags`);

        return this.http.get<{ [name: string]: number }>(url);
    }

    public getAssets(appName: string, q?: AssetQueryDto): Observable<AssetsDto> {
        const { ids, maxLength, parentId, query, skip, tags, take } = q || {};

        let fullQuery: string;

        let queryObj: Query | undefined;

        if (ids && ids.length > 0) {
            fullQuery = `ids=${ids.join(',')}`;
        } else {
            queryObj = {};

            const filters: any[] = [];

            if (query && query.fullText && query.fullText.length > 0) {
                filters.push({ path: 'fileName', op: 'contains', value: query.fullText });
            }

            if (tags) {
                for (const tag of tags) {
                    if (tag && tag.length > 0) {
                        filters.push({ path: 'tags', op: 'eq', value: tag });
                    }
                }
            }

            if (filters.length > 0) {
                queryObj.filter = { and: filters };
            }

            if (take && take > 0) {
                queryObj.take = take;
            }

            if (skip && skip > 0) {
                queryObj.skip = skip;
            }

            fullQuery = `q=${encodeQuery(queryObj)}`;

            if (parentId) {
                fullQuery = StringHelper.appendToUrl(fullQuery, 'parentId', parentId, true);
            }
        }

        if (fullQuery.length > (maxLength || 2000)) {
            const body: any = {};

            if (ids && ids.length > 0) {
                body.ids = ids;
            } else if (queryObj) {
                body.q = queryObj;
            }

            if (parentId) {
                body.parentId = parentId;
            }

            const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/query`);

            return this.http.post<{ total: number; items: any[]; folders: any[] } & Resource>(url, body).pipe(
                map(({ total, items, _links }) => {
                    const assets = items.map(parseAsset);

                    return new AssetsDto(total, assets, _links);
                }),
                pretifyError('i18n:assets.loadFailed'));
        } else {
            const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets?${fullQuery}`);

            return this.http.get<{ total: number; items: any[]; folders: any[] } & Resource>(url).pipe(
                map(({ total, items, _links }) => {
                    const assets = items.map(parseAsset);

                    return new AssetsDto(total, assets, _links);
                }),
                pretifyError('i18n:assets.loadFailed'));
        }
    }

    public getAssetFolders(appName: string, parentId: string, scope: AssetFolderScope): Observable<AssetFoldersDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/folders?parentId=${parentId}&scope=${scope}`);

        return this.http.get<{ total: number; items: any[]; folders: any[]; path: any[] } & Resource>(url).pipe(
            map(({ total, items, path, _links }) => {
                const assetFolders = items.map(parseAssetFolder);
                const assetPath = path.map(parseAssetFolder);

                return new AssetFoldersDto(total, assetFolders, assetPath, _links);
            }),
            pretifyError('i18n:assets.loadFoldersFailed'));
    }

    public getAsset(appName: string, id: string): Observable<AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                const body = payload.body;

                return parseAsset(body);
            }),
            pretifyError('i18n:assets.loadFailed'));
    }

    public postAssetFile(appName: string, file: Blob, parentId?: string): Observable<number | AssetDto> {
        let url = this.apiUrl.buildUrl(`api/apps/${appName}/assets`);

        if (parentId) {
            url += `?parentId=${parentId}`;
        }

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
                    throw new Error('Invalid');
                }
            }),
            catchError((error: any) => {
                if (Types.is(error, HttpErrorResponse) && error.status === 413) {
                    return throwError(() => new ErrorDto(413, 'i18n:assets.fileTooBig'));
                } else {
                    return throwError(() => error);
                }
            }),
            tap(value => {
                if (!Types.isNumber(value)) {
                    this.analytics.trackEvent('Asset', 'Uploaded', appName);
                }
            }),
            pretifyError('i18n:assets.uploadFailed'));
    }

    public putAssetFile(appName: string, resource: Resource, file: Blob, version: Version): Observable<number | AssetDto> {
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
                    throw new Error('Invalid');
                }
            }),
            catchError(error => {
                if (Types.is(error, HttpErrorResponse) && error.status === 413) {
                    return throwError(() => new ErrorDto(413, 'i18n:assets.fileTooBig'));
                } else {
                    return throwError(() => error);
                }
            }),
            tap(value => {
                if (!Types.isNumber(value)) {
                    this.analytics.trackEvent('Asset', 'Replaced', appName);
                }
            }),
            pretifyError('i18n:assets.replaceFailed'));
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
            pretifyError('i18n:assets.createFolderFailed'));
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
            pretifyError('i18n:assets.updateFailed'));
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
            pretifyError('i18n:assets.updateFolderFailed'));
    }

    public putAssetItemParent(appName: string, resource: Resource, dto: MoveAssetItemDto, version: Version): Observable<Versioned<any>> {
        const link = resource._links['move'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            tap(() => {
                this.analytics.trackEvent('Asset', 'Moved', appName);
            }),
            pretifyError('i18n:assets.moveFailed'));
    }

    public deleteAssetItem(appName: string, asset: Resource, checkReferrers: boolean, version: Version): Observable<Versioned<any>> {
        const link = asset._links['delete'];

        const url = `${this.apiUrl.buildUrl(link.href)}?checkReferrers=${checkReferrers}`;

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Asset', 'Deleted', appName);
            }),
            pretifyError('i18n:assets.deleteFailed'));
    }

    public getCompletions(appName: string): Observable<AssetCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/completion`);

        return this.http.get<AssetCompletions>(url);
    }
}

function parseAsset(response: any) {
    return new AssetDto(response._links, response._meta,
        response.id,
        DateTime.parseISO(response.created), response.createdBy,
        DateTime.parseISO(response.lastModified), response.lastModifiedBy,
        new Version(response.version.toString()),
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
        response.tags || []);
}

function parseAssetFolder(response: any) {
    return new AssetFolderDto(response._links,
        response.id,
        response.folderName,
        response.parentId,
        new Version(response.version.toString()));
}
