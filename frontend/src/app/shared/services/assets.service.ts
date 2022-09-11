/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, filter, map } from 'rxjs/operators';
import { ApiUrlConfig, DateTime, ErrorDto, getLinkUrl, hasAnyLink, HTTP, Metadata, pretifyError, Resource, ResourceLinks, StringHelper, Types, Version, Versioned } from '@app/framework';
import { AuthService } from './auth.service';
import { Query, sanitize } from './query';

const SVG_PREVIEW_LIMIT = 10 * 1024;

const MIME_TIFF = 'image/tiff';
const MIME_SVG = 'image/svg+xml';

type AssetFolderScope = 'PathAndItems' | 'Path' | 'Items';

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
            return this.fileName.substring(0, index);
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

export type AssetsDto = Readonly<{
    // The list of assets.
    items: ReadonlyArray<AssetDto>;

    // The total number of assets.
    total: number;

    // True, if the user has permissions to create an asset.
    canCreate?: boolean;

    // True, if the user has permissions to rename a tag.
    canRenameTag?: boolean;
}>;

export type AssetFoldersDto = Readonly<{
    // The list of asset folders.
    items: ReadonlyArray<AssetFolderDto>;

    // The path to the asset folders.
    path: ReadonlyArray<AssetFolderDto>;

    // True, if the user has permissions to create an asset folder.
    canCreate?: boolean;
}>;

export type AssetCompletions = ReadonlyArray<{
    // The autocompletion path.
    path: string;

    // The description of the autocompletion field.
    description: string;

    // The type of the autocompletion field.
    type: string;
 }>;

export type AnnotateAssetDto = Readonly<{
    // The optional file name.
    fileName?: string;

    // The optional flag, if an asset is protected.
    isProtected?: boolean;

    // The optiona slug.
    slug?: string;

    // The optional tags.
    tags?: ReadonlyArray<string>;

    // The optional metadata.
    metadata?: { [key: string]: any };
}>;

export type CreateAssetFolderDto = Readonly<{
    // The name of the folder.
    folderName: string;
} & MoveAssetItemDto>;

export type RenameAssetFolderDto = Readonly<{
    // The name of the folder.
    folderName: string;
}>;

export type RenameAssetTagDto = Readonly<{
    // The name of the tag.
    tagName: string;
}>;

export type MoveAssetItemDto = Readonly<{
    // The new ID to the asset folder.
    parentId?: string;
}>;

export type AssetsQuery = Readonly<{
    // True, to not return the total number of items.
    noTotal?: boolean;

    // True, to not return the total number of items, if the query would be slow.
    noSlowTotal?: boolean;
}>;

export type AssetsByRef = Readonly<{
    // The reference.
    ref: string;
}>;

export type AssetsByIds = Readonly<{
    // The IDs of the assets.
    ids: ReadonlyArray<string>;
}>;

export type AssetsByQuery = Readonly<{
    // The JSON query.
    query?: Query;

    // The number of items to skip.
    skip?: number;

    // The number of items to take.
    take?: number;

    // The tags to filter.
    tags?: ReadonlyArray<string>;

    // The ID of the asset folder.
    parentId?: string;
}>;

