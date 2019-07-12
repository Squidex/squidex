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
    hasAnyLink,
    HTTP,
    pretifyError,
    Resource,
    ResourceLinks,
    StringHelper,
    Version,
    Versioned
} from '@app/framework';

import { createProperties, FieldPropertiesDto } from './schemas.types';

export type SchemasDto = {
    readonly items: SchemaDto[];

    readonly canCreate: boolean;
} & Resource;

export class SchemaDto {
    public readonly _links: ResourceLinks;

    public readonly canAddField: boolean;
    public readonly canDelete: boolean;
    public readonly canOrderFields: boolean;
    public readonly canPublish: boolean;
    public readonly canReadContents: boolean;
    public readonly canUnpublish: boolean;
    public readonly canUpdate: boolean;
    public readonly canUpdateCategory: boolean;
    public readonly canUpdateScripts: boolean;
    public readonly canUpdateUrls: boolean;

    public readonly displayName: string;

    constructor(links: ResourceLinks,
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
        this._links = links;

        this.canAddField = hasAnyLink(links, 'fields/add');
        this.canDelete = hasAnyLink(links, 'delete');
        this.canOrderFields = hasAnyLink(links, 'fields/order');
        this.canPublish = hasAnyLink(links, 'publish');
        this.canReadContents = hasAnyLink(links, 'contents');
        this.canUnpublish = hasAnyLink(links, 'unpublish');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canUpdateCategory = hasAnyLink(links, 'update/category');
        this.canUpdateScripts = hasAnyLink(links, 'update/scripts');
        this.canUpdateUrls = hasAnyLink(links, 'update/urls');

        this.displayName = StringHelper.firstNonEmpty(this.properties.label, this.name);
    }
}

export class SchemaDetailsDto extends SchemaDto {
    public listFields: RootFieldDto[];
    public listFieldsEditable: RootFieldDto[];

    constructor(links: ResourceLinks, id: string, name: string, category: string,
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
        super(links, id, name, category, properties, isSingleton, isPublished, created, createdBy, lastModified, lastModifiedBy, version);

        if (fields) {
            let listFields = this.fields.filter(x => x.properties.isListField && x.properties.isContentField);

            if (listFields.length === 0 && this.fields.length > 0) {
                listFields = [this.fields[0]];
            }

            if (listFields.length === 0) {
                listFields = NONE_FIELDS;
            }

            this.listFields = listFields;
            this.listFieldsEditable = listFields.filter(x => x.isInlineEditable);
        }
    }
}

export class FieldDto {
    public readonly _links: ResourceLinks;

    public readonly canAddField: boolean;
    public readonly canDelete: boolean;
    public readonly canDisable: boolean;
    public readonly canEnable: boolean;
    public readonly canHide: boolean;
    public readonly canLock: boolean;
    public readonly canOrderFields: boolean;
    public readonly canShow: boolean;
    public readonly canUpdate: boolean;

    public get isInlineEditable(): boolean {
        return !this.isDisabled && this.properties['inlineEditable'] === true;
    }

    public get displayName() {
        return StringHelper.firstNonEmpty(this.properties.label, this.name);
    }

    public get displayPlaceholder() {
        return this.properties.placeholder || '';
    }

    constructor(links: ResourceLinks,
        public readonly fieldId: number,
        public readonly name: string,
        public readonly properties: FieldPropertiesDto,
        public readonly isLocked: boolean = false,
        public readonly isHidden: boolean = false,
        public readonly isDisabled: boolean = false
    ) {
        this._links = links;

        this.canAddField = hasAnyLink(links, 'fields/add');
        this.canDelete = hasAnyLink(links, 'delete');
        this.canDisable = hasAnyLink(links, 'disable');
        this.canEnable = hasAnyLink(links, 'enable');
        this.canOrderFields = hasAnyLink(links, 'fields/order');
        this.canHide = hasAnyLink(links, 'hide');
        this.canLock = hasAnyLink(links, 'lock');
        this.canShow = hasAnyLink(links, 'show');
        this.canUpdate = hasAnyLink(links, 'update');
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

    constructor(links: ResourceLinks, fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly partitioning: string,
        isLocked: boolean = false,
        isHidden: boolean = false,
        isDisabled: boolean = false,
        public readonly nested: NestedFieldDto[] = []
    ) {
        super(links, fieldId, name, properties, isLocked, isHidden, isDisabled);
    }
}

const NONE_FIELD = new RootFieldDto({}, -1, '', createProperties('String'), 'invariant');
const NONE_FIELDS = [NONE_FIELD];

export class NestedFieldDto extends FieldDto {
    constructor(links: ResourceLinks, fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly parentId: number,
        isLocked: boolean = false,
        isHidden: boolean = false,
        isDisabled: boolean = false
    ) {
        super(links, fieldId, name, properties, isLocked, isHidden, isDisabled);
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
    readonly partitioning?: string;
    readonly properties: FieldPropertiesDto;
}

export interface CreateSchemaDto {
    readonly name: string;
    readonly fields?: RootFieldDto[];
    readonly properties?: SchemaPropertiesDto;
    readonly isSingleton?: boolean;
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

