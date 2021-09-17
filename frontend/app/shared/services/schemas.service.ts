/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, hasAnyLink, HTTP, pretifyError, Resource, ResourceLinks, StringHelper, Types, Version, Versioned } from '@app/framework';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { createProperties, FieldPropertiesDto } from './schemas.types';

export const MetaFields = {
    id: 'meta.id',
    created: 'meta.created',
    createdByAvatar: 'meta.createdBy.avatar',
    createdByName: 'meta.createdBy.name',
    lastModified: 'meta.lastModified',
    lastModifiedByAvatar: 'meta.lastModifiedBy.avatar',
    lastModifiedByName: 'meta.lastModifiedBy.name',
    status: 'meta.status',
    statusColor: 'meta.status.color',
    statusNext: 'meta.status.next',
    version: 'meta.version',
};

export type SchemaType = 'Default' | 'Singleton' | 'Component';
export type SchemaScripts = Record<string, string | null>;
export type PreviewUrls = Record<string, string>;

export class SchemaDto {
    public readonly _links: ResourceLinks;

    public readonly canAddField: boolean;
    public readonly canContentsRead: boolean;
    public readonly canContentsCreate: boolean;
    public readonly canContentsCreateAndPublish: boolean;
    public readonly canDelete: boolean;
    public readonly canOrderFields: boolean;
    public readonly canPublish: boolean;
    public readonly canReadContents: boolean;
    public readonly canSynchronize: boolean;
    public readonly canUnpublish: boolean;
    public readonly canUpdate: boolean;
    public readonly canUpdateCategory: boolean;
    public readonly canUpdateScripts: boolean;
    public readonly canUpdateUIFields: boolean;
    public readonly canUpdateUrls: boolean;
    public readonly canUpdateRules: boolean;

    public readonly displayName: string;

    public readonly contentFields: ReadonlyArray<RootFieldDto>;

    public readonly defaultListFields: ReadonlyArray<TableField>;
    public readonly defaultReferenceFields: ReadonlyArray<TableField>;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version,
        public readonly name: string,
        public readonly category: string,
        public readonly type: SchemaType,
        public readonly isPublished: boolean,
        public readonly properties: SchemaPropertiesDto,
        public readonly fields: ReadonlyArray<RootFieldDto> = [],
        public readonly fieldsInLists: Tags = [],
        public readonly fieldsInReferences: Tags = [],
        public readonly fieldRules: ReadonlyArray<FieldRule> = [],
        public readonly previewUrls: PreviewUrls = {},
        public readonly scripts: SchemaScripts = {},
    ) {
        this._links = links;

        this.canAddField = hasAnyLink(links, 'fields/add');
        this.canContentsRead = hasAnyLink(links, 'contents');
        this.canContentsCreate = hasAnyLink(links, 'contents/create');
        this.canContentsCreateAndPublish = hasAnyLink(links, 'contents/create/publish');
        this.canDelete = hasAnyLink(links, 'delete');
        this.canOrderFields = hasAnyLink(links, 'fields/order');
        this.canPublish = hasAnyLink(links, 'publish');
        this.canReadContents = hasAnyLink(links, 'contents');
        this.canSynchronize = hasAnyLink(this, 'update/sync');
        this.canUnpublish = hasAnyLink(links, 'unpublish');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canUpdateCategory = hasAnyLink(links, 'update/category');
        this.canUpdateScripts = hasAnyLink(links, 'update/scripts');
        this.canUpdateUIFields = hasAnyLink(links, 'fields/ui');
        this.canUpdateUrls = hasAnyLink(links, 'update/urls');
        this.canUpdateRules = hasAnyLink(links, 'update/rules');

        this.displayName = StringHelper.firstNonEmpty(this.properties.label, this.name);

        if (fields) {
            this.contentFields = fields.filter(x => x.properties.isContentField);

            const listFields = findFields(fieldsInLists, this.contentFields);

            if (listFields.length === 0) {
                listFields.push(MetaFields.lastModifiedByAvatar);

                if (fields.length > 0) {
                    listFields.push(this.fields[0]);
                } else {
                    listFields.push('');
                }

                listFields.push(MetaFields.statusColor);
                listFields.push(MetaFields.lastModified);
            }

            this.defaultListFields = listFields;

            this.defaultReferenceFields = findFields(fieldsInReferences, this.contentFields);

            if (this.defaultReferenceFields.length === 0) {
                if (fields.length > 0) {
                    this.defaultReferenceFields = [fields[0]];
                } else {
                    this.defaultReferenceFields = [''];
                }
            }
        }
    }

    public export(): any {
        const fieldKeys = [
            'fieldId',
            'parentId',
            'parentFieldId',
            '_links',
        ];

        const cleanup = (source: any, ...exclude: string[]): any => {
            const clone = {};

            for (const key in source) {
                if (source.hasOwnProperty(key) && exclude.indexOf(key) < 0 && key.indexOf('can') !== 0) {
                    const value = source[key];

                    if (!Types.isUndefined(value) && !Types.isNull(value)) {
                        clone[key] = value;
                    }
                }
            }

            return clone;
        };

        const result: any = {
            previewUrls: this.previewUrls,
            properties: cleanup(this.properties),
            category: this.category,
            scripts: this.scripts,
            isPublished: this.isPublished,
            fieldRules: this.fieldRules,
            fieldsInLists: this.fieldsInLists,
            fieldsInReferences: this.fieldsInReferences,
            fields: this.fields.map(field => {
                const copy = cleanup(field, ...fieldKeys);

                copy.properties = cleanup(field.properties);

                if (Types.isArray(copy.nested)) {
                    if (copy.nested.length === 0) {
                        delete copy['nested'];
                    } else {
                        copy.nested = field.nested.map(nestedField => {
                            const nestedCopy = cleanup(nestedField, ...fieldKeys);

                            nestedCopy.properties = cleanup(nestedField.properties);

                            return nestedCopy;
                        });
                    }
                }

                return copy;
            }),
            type: this.type,
        };

        return result;
    }
}