@Injectable()
export class AssetsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public putTag(appName: string, name: string, dto: RenameAssetTagDto): Observable<{ [name: string]: number }> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/tags/${encodeURIComponent(name)}`);

        return this.http.put<{ [name: string]: number }>(url, dto).pipe(
            pretifyError('i18n:assets.renameTagFailed'));
    }

    public getTags(appName: string): Observable<{ [name: string]: number }> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/tags`);

        return this.http.get<{ [name: string]: number }>(url).pipe(
            pretifyError('i18n:assets.loadTagsFailed'));
    }

    public getAssets(appName: string, q?: AssetsQuery & (AssetsByQuery | AssetsByIds | AssetsByRef)): Observable<AssetsDto> {
        const body = buildQuery(q as any);

        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/query`);

        return this.http.post<any>(url, body, buildHeaders(q, q?.['ref'])).pipe(
            map(body => {
                return parseAssets(body);
            }),
            pretifyError('i18n:assets.loadFailed'));
    }

    public getAssetFolders(appName: string, parentId: string, scope: AssetFolderScope): Observable<AssetFoldersDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/folders?parentId=${parentId}&scope=${scope}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseAssetFolders(body);
            }),
            pretifyError('i18n:assets.loadFoldersFailed'));
    }

    public getAsset(appName: string, id: string): Observable<AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return parseAsset(payload.body);
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
            pretifyError('i18n:assets.replaceFailed'));
    }

    public postAssetFolder(appName: string, dto: CreateAssetFolderDto): Observable<AssetFolderDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/folders`);

        return HTTP.postVersioned(this.http, url, dto).pipe(
            map(({ payload }) => {
                return parseAssetFolder(payload.body);
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
            pretifyError('i18n:assets.updateFailed'));
    }

    public putAssetFolder(appName: string, resource: Resource, dto: RenameAssetFolderDto, version: Version): Observable<AssetFolderDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseAssetFolder(payload.body);
            }),
            pretifyError('i18n:assets.updateFolderFailed'));
    }

    public putAssetItemParent(appName: string, resource: Resource, dto: MoveAssetItemDto, version: Version): Observable<Versioned<any>> {
        const link = resource._links['move'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            pretifyError('i18n:assets.moveFailed'));
    }

    public deleteAssetItem(appName: string, asset: Resource, checkReferrers: boolean, version: Version): Observable<Versioned<any>> {
        const link = asset._links['delete'];

        const url = `${this.apiUrl.buildUrl(link.href)}?checkReferrers=${checkReferrers}`;

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            pretifyError('i18n:assets.deleteFailed'));
    }

    public getCompletions(appName: string): Observable<AssetCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/completion`);

        return this.http.get<AssetCompletions>(url);
    }
}

function buildHeaders(q: AssetsQuery | undefined, noTotal: boolean) {
    let options = {
        headers: {},
    };

    if (q?.noTotal || noTotal) {
        options.headers['X-NoTotal'] = '1';
    }

    if (q?.noSlowTotal) {
        options.headers['X-NoSlowTotal'] = '1';
    }

    return options;
}

function buildQuery(q?: AssetsQuery & AssetsByQuery & AssetsByIds & AssetsByRef) {
    const { ids, parentId, query, ref, skip, tags, take } = q || {};

    const body: any = {};

    if (parentId) {
        body.parentId = parentId;
    }

    if (ref) {
        const queryObj: Query = {
            filter: {
                or: [{
                    path: 'id',
                    op: 'eq',
                    value: ref,
                }, {
                    path: 'slug',
                    op: 'eq',
                    value: ref,
                }],
            },
            take: 1,
        };

        body.q = sanitize(queryObj);
    } else if (Types.isArray(ids)) {
        body.ids = ids;
    } else {
        const queryObj: Query = {};

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

        body.q = sanitize(queryObj);
    }

    return body;
}

function parseAssets(response: { items: any[]; total: number } & Resource): AssetsDto {
    const { items: list, total, _links } = response;
    const items = list.map(parseAsset);

    const canCreate = hasAnyLink(_links, 'create');
    const canRenameTag = hasAnyLink(_links, 'tags/rename');

    return { items, total, canCreate, canRenameTag };
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

function parseAssetFolders(response: { items: any[]; path: any[]; total: number } & Resource): AssetFoldersDto {
    const { items: list, _links } = response;
    const items = list.map(parseAssetFolder);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, canCreate, path: response.path.map(parseAssetFolder) };
}

function parseAssetFolder(response: any) {
    return new AssetFolderDto(response._links,
        response.id,
        response.folderName,
        response.parentId,
        new Version(response.version.toString()));
}
