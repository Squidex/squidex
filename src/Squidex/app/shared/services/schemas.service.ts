/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP,
    mapVersioned,
    Model,
    pretifyError,
    StringHelper,
    Version,
    Versioned
} from '@app/framework';

import { createProperties, FieldPropertiesDto } from './schemas.types';

export class SchemaDto extends Model<SchemaDto> {
    public get displayName() {
        return StringHelper.firstNonEmpty(this.properties.label, this.name);
    }

    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly category: string,
        public readonly properties: SchemaPropertiesDto,
        public readonly isSingleton: boolean,
        public readonly isPublished: boolean,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version
    ) {
        super();
    }
}

export class SchemaDetailsDto extends SchemaDto {
    public listFields: RootFieldDto[];
    public listFieldsEditable: RootFieldDto[];

    constructor(id: string, name: string, category: string,
        properties: SchemaPropertiesDto,
        isSingleton: boolean,
        isPublished: boolean,
        created: DateTime,
        createdBy: string,
        lastModified: DateTime,
        lastModifiedBy: string,
        version: Version,
        public readonly fields: RootFieldDto[] = [],
        public readonly scripts = {},
        public readonly previewUrls = {}
    ) {
        super(id, name, category, properties, isSingleton, isPublished, created, createdBy, lastModified, lastModifiedBy, version);

        this.onCloned();
    }

    protected onCloned() {
        if (this.fields) {
            let fields = this.fields.filter(x => x.properties.isListField && x.properties.isContentField);

            if (fields.length === 0 && this.fields.length > 0) {
                fields = [this.fields[0]];
            }

            if (fields.length === 0) {
                fields = NONE_FIELDS;
            }

            this.listFields = fields;
            this.listFieldsEditable = fields.filter(x => x.isInlineEditable);
        }
    }

    public with(value: Partial<SchemaDetailsDto>): SchemaDetailsDto {
        return this.clone(value);
    }
}

export class FieldDto extends Model<FieldDto> {
    public get isInlineEditable(): boolean {
        return !this.isDisabled && this.properties['inlineEditable'] === true;
    }

    public get displayName() {
        return StringHelper.firstNonEmpty(this.properties.label, this.name);
    }

    public get displayPlaceholder() {
        return this.properties.placeholder || '';
    }

    constructor(
        public readonly fieldId: number,
        public readonly name: string,
        public readonly properties: FieldPropertiesDto,
        public readonly isLocked: boolean = false,
        public readonly isHidden: boolean = false,
        public readonly isDisabled: boolean = false
    ) {
        super();
    }
}

export class RootFieldDto extends FieldDto {
    public get isLocalizable() {
        return this.partitioning === 'language';
    }

    public get isArray() {
        return this.properties.fieldType === 'Array';
    }

    public get isString() {
        return this.properties.fieldType === 'String';
    }

    public get isTranslatable() {
        return this.isLocalizable && this.isString && (this.properties.editor === 'Input' || this.properties.editor === 'Textarea');
    }

    constructor(fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly partitioning: string,
        isLocked: boolean = false,
        isHidden: boolean = false,
        isDisabled: boolean = false,
        public readonly nested: NestedFieldDto[] = []
    ) {
        super(fieldId, name, properties, isLocked, isHidden, isDisabled);
    }

    public with(value: Partial<RootFieldDto>): RootFieldDto {
        return this.clone(value);
    }
}

const NONE_FIELD = new RootFieldDto(-1, '', createProperties('String'), 'invariant');
const NONE_FIELDS = [NONE_FIELD];

export class NestedFieldDto extends FieldDto {
    constructor(fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly parentId: number,
        isLocked: boolean = false,
        isHidden: boolean = false,
        isDisabled: boolean = false
    ) {
        super(fieldId, name, properties, isLocked, isHidden, isDisabled);
    }

    public with(value: Partial<NestedFieldDto>): NestedFieldDto {
        return this.clone(value);
    }
}

export class SchemaPropertiesDto {
    constructor(
        public readonly label?: string,
        public readonly hints?: string
    ) {
    }
}

export interface AddFieldDto {
    readonly name: string;
    readonly partitioning: string;
    readonly properties: FieldPropertiesDto;
}

export interface CreateSchemaDto {
    readonly name: string;
    readonly fields?: RootFieldDto[];
    readonly properties?: SchemaPropertiesDto;
    readonly isSingleton?: boolean;
}

export interface SchemaCreatedDto {
    readonly id: string;
}

export interface UpdateSchemaCategoryDto {
    readonly name?: string;
}

export interface UpdateFieldDto {
    readonly properties: FieldPropertiesDto;
}

export interface UpdateSchemaDto {
    readonly label?: string;
    readonly hints?: string;
}

