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
import { ApiUrlConfig, DateTime, hasAnyLink, HTTP, pretifyError, Resource, ResourceLinks, StringHelper, Types, Version, Versioned } from '@app/framework';
import { QueryModel } from './query';
import { createProperties, FieldPropertiesDto } from './schemas.types';

export type FieldRuleAction = 'Disable' | 'Hide' | 'Require';
export type SchemaType = 'Default' | 'Singleton' | 'Component';
export type SchemaScripts = Record<string, string | null>;
export type PreviewUrls = Record<string, string>;

export const META_FIELDS = {
    empty: {
        name: '',
        label: '',
    },
    id: {
        name: 'meta.id',
        label: 'i18n:schemas.tableHeaders.id',
    },
    created: {
        name: 'meta.created',
        label: 'i18n:schemas.tableHeaders.created',
    },
    createdByAvatar: {
        name: 'meta.createdBy.avatar',
        label: 'i18n:schemas.tableHeaders.createdByShort',
    },
    createdByName: {
        name: 'meta.createdBy.name',
        label: 'i18n:schemas.tableHeaders.createdBy',
    },
    lastModified: {
        name: 'meta.lastModified',
        label: 'i18n:schemas.tableHeaders.lastModified',
    },
    lastModifiedByAvatar: {
        name: 'meta.lastModifiedBy.avatar',
        label: 'i18n:schemas.tableHeaders.lastModifiedByShort',
    },
    lastModifiedByName: {
        name: 'meta.lastModifiedBy.name',
        label: 'i18n:schemas.tableHeaders.lastModifiedBy',
    },
    status: {
        name: 'meta.status',
        label: 'i18n:schemas.tableHeaders.status',
    },
    statusColor: {
        name: 'meta.status.color',
        label: 'i18n:schemas.tableHeaders.status',
    },
    statusNext: {
        name: 'meta.status.next',
        label: 'i18n:schemas.tableHeaders.nextStatus',
    },
    version: {
        name: 'meta.version',
        label: 'i18n:schemas.tableHeaders.version',
    },
    translationStatus: {
        name: 'meta.translationStatus',
        label: 'i18n:schemas.tableHeaders.translationStatus',
    },
    translationStatusAverage: {
        name: 'meta.translationStatusAverage',
        label: 'i18n:schemas.tableHeaders.translationStatusAverage',
    },
};

export const FIELD_RULE_ACTIONS: ReadonlyArray<FieldRuleAction> = [
    'Disable',
    'Hide',
    'Require',
];

export class SchemaDto {
    public readonly _links: ResourceLinks;

    public readonly canAddField: boolean;
    public readonly canContentsCreate: boolean;
    public readonly canContentsCreateAndPublish: boolean;
    public readonly canContentsRead: boolean;
    public readonly canDelete: boolean;
    public readonly canOrderFields: boolean;
    public readonly canPublish: boolean;
    public readonly canReadContents: boolean;
    public readonly canSynchronize: boolean;
    public readonly canUnpublish: boolean;
    public readonly canUpdate: boolean;
    public readonly canUpdateCategory: boolean;
    public readonly canUpdateRules: boolean;
    public readonly canUpdateScripts: boolean;
    public readonly canUpdateUIFields: boolean;
    public readonly canUpdateUrls: boolean;

    public readonly displayName: string;

    public readonly contentFields: ReadonlyArray<TableField> = [];