    public getSchemas(appName: string): Observable<SchemasDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return parseSchemas(payload.body);
            }),
            pretifyError('Failed to load schemas. Please reload.'));
    }

    public getSchema(appName: string, name: string): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${name}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ version, payload }) => {
                return parseSchemaWithDetails(payload.body, version);
            }),
            pretifyError('Failed to load schema. Please reload.'));
    }

    public postSchema(appName: string, dto: CreateSchemaDto): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.postVersioned(this.http, url, dto).pipe(
            map(({ version, payload }) => {
                return parseSchemaWithDetails(payload.body, version);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Created', appName);
            }),
            pretifyError('Failed to create schema. Please reload.'));
    }

    public putScripts(appName: string, resource: Resource, dto: {}, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['update/scripts'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'ScriptsConfigured', appName);
            }),
            pretifyError('Failed to update schema scripts. Please reload.'));
    }

    public putSchema(appName: string, resource: Resource, dto: UpdateSchemaDto, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Updated', appName);
            }),
            pretifyError('Failed to update schema. Please reload.'));
    }

    public putCategory(appName: string, resource: Resource, dto: UpdateSchemaCategoryDto, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['update/category'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'CategoryChanged', appName);
            }),
            pretifyError('Failed to change category. Please reload.'));
    }

    public putPreviewUrls(appName: string, resource: Resource, dto: {}, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['update/urls'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'PreviewUrlsConfigured', appName);
            }),
            pretifyError('Failed to configure preview urls. Please reload.'));
    }

    public publishSchema(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['publish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Published', appName);
            }),
            pretifyError('Failed to publish schema. Please reload.'));
    }

    public unpublishSchema(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['unpublish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Unpublished', appName);
            }),
            pretifyError('Failed to unpublish schema. Please reload.'));
    }

    public postField(appName: string, resource: Resource, dto: AddFieldDto, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['fields/add'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldCreated', appName);
            }),
            pretifyError('Failed to add field. Please reload.'));
    }

    public putFieldOrdering(appName: string, resource: Resource, dto: number[], version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['fields/order'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, { fieldIds: dto }).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldsReordered', appName);
            }),
            pretifyError('Failed to reorder fields. Please reload.'));
    }

    public putField(appName: string, resource: Resource, dto: UpdateFieldDto, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldUpdated', appName);
            }),
            pretifyError('Failed to update field. Please reload.'));
    }

    public lockField(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['lock'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldLocked', appName);
            }),
            pretifyError('Failed to lock field. Please reload.'));
    }

    public enableField(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['enable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldEnabled', appName);
            }),
            pretifyError('Failed to enable field. Please reload.'));
    }

    public disableField(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['disable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldDisabled', appName);
            }),
            pretifyError('Failed to disable field. Please reload.'));
    }

    public showField(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['show'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldShown', appName);
            }),
            pretifyError('Failed to show field. Please reload.'));
    }

    public hideField(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['hide'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldHidden', appName);
            }),
            pretifyError('Failed to hide field. Please reload.'));
    }

    public deleteField(appName: string, resource: Resource, version: Version): Observable<SchemaDetailsDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ version: newVersion, payload }) => {
                return parseSchemaWithDetails(payload.body, newVersion);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldDeleted', appName);
            }),
            pretifyError('Failed to delete field. Please reload.'));
    }

    public deleteSchema(appName: string, resource: Resource, version: Version): Observable<Versioned<any>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'Deleted', appName);
            }),
            pretifyError('Failed to delete schema. Please reload.'));
    }
}

function parseSchemas(response: any) {
    const raw: any[] = response.items;

    const items = raw.map(item =>
        new SchemaDto(item._links,
            item.id,
            item.name,
            item.category,
            new SchemaPropertiesDto(item.properties.label, item.properties.hints),
            item.isSingleton,
            item.isPublished,
            DateTime.parseISO_UTC(item.created), item.createdBy,
            DateTime.parseISO_UTC(item.lastModified), item.lastModifiedBy,
            new Version(item.version.toString())));

    const _links = response._links;

    return { items, _links, canCreate: hasAnyLink(_links, 'create') };
}

function parseSchemaWithDetails(response: any, version: Version) {
    const fields = response.fields.map((item: any) => {
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

                return new NestedFieldDto(nestedItem._links,
                    nestedItem.fieldId,
                    nestedItem.name,
                    nestedPropertiesDto,
                    item.fieldId,
                    nestedItem.isLocked,
                    nestedItem.isHidden,
                    nestedItem.isDisabled);
            });
        }

        return new RootFieldDto(item._links,
            item.fieldId,
            item.name,
            propertiesDto,
            item.partitioning,
            item.isLocked,
            item.isHidden,
            item.isDisabled,
            nested || []);
    });

    const properties = new SchemaPropertiesDto(response.properties.label, response.properties.hints);

    return new SchemaDetailsDto(response._links,
        response.id,
        response.name,
        response.category,
        properties,
        response.isSingleton,
        response.isPublished,
        DateTime.parseISO_UTC(response.created), response.createdBy,
        DateTime.parseISO_UTC(response.lastModified), response.lastModifiedBy,
        version,
        fields,
        response.scripts || {},
        response.previewUrls || {});
}