@Injectable()
export class SchemasService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getSchemas(appName: string): Observable<SchemaDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
            map(({ payload }) => {
                const body = payload.body;

                const items: any[] = body;

                const schemas = items.map(item => {
                    const properties = new SchemaPropertiesDto(item.properties.label, item.properties.hints);

                    return new SchemaDto(
                        item.id,
                        item.name,
                        item.category, properties,
                        item.isSingleton,
                        item.isPublished,
                        DateTime.parseISO_UTC(item.created), item.createdBy,
                        DateTime.parseISO_UTC(item.lastModified), item.lastModifiedBy,
                        new Version(item.version.toString()));
                });

                return schemas;
            }),
            pretifyError('Failed to load schemas. Please reload.'));
    }

    public getSchema(appName: string, id: string): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${id}`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
            map(({ version, payload }) => {
                const body = payload.body;

                const fields = body.fields.map((item: any) => {
                    const propertiesDto =
                        createProperties(
                            item.properties.fieldType,
                            item.properties);

                    let nested: NestedFieldDto[] | null = null;

                    if (item.nested && item.nested.length > 0) {
                        nested = item.nested.map((nestedItem: any) => {
                            const nestedPropertiesDto =
                                createProperties(
                                    nestedItem.properties.fieldType,
                                    nestedItem.properties);

                            return new NestedFieldDto(
                                nestedItem.fieldId,
                                nestedItem.name,
                                nestedPropertiesDto,
                                item.fieldId,
                                nestedItem.isLocked,
                                nestedItem.isHidden,
                                nestedItem.isDisabled);
                        });
                    }

                    return new RootFieldDto(
                        item.fieldId,
                        item.name,
                        propertiesDto,
                        item.partitioning,
                        item.isLocked,
                        item.isHidden,
                        item.isDisabled,
                        nested || []);
                });

                const properties = new SchemaPropertiesDto(body.properties.label, body.properties.hints);

                return new SchemaDetailsDto(
                    body.id,
                    body.name,
                    body.category,
                    properties,
                    body.isSingleton,
                    body.isPublished,
                    DateTime.parseISO_UTC(body.created), body.createdBy,
                    DateTime.parseISO_UTC(body.lastModified), body.lastModifiedBy,
                    version,
                    fields,
                    body.scripts || {},
                    body.previewUrls || {});
            }),
            pretifyError('Failed to load schema. Please reload.'));
    }

    public postSchema(appName: string, dto: CreateSchemaDto): Observable<Versioned<SchemaCreatedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.postVersioned<any>(this.http, url, dto).pipe(
            mapVersioned(({ body }) => body!),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Created', appName);
            }),
            pretifyError('Failed to create schema. Please reload.'));
    }

    public deleteSchema(appName: string, schemaName: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'Deleted', appName);
            }),
            pretifyError('Failed to delete schema. Please reload.'));
    }

    public putScripts(appName: string, schemaName: string, dto: {}, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/scripts`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'ScriptsConfigured', appName);
            }),
            pretifyError('Failed to update schema scripts. Please reload.'));
    }

    public putSchema(appName: string, schemaName: string, dto: UpdateSchemaDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'Updated', appName);
            }),
            pretifyError('Failed to update schema. Please reload.'));
    }

    public publishSchema(appName: string, schemaName: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/publish`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'Published', appName);
            }),
            pretifyError('Failed to publish schema. Please reload.'));
    }

    public unpublishSchema(appName: string, schemaName: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/unpublish`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'Unpublished', appName);
            }),
            pretifyError('Failed to unpublish schema. Please reload.'));
    }

    public putCategory(appName: string, schemaName: string, dto: UpdateSchemaCategoryDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/category`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'CategoryChanged', appName);
            }),
            pretifyError('Failed to change category. Please reload.'));
    }

    public putPreviewUrls(appName: string, schemaName: string, dto: {}, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/preview-urls`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'PreviewUrlsConfigured', appName);
            }),
            pretifyError('Failed to configure preview urls. Please reload.'));
    }

    public postField(appName: string, schemaName: string, dto: AddFieldDto, parentId: number | undefined, version: Version): Observable<Versioned<RootFieldDto | NestedFieldDto>> {
        const url = this.buildUrl(appName, schemaName, parentId, '');

        return HTTP.postVersioned<any>(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                if (parentId) {
                    const field = new NestedFieldDto(body.id, dto.name, dto.properties, parentId);

                    return field;
                } else {
                    const field = new RootFieldDto(body.id, dto.name, dto.properties, dto.partitioning);

                    return field;
                }
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldCreated', appName);
            }),
            pretifyError('Failed to add field. Please reload.'));
    }

    public putFieldOrdering(appName: string, schemaName: string, dto: number[], parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, '/ordering');

        return HTTP.putVersioned(this.http, url, { fieldIds: dto }, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldsReordered', appName);
            }),
            pretifyError('Failed to reorder fields. Please reload.'));
    }

    public putField(appName: string, schemaName: string, fieldId: number, dto: UpdateFieldDto, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldUpdated', appName);
            }),
            pretifyError('Failed to update field. Please reload.'));
    }

    public lockField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/lock`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldLocked', appName);
            }),
            pretifyError('Failed to lock field. Please reload.'));
    }

    public enableField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/enable`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldEnabled', appName);
            }),
            pretifyError('Failed to enable field. Please reload.'));
    }

    public disableField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/disable`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldDisabled', appName);
            }),
            pretifyError('Failed to disable field. Please reload.'));
    }

    public showField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/show`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldShown', appName);
            }),
            pretifyError('Failed to show field. Please reload.'));
    }

    public hideField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/hide`);

        return HTTP.putVersioned(this.http, url, {}, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldHidden', appName);
            }),
            pretifyError('Failed to hide field. Please reload.'));
    }

    public deleteField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldDeleted', appName);
            }),
            pretifyError('Failed to delete field. Please reload.'));
    }

    private buildUrl(appName: string, schemaName: string, parentId: number | undefined, suffix: string) {
        const url =
            parentId ?
                this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${parentId}/nested${suffix}`) :
                this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields${suffix}`);

        return url;
    }
}