    public readonly defaultListFields: ReadonlyArray<TableField> = [];
    public readonly defaultReferenceFields: ReadonlyArray<TableField> = [];

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
        public readonly fieldsInLists: ReadonlyArray<string> = [],
        public readonly fieldsInReferences: ReadonlyArray<string> = [],
        public readonly fieldRules: ReadonlyArray<FieldRule> = [],
        public readonly previewUrls: PreviewUrls = {},
        public readonly scripts: SchemaScripts = {},
    ) {
        this._links = links;

        this.canAddField = hasAnyLink(links, 'fields/add');
        this.canContentsCreate = hasAnyLink(links, 'contents/create');
        this.canContentsCreateAndPublish = hasAnyLink(links, 'contents/create/publish');
        this.canContentsRead = hasAnyLink(links, 'contents');
        this.canDelete = hasAnyLink(links, 'delete');
        this.canOrderFields = hasAnyLink(links, 'fields/order');
        this.canPublish = hasAnyLink(links, 'publish');
        this.canReadContents = hasAnyLink(links, 'contents');
        this.canSynchronize = hasAnyLink(this, 'update/sync');
        this.canUnpublish = hasAnyLink(links, 'unpublish');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canUpdateCategory = hasAnyLink(links, 'update/category');
        this.canUpdateRules = hasAnyLink(links, 'update/rules');
        this.canUpdateScripts = hasAnyLink(links, 'update/scripts');
        this.canUpdateUIFields = hasAnyLink(links, 'fields/ui');
        this.canUpdateUrls = hasAnyLink(links, 'update/urls');

        this.displayName = StringHelper.firstNonEmpty(this.properties.label, this.name);

        function tableField(rootField: RootFieldDto) {
            return { name: rootField.name, label: rootField.displayName, rootField };
        }

        if (fields) {
            this.contentFields = fields.filter(x => x.properties.isContentField).map(tableField);

            function tableFields(names: ReadonlyArray<string>, fields: ReadonlyArray<RootFieldDto>): TableField[] {
                const result: TableField[] = [];

                for (const name of names) {
                    const metaField = Object.values(META_FIELDS).find(x => x.name === name);

                    if (metaField) {
                        result.push(metaField);
                    } else {
                        const field = fields.find(x => x.name === name && x.properties.isContentField);

                        if (field) {
                            result.push(tableField(field));
                        }
                    }
                }

                return result;
            }

            const listFields = tableFields(fieldsInLists, fields);

            if (listFields.length === 0) {
                listFields.push(META_FIELDS.lastModifiedByAvatar);

                if (fields.length > 0) {
                    listFields.push(tableField(this.fields[0]));
                } else {
                    listFields.push(META_FIELDS.empty);
                }

                listFields.push(META_FIELDS.statusColor);
                listFields.push(META_FIELDS.lastModified);
            }

            this.defaultListFields = listFields;

            const referenceFields = tableFields(fieldsInReferences, fields);

            if (referenceFields.length === 0) {
                if (fields.length > 0) {
                    referenceFields.push(tableField(this.fields[0]));
                } else {
                    referenceFields.push(META_FIELDS.empty);
                }
            }

            this.defaultReferenceFields = referenceFields;
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

            for (const [key, value] of Object.entries(source)) {
                if (!exclude.includes(key) && key.indexOf('can') !== 0 && !Types.isUndefined(value) && !Types.isNull(value)) {
                    clone[key] = value;
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

export type TableField = Readonly<{
    // The name of the table field.
    name: string;

    // The label for the table header.
    label: string;

    // The reference to the root field.
    rootField?: RootFieldDto;
}>;

export type FieldRule = Readonly<{
    // The path to the field to update when the rule is valid.
    field: string;

    // The action to invoke.
    action: FieldRuleAction;

    //The condition as javascript expression.
    condition: string;
}>;

export type SchemaCompletions = ReadonlyArray<{
    // The autocompletion path.
    path: string;

    // The description of the autocompletion field.
    description: string;

    // The type of the autocompletion field.
    type: string;
 }>;

export type SchemasDto = Readonly<{
    // The list of schemas.
    items: ReadonlyArray<SchemaDto>;

    // True, if the user has permissions to create a new schema.
    canCreate?: boolean;
}>;

export type AddFieldDto = Readonly<{
    // The name of the field.
    name: string;

    // The partitioning of the field.
    partitioning?: string;

    // The field properties.
    properties: FieldPropertiesDto;
}>;

export type UpdateUIFields = Readonly<{
    // The names of all fields that should be shown in the list.
    fieldsInLists?: ReadonlyArray<string>;

    // The names of all fields that should be shown in the reference list.
    fieldsInReferences?: ReadonlyArray<string>;
}>;

export type CreateSchemaDto = Readonly<{
    // The name of the schema.
    name: string;

    // The initial fields of the schema.
    fields?: ReadonlyArray<RootFieldDto>;

    // The category name.
    category?: string;

    // The type of the schema.
    type?: string;

    // The initial published state.
    isPublished?: boolean;

    // The initial schema properties.
    properties?: SchemaPropertiesDto;
}>;

export type UpdateSchemaCategoryDto = Readonly<{
    // The name of the category.
    name?: string;
}>;

export type UpdateFieldDto = Readonly<{
    // The field properties.
    properties: FieldPropertiesDto;
}>;

export type SynchronizeSchemaDto = Readonly<{
    // True, to not delete fields when synchronizing.
    noFieldDeletiong?: boolean;

    // True, to not recreate fields when synchronizing.
    noFieldRecreation?: boolean;

    // The additional properties.
    [key: string]: any;
}>;

export type UpdateSchemaDto = Readonly<{
    // The label of the schema.
    label?: string;

    // The hints to explain the schema.
    hints?: string;

    // The URL to the contents sidebar plugin.
    contentsSidebarUrl?: string;

    // The URL to the content sidebar plugin.
    contentSidebarUrl?: string;

    // The URL to an editor to replace the editor.
    contentEditorUrl?: string;

    // True, if the content should be validated on publishing.
    validateOnPublish?: boolean;

    // The tags.
    tags?: ReadonlyArray<string>;
}>;

@Injectable()
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
            pretifyError('i18n:schemas.createFailed'));
    }

    public putScripts(appName: string, resource: Resource, dto: {}, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/scripts'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.updateRulesFailed'));
    }

    public putSchemaSync(appName: string, resource: Resource, dto: SynchronizeSchemaDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/sync'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.updateFailed'));
    }

    public putCategory(appName: string, resource: Resource, dto: UpdateSchemaCategoryDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['update/category'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.updatePreviewUrlsFailed'));
    }

    public publishSchema(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['publish'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.unpublishFailed'));
    }

    public postField(appName: string, resource: Resource, dto: AddFieldDto, version: Version): Observable<SchemaDto> {
        const link = resource._links['fields/add'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.updateUIFieldsFailed'));
    }

    public putFieldOrdering(appName: string, resource: Resource, dto: ReadonlyArray<number>, version: Version): Observable<SchemaDto> {
        const link = resource._links['fields/order'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, { fieldIds: dto }).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.updateFieldFailed'));
    }

    public lockField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['lock'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.enableFieldFailed'));
    }

    public disableField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['disable'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.showFieldFailed'));
    }

    public hideField(appName: string, resource: Resource, version: Version): Observable<SchemaDto> {
        const link = resource._links['hide'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, {}).pipe(
            map(({ payload }) => {
                return parseSchema(payload.body);
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
            pretifyError('i18n:schemas.deleteFieldFailed'));
    }

    public deleteSchema(appName: string, resource: Resource, version: Version): Observable<Versioned<any>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            pretifyError('i18n:schemas.deleteFailed'));
    }

    public getCompletions(appName: string, schemaName: string): Observable<SchemaCompletions> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/completion`);

        return this.http.get<SchemaCompletions>(url);
    }

    public getFilters(appName: string, schemaName: string): Observable<QueryModel> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/filters`);

        return this.http.get<QueryModel>(url);
    }
}

function parseSchemas(response: { items: any[] } & Resource) {
    const { items: list, _links } = response;
    const items = list.map(parseSchema);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, canCreate };
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
