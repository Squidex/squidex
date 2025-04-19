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
import { ApiUrlConfig, ErrorDto, HTTP, pretifyError, Resource, ScriptCompletions, StringHelper, Types, Versioned, VersionOrTag } from '@app/framework';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, AssetFoldersDto, AssetsDto, CreateAssetFolderDto, MoveAssetDto, MoveAssetFolderDto, RenameAssetFolderDto, RenameTagDto } from './../model';
import { Query, sanitize } from './query';

type AssetFolderScope = 'PathAndItems' | 'Path' | 'Items';

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

@Injectable({
    providedIn: 'root',
})
export class AssetsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public putTag(appName: string, name: string, dto: RenameTagDto): Observable<{ [name: string]: number }> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/tags/${encodeURIComponent(name)}`);

        return this.http.put<{ [name: string]: number }>(url, dto.toJSON()).pipe(
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

        return this.http.post<any>(url, body, buildHeaders(q, (q as any)?.ref)).pipe(
            map(body => {
                return AssetsDto.fromJSON(body);
            }),
            pretifyError('i18n:assets.loadFailed'));
    }

    public getAssetFolders(appName: string, parentId: string, scope: AssetFolderScope): Observable<AssetFoldersDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/folders${StringHelper.buildQuery({ parentId, scope })}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return AssetFoldersDto.fromJSON(body);
            }),
            pretifyError('i18n:assets.loadFoldersFailed'));
    }

    public getAsset(appName: string, id: string): Observable<AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/${id}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return AssetDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:assets.loadFailed'));
    }

    public postAssetFile(appName: string, file: HTTP.UploadFile, parentId?: string): Observable<number | AssetDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets${StringHelper.buildQuery({ parentId })}`);

        return HTTP.upload(this.http, 'POST', url, file).pipe(
            filter(event =>
                event.type === HttpEventType.UploadProgress ||
                event.type === HttpEventType.Response),
            map(event => {
                if (event.type === HttpEventType.UploadProgress) {
                    const percentDone = event.total ? Math.round(100 * event.loaded / event.total) : 0;

                    return percentDone;
                } else if (Types.is(event, HttpResponse)) {
                    return AssetDto.fromJSON(event.body);
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

    public putAssetFile(appName: string, resource: Resource, file: HTTP.UploadFile, version: VersionOrTag): Observable<number | AssetDto> {
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
                    return AssetDto.fromJSON(event.body);
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

        return HTTP.postVersioned(this.http, url, dto.toJSON()).pipe(
            map(({ payload }) => {
                return AssetFolderDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:assets.createFolderFailed'));
    }

    public putAsset(appName: string, resource: Resource, dto: AnnotateAssetDto, version: VersionOrTag): Observable<AssetDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return AssetDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:assets.updateFailed'));
    }

    public putAssetFolder(appName: string, resource: Resource, dto: RenameAssetFolderDto, version: VersionOrTag): Observable<AssetFolderDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return AssetFolderDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:assets.updateFolderFailed'));
    }

    public putAssetParent(appName: string, resource: Resource, dto: MoveAssetDto, version: VersionOrTag): Observable<AssetDto> {
        const link = resource._links['move'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return AssetDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:assets.moveFailed'));
    }

    public putAssetFolderParent(appName: string, resource: Resource, dto: MoveAssetFolderDto, version: VersionOrTag): Observable<AssetFolderDto> {
        const link = resource._links['move'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return AssetFolderDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:assets.moveFailed'));
    }

    public deleteAssetItem(appName: string, asset: Resource, checkReferrers: boolean, version: VersionOrTag): Observable<Versioned<any>> {
        const link = asset._links['delete'];

        const url = `${this.apiUrl.buildUrl(link.href)}${StringHelper.buildQuery({ checkReferrers })}`;

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            pretifyError('i18n:assets.deleteFailed'));
    }

    public getCompletions(appName: string): Observable<ScriptCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/assets/completion`);

        return this.http.get<ScriptCompletions>(url);
    }
}

function buildHeaders(q: AssetsQuery | undefined, noTotal: boolean) {
    let options = {
        headers: {} as Record<string, string>,
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

        return { q: sanitize(queryObj) };
    } else if (Types.isArray(ids)) {
        return { ids };
    } else {
        const queryObj: Query = {};
        const queryFilters: any[] = [];

        if (query && query.fullText && query.fullText.length > 0) {
            queryFilters.push({ path: 'fileName', op: 'contains', value: query.fullText });
        }

        if (tags) {
            for (const tag of tags) {
                if (tag && tag.length > 0) {
                    queryFilters.push({ path: 'tags', op: 'eq', value: tag });
                }
            }
        }

        if (queryFilters.length > 0) {
            queryObj.filter = { and: queryFilters };
        }

        if (take && take > 0) {
            queryObj.take = take;
        }

        if (skip && skip > 0) {
            queryObj.skip = skip;
        }

        return { q: sanitize(queryObj), parentId };
    }
}