function findFields(names: ReadonlyArray<string>, fields: ReadonlyArray<RootFieldDto>): TableField[] {
    const result: TableField[] = [];

    for (const name of names) {
        if (name.startsWith('meta.')) {
            result.push(name);
        } else {
            const field = fields.find(x => x.name === name);

            if (field) {
                result.push(field);
            }
        }
    }

    return result;
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
        return !this.isDisabled && this.rawProperties.inlineEditable === true;
    }

    public get displayName() {
        return StringHelper.firstNonEmpty(this.properties.label, this.name);
    }

    public get displayPlaceholder() {
        return this.properties.placeholder || '';
    }

    public get rawProperties(): any {
        return this.properties;
    }

    constructor(links: ResourceLinks,
        public readonly fieldId: number,
        public readonly name: string,
        public readonly properties: FieldPropertiesDto,
        public readonly isLocked: boolean = false,
        public readonly isHidden: boolean = false,
        public readonly isDisabled: boolean = false,
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

    constructor(links: ResourceLinks, fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly partitioning: string,
        isLocked: boolean = false,
        isHidden: boolean = false,
        isDisabled: boolean = false,
        public readonly nested: ReadonlyArray<NestedFieldDto> = [],
    ) {
        super(links, fieldId, name, properties, isLocked, isHidden, isDisabled);
    }
}

export class NestedFieldDto extends FieldDto {
    constructor(links: ResourceLinks, fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly parentId: number,
        isLocked: boolean = false,
        isHidden: boolean = false,
        isDisabled: boolean = false,
    ) {
        super(links, fieldId, name, properties, isLocked, isHidden, isDisabled);
    }
}

export class SchemaPropertiesDto {
    constructor(
        public readonly label?: string,
        public readonly hints?: string,
        public readonly contentsSidebarUrl?: string,
        public readonly contentSidebarUrl?: string,
        public readonly contentEditorUrl?: string,
        public readonly validateOnPublish?: boolean,
        public readonly tags?: ReadonlyArray<string>,
    ) {
    }
}

export const FIELD_RULE_ACTIONS: ReadonlyArray<FieldRuleAction> = [
    'Disable',
    'Hide',
    'Require',
];

type Tags = readonly string[];

export type TableField = RootFieldDto | string;

export type FieldRuleAction = 'Disable' | 'Hide' | 'Require';
export type FieldRule = { field: string; action: FieldRuleAction; condition: string };

export type SchemaCompletions =
    ReadonlyArray<{ path: string; description: string; type: string }>;

export type SchemasDto =
    Readonly<{ items: ReadonlyArray<SchemaDto>; canCreate: boolean } & Resource>;

export type AddFieldDto =
    Readonly<{ name: string; partitioning?: string; properties: FieldPropertiesDto }>;

export type UpdateUIFields =
    Readonly<{ fieldsInLists?: Tags; fieldsInReferences?: Tags }>;

export type CreateSchemaDto =
    Readonly<{ name: string; fields?: ReadonlyArray<RootFieldDto>; category?: string; type?: string; isPublished?: boolean; properties?: SchemaPropertiesDto }>;

export type UpdateSchemaCategoryDto =
    Readonly<{ name?: string }>;

export type UpdateFieldDto =
    Readonly<{ properties: FieldPropertiesDto }>;

export type SynchronizeSchemaDto =
    Readonly<{ noFieldDeletiong?: boolean; noFieldRecreation?: boolean; [key: string]: any }>;

export type UpdateSchemaDto =
    Readonly<{ label?: string; hints?: string; contentsSidebarUrl?: string; contentSidebarUrl?: string; contentEditorUrl?: string; validateOnPublish?: boolean; tags?: Tags }>;

