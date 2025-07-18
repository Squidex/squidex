/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, HTTP, pretifyError, Resource, ScriptCompletions, Versioned, VersionOrTag } from '@app/framework';
import { AddFieldDto, ChangeCategoryDto, ConfigureFieldRulesDto, ConfigureUIFieldsDto, CreateSchemaDto, GenerateSchemaDto, GenerateSchemaResponseDto, SchemaDto, SchemasDto, SynchronizeSchemaDto, UpdateFieldDto, UpdateSchemaDto } from './../model';
import { QueryModel } from './query';

@Injectable({
    providedIn: 'root',
})
export class SchemasService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getSchemas(appName: string): Observable<SchemasDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return SchemasDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.loadFailed'));
    }

    public getSchema(appName: string, name: string): Observable<SchemaDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${name}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.loadSchemaFailed'));
    }

    public postSchema(appName: string, dto: CreateSchemaDto): Observable<SchemaDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.postVersioned(this.http, url, dto.toJSON()).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.createFailed'));
    }

    public generateSchema(appName: string, dto: GenerateSchemaDto): Observable<GenerateSchemaResponseDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/generate`);

        return HTTP.postVersioned(this.http, url, dto.toJSON()).pipe(
            map(({ payload }) => {
                return GenerateSchemaResponseDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.createFailed'));
    }

    public putScripts(appName: string, resource: Resource, dto: Record<string, string>, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['update/scripts'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.updateScriptsFailed'));
    }

    public putFieldRules(appName: string, resource: Resource, dto: ConfigureFieldRulesDto, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['update/rules'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.updateRulesFailed'));
    }

    public putSchemaSync(appName: string, resource: Resource, dto: SynchronizeSchemaDto, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['update/sync'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.synchronizeFailed'));
    }

    public putSchema(appName: string, resource: Resource, dto: UpdateSchemaDto, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, new UpdateSchemaDto(dto)).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.updateFailed'));
    }

    public putCategory(appName: string, resource: Resource, dto: ChangeCategoryDto, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['update/category'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.changeCategoryFailed'));
    }

    public putPreviewUrls(appName: string, resource: Resource, dto: Record<string, string>, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['update/urls'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.updatePreviewUrlsFailed'));
    }

    public publishSchema(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['publish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.publishFailed'));
    }

    public unpublishSchema(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['unpublish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.unpublishFailed'));
    }

    public postField(appName: string, resource: Resource, dto: AddFieldDto, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['fields/add'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.addFieldFailed'));
    }

    public putUIFields(appName: string, resource: Resource, dto: ConfigureUIFieldsDto, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['fields/ui'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.updateUIFieldsFailed'));
    }

    public putFieldOrdering(appName: string, resource: Resource, dto: ReadonlyArray<number>, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['fields/order'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, { fieldIds: dto }).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.reorderFieldsFailed'));
    }

    public putField(appName: string, resource: Resource, dto: UpdateFieldDto, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto.toJSON()).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.updateFieldFailed'));
    }

    public lockField(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['lock'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.lockFieldFailed'));
    }

    public enableField(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['enable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.enableFieldFailed'));
    }

    public disableField(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['disable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.disableFieldFailed'));
    }

    public showField(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['show'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.showFieldFailed'));
    }

    public hideField(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['hide'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.hideFieldFailed'));
    }

    public deleteField(appName: string, resource: Resource, version: VersionOrTag): Observable<SchemaDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return SchemaDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:schemas.deleteFieldFailed'));
    }

    public deleteSchema(appName: string, resource: Resource, version: VersionOrTag): Observable<Versioned<any>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            pretifyError('i18n:schemas.deleteFailed'));
    }

    public getContentScriptsCompletion(appName: string, schemaName: string): Observable<ScriptCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/completion/content-scripts`);

        return this.http.get<ScriptCompletions>(url);
    }

    public getContentTriggerCompletion(appName: string, schemaName: string): Observable<ScriptCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/completion/content-triggers`);

        return this.http.get<ScriptCompletions>(url);
    }

    public getFieldRulesCompletion(appName: string, schemaName: string): Observable<ScriptCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/completion/field-rules`);

        return this.http.get<ScriptCompletions>(url);
    }

    public getPreviewUrlsCompletion(appName: string, schemaName: string): Observable<ScriptCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/completion/preview-urls`);

        return this.http.get<ScriptCompletions>(url);
    }

    public getFilters(appName: string, schemaName: string): Observable<QueryModel> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/filters`);

        return this.http.get<QueryModel>(url);
    }
}