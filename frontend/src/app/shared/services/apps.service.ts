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
import { ApiUrlConfig, ErrorDto, HTTP, mapVersioned, pretifyError, Resource, Types, Versioned, VersionOrTag } from '@app/framework';
import { AppDto, AppSettingsDto, AssetScriptsDto, CreateAppDto, ICreateAppDto, ITransferToTeamDto, IUpdateAppDto, IUpdateAppSettingsDto, IUpdateAssetScriptsDto, TransferToTeamDto, UpdateAppDto, UpdateAppSettingsDto, UpdateAssetScriptsDto } from './../model';

@Injectable({
    providedIn: 'root',
})
export class AppsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getApps(): Observable<ReadonlyArray<AppDto>> {
        const url = this.apiUrl.buildUrl('/api/apps');

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const apps = body.map(AppDto.fromJSON);

                return apps;
            }),
            pretifyError('i18n:apps.loadFailed'));
    }

    public getTeamApps(teamId: string): Observable<ReadonlyArray<AppDto>> {
        const url = this.apiUrl.buildUrl(`/api/teams/${teamId}/apps`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const apps = body.map(AppDto.fromJSON);

                return apps;
            }),
            pretifyError('i18n:apps.loadFailed'));
    }

    public getApp(appName: string): Observable<AppDto> {
        const url = this.apiUrl.buildUrl(`/api/apps/${appName}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const app = AppDto.fromJSON(body);

                return app;
            }),
            pretifyError('i18n:apps.appLoadFailed'));
    }

    public postApp(dto: ICreateAppDto): Observable<AppDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return this.http.post(url, new CreateAppDto(dto).toJSON()).pipe(
            map(body => {
                return AppDto.fromJSON(body);
            }),
            pretifyError('i18n:apps.createFailed'));
    }

    public putApp(appName: string, resource: Resource, dto: IUpdateAppDto, version: VersionOrTag): Observable<AppDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, new UpdateAppDto(dto).toJSON()).pipe(
            map(({ payload }) => {
                return AppDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:apps.updateFailed'));
    }

    public transferApp(appName: string, resource: Resource, dto: ITransferToTeamDto, version: VersionOrTag): Observable<AppDto> {
        const link = resource._links['transfer'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, new TransferToTeamDto(dto).toJSON()).pipe(
            map(({ payload }) => {
                return AppDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:apps.transferFailed'));
    }

    public getSettings(appName: string): Observable<AppSettingsDto> {
        const url = this.apiUrl.buildUrl(`/api/apps/${appName}/settings`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return AppSettingsDto.fromJSON(body);
            }),
            pretifyError('i18n:apps.loadSettingsFailed'));
    }

    public putSettings(appName: string, resource: Resource, dto: IUpdateAppSettingsDto, version: VersionOrTag): Observable<AppSettingsDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, new UpdateAppSettingsDto(dto).toJSON()).pipe(
            map(({ payload }) => {
                return AppSettingsDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:apps.updateSettingsFailed'));
    }

    public getAssetScripts(appName: string): Observable<Versioned<AssetScriptsDto>> {
        const url = this.apiUrl.buildUrl(`/api/apps/${appName}/assets/scripts`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return AssetScriptsDto.fromJSON(body);
            }),
            pretifyError('i18n:apps.loadAssetScriptsFailed'));
    }

    public putAssetScripts(appName: string, resource: Resource, dto: IUpdateAssetScriptsDto, version: VersionOrTag): Observable<Versioned<AssetScriptsDto>> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, new UpdateAssetScriptsDto(dto).toJSON()).pipe(
            mapVersioned(({ body }) => {
                return AssetScriptsDto.fromJSON(body);
            }),
            pretifyError('i18n:apps.updateAssetScriptsFailed'));
    }

    public postAppImage(appName: string, resource: Resource, file: HTTP.UploadFile, version: VersionOrTag): Observable<number | AppDto> {
        const link = resource._links['image/upload'];

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
                    return AppDto.fromJSON(event.body);
                } else {
                    throw new Error('Invalid');
                }
            }),
            catchError(error => {
                if (Types.is(error, HttpErrorResponse) && error.status === 413) {
                    return throwError(() => new ErrorDto(413, 'i18n:apps.uploadImageTooBig'));
                } else {
                    return throwError(() => error);
                }
            }),
            pretifyError('i18n:apps.uploadImageFailed'));
    }

    public deleteAppImage(appName: string, resource: Resource, version: VersionOrTag): Observable<AppDto> {
        const link = resource._links['image/delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            map(({ payload }) => {
                return AppDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:apps.removeImageFailed'));
    }

    public leaveApp(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['leave'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:apps.leaveFailed'));
    }

    public deleteApp(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:apps.archiveFailed'));
    }
}