@Injectable()
export class SchemasService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getSchemas(appName: string): Observable<SchemasDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return parseSchemas(payload.body);
            }),
            pretifyError('i18n:schemas.loadFailed'));
    }

    public getSchema(appName: string, name: string): Observable<SchemaDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${name}`);

        return HTTP.getVersioned(this.http, url).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            pretifyError('i18n:schemas.loadSchemaFailed'));
    }

    public postSchema(appName: string, dto: CreateSchemaDto): Observable<SchemaDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.postVersioned(this.http, url, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Created', appName);
            }),
            pretifyError('i18n:schemas.createFailed'));
    }

    public putScripts(appName: string, resource: Resource, dto: {}, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/scripts'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'ScriptsConfigured', appName);
            }),
            pretifyError('i18n:schemas.updateScriptsFailed'));
    }

    public putFieldRules(appName: string, resource: Resource, dto: ReadonlyArray<FieldRule>, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/rules'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, { fieldRules: dto }).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'RulesConfigured', appName);
            }),
            pretifyError('i18n:schemas.updateRulesFailed'));
    }

    public putSchemaSync(appName: string, resource: Resource, dto: SynchronizeSchemaDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/sync'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Updated', appName);
            }),
            pretifyError('i18n:schemas.synchronizeFailed'));
    }

    public putSchema(appName: string, resource: Resource, dto: UpdateSchemaDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Updated', appName);
            }),
            pretifyError('i18n:schemas.updateFailed'));
    }

    public putCategory(appName: string, resource: Resource, dto: UpdateSchemaCategoryDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/category'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'CategoryChanged', appName);
            }),
            pretifyError('i18n:schemas.changeCategoryFailed'));
    }

    public putPreviewUrls(appName: string, resource: Resource, dto: {}, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/urls'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'PreviewUrlsConfigured', appName);
            }),
            pretifyError('i18n:schemas.updatePreviewUrlsFailed'));
    }

    public publishSchema(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['publish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Published', appName);
            }),
            pretifyError('i18n:schemas.publishFailed'));
    }

    public unpublishSchema(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['unpublish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'Unpublished', appName);
            }),
            pretifyError('i18n:schemas.unpublishFailed'));
    }

    public postField(appName: string, resource: Resource, dto: AddFieldDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['fields/add'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldCreated', appName);
            }),
            pretifyError('i18n:schemas.addFieldFailed'));
    }

    public putUIFields(appName: string, resource: Resource, dto: UpdateUIFields, version: Version): Observable<SchemaDto> {
        const link = resource._links['fields/ui'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'UIFieldsConfigured', appName);
            }),
            pretifyError('i18n:schemas.updateUIFieldsFailed'));
    }

    public putFieldOrdering(appName: string, resource: Resource, dto: ReadonlyArray<number>, version: Version): Observable<SchemaDto> {
        const link = resource._links['fields/order'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, { fieldIds: dto }).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldsReordered', appName);
            }),
            pretifyError('i18n:schemas.reorderFieldsFailed'));
    }

    public putField(appName: string, resource: Resource, dto: UpdateFieldDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldUpdated', appName);
            }),
            pretifyError('i18n:schemas.updateFieldFailed'));
    }

    public lockField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['lock'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldLocked', appName);
            }),
            pretifyError('i18n:schemas.lockFieldFailed'));
    }

    public enableField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['enable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldEnabled', appName);
            }),
            pretifyError('i18n:schemas.enableFieldFailed'));
    }

    public disableField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['disable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldDisabled', appName);
            }),
            pretifyError('i18n:schemas.disableFieldFailed'));
    }

    public showField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['show'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldShown', appName);
            }),
            pretifyError('i18n:schemas.showFieldFailed'));
    }

    public hideField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['hide'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldHidden', appName);
            }),
            pretifyError('i18n:schemas.hideFieldFailed'));
    }

    public deleteField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
            }),
            tap(() => {
                this.analytics.trackEvent('Schema', 'FieldDeleted', appName);
            }),
            pretifyError('i18n:schemas.deleteFieldFailed'));
    }

    public deleteSchema(appName: string, resource: Resource, version: Version): Observable<Versioned<any>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            tap(() => {
                this.analytics.trackEvent('Schema', 'Deleted', appName);
            }),
            pretifyError('i18n:schemas.deleteFailed'));
    }

    public getCompletions(appName: string, schemaName: string): Observable<SchemaCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/completion`);

        return this.http.get<SchemaCompletions>(url);
    }
}

function parseSchemas(response: any) {
    const raw: any[] = response.items;

    const items = raw.map(parseSchema);

    const _links = response._links;

    return { items, _links, canCreate: hasAnyLink(_links, 'create') };
}

function parseSchema(response: any) {
    const fields = response.fields.map(parseField);

    return new SchemaDto(response._links,
        response.id,
        DateTime.parseISO(response.created), response.createdBy,
        DateTime.parseISO(response.lastModified), response.lastModifiedBy,
        new Version(response.version.toString()),
        response.name,
        response.category,
        response.type,
        response.isPublished,
        parseProperties(response.properties),
        fields,
        response.fieldsInLists,
        response.fieldsInReferences,
        response.fieldRules,
        response.previewUrls || {},
        response.scripts || {});
}

function parseProperties(response: any) {
    return new SchemaPropertiesDto(
        response.label,
        response.hints,
        response.contentsSidebarUrl,
        response.contentSidebarUrl,
        response.contentEditorUrl,
        response.validateOnPublish,
        response.tags);
}

export function parseField(item: any) {
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
}
