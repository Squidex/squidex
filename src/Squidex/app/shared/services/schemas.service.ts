/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ValidatorFn, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/angular/http/http-extensions';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP,
    Model,
    StringHelper,
    ValidatorsEx,
    Version,
    Versioned
} from '@app/framework';

export const fieldTypes = [
    {
        type: 'String',
        description: 'Titles, names, paragraphs.'
    }, {
        type: 'Assets',
        description: 'Images, videos, documents.'
    }, {
        type: 'Boolean',
        description: 'Yes or no, true or false.'
    }, {
        type: 'DateTime',
        description: 'Events date, opening hours.'
    }, {
        type: 'Geolocation',
        description: 'Coordinates: latitude and longitude.'
    }, {
        type: 'Json',
        description: 'Data in JSON format, for developers.'
    }, {
        type: 'Number',
        description: 'ID, order number, rating, quantity.'
    }, {
        type: 'References',
        description: 'Links to other content items.'
    }, {
        type: 'Tags',
        description: 'Special format for tags.'
    }
];

export const fieldInvariant = 'iv';

export function createProperties(fieldType: string, values: Object | null = null): FieldPropertiesDto {
    let properties: FieldPropertiesDto;

    switch (fieldType) {
        case 'Array':
            properties = new ArrayFieldPropertiesDto();
            break;
        case 'Assets':
            properties = new AssetsFieldPropertiesDto();
            break;
        case 'Boolean':
            properties = new BooleanFieldPropertiesDto('Checkbox');
            break;
        case 'DateTime':
            properties = new DateTimeFieldPropertiesDto('DateTime');
            break;
        case 'Geolocation':
            properties = new GeolocationFieldPropertiesDto();
            break;
        case 'Json':
            properties = new JsonFieldPropertiesDto();
            break;
        case 'Number':
            properties = new NumberFieldPropertiesDto('Input');
            break;
        case 'References':
            properties = new ReferencesFieldPropertiesDto();
            break;
        case 'String':
            properties = new StringFieldPropertiesDto('Input');
            break;
        case 'Tags':
            properties = new TagsFieldPropertiesDto();
            break;
        default:
            throw 'Invalid properties type';
    }

    if (values) {
        Object.assign(properties, values);
    }

    return properties;
}

export class SchemaDto extends Model {
    public displayName: string;

    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly category: string,
        public readonly properties: SchemaPropertiesDto,
        public readonly isPublished: boolean,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version
    ) {
        super();
    }

    public onCreated() {
        this.displayName = StringHelper.firstNonEmpty(this.properties.label, this.name);
    }

    public with(value: Partial<SchemaDto>): SchemaDto {
        return this.clone(value);
    }
}

export class SchemaDetailsDto extends SchemaDto {
    public listFields: RootFieldDto[];

    constructor(id: string, name: string, category: string, properties: SchemaPropertiesDto, isPublished: boolean, created: DateTime, createdBy: string, lastModified: DateTime, lastModifiedBy: string, version: Version,
        public readonly fields: RootFieldDto[],
        public readonly scriptQuery?: string,
        public readonly scriptCreate?: string,
        public readonly scriptUpdate?: string,
        public readonly scriptDelete?: string,
        public readonly scriptChange?: string
    ) {
        super(id, name, category, properties, isPublished, created, createdBy, lastModified, lastModifiedBy, version);

        this.onCreated();
    }

    public onCreated() {
        super.onCreated();

        this.listFields = this.fields.filter(x => x.properties.isListField);

        if (this.listFields.length === 0 && this.fields.length > 0) {
            this.listFields = [this.fields[0]];
        }

        if (this.listFields.length === 0) {
            this.listFields = [<any>{ properties: {} }];
        }
    }

    public with(value: Partial<SchemaDetailsDto>): SchemaDetailsDto {
        return this.clone(value);
    }
}

export class FieldDto extends Model {
    public displayName: string;
    public displayPlaceholder: string;

    constructor(
        public readonly fieldId: number,
        public readonly name: string,
        public readonly properties: FieldPropertiesDto
    ) {
        super();
    }

    public onCreated() {
        this.displayName = StringHelper.firstNonEmpty(this.properties.label, this.name);
        this.displayPlaceholder = this.properties.placeholder || '';
    }

    public formatValue(value: any): string {
        return this.properties.formatValue(value);
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        return this.properties.createValidators(isOptional);
    }

    public defaultValue(): any {
        return this.properties.getDefaultValue();
    }

    public with(value: Partial<FieldDto>): FieldDto {
        return this.clone(value);
    }
}

export class RootFieldDto extends FieldDto {
    public readonly isLocalizable = this.partitioning === 'language';

    constructor(fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly partitioning: string,
        public readonly isHidden: boolean = false,
        public readonly isDisabled: boolean = false,
        public readonly isLocked: boolean = false,
        public readonly nested: NestedFieldDto[] = []
    ) {
        super(fieldId, name, properties);

        this.onCreated();
    }

    public with(value: Partial<RootFieldDto>): RootFieldDto {
        return this.clone(value);
    }
}

export class NestedFieldDto extends FieldDto {
    constructor(fieldId: number, name: string, properties: FieldPropertiesDto,
        public readonly parentId: number,
        public readonly isHidden: boolean = false,
        public readonly isDisabled: boolean = false
    ) {
        super(fieldId, name, properties);

        this.onCreated();
    }

    public with(value: Partial<NestedFieldDto>): NestedFieldDto {
        return this.clone(value);
    }
}

export type AnyFieldDto = RootFieldDto | NestedFieldDto;

export abstract class FieldPropertiesDto {
    public abstract fieldType: string;

    public readonly editorUrl?: string;
    public readonly label?: string;
    public readonly hints?: string;
    public readonly placeholder?: string;
    public readonly isRequired: boolean = false;
    public readonly isListField: boolean = false;

    constructor(public readonly editor: string,
        props?: Partial<FieldPropertiesDto>
    ) {
        if (props) {
            Object.assign(this, props);
        }
    }

    public abstract formatValue(value: any): string;

    public abstract createValidators(isOptional: boolean): ValidatorFn[];

    public getDefaultValue(): any {
        return null;
    }
}

export class ArrayFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Array';

    public readonly minItems?: number;
    public readonly maxItems?: number;

    constructor(
        props?: Partial<ArrayFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        if (value.length) {
            return `${value.length} Items(s)`;
        } else {
            return '0 Items';
        }
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        if (this.minItems) {
            validators.push(Validators.minLength(this.minItems));
        }

        if (this.maxItems) {
            validators.push(Validators.maxLength(this.maxItems));
        }

        return validators;
    }
}

export class AssetsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Assets';

    public readonly minItems?: number;
    public readonly maxItems?: number;
    public readonly minSize?: number;
    public readonly maxSize?: number;
    public readonly allowedExtensions?: string[];
    public readonly mustBeImage?: boolean;
    public readonly minWidth?: number;
    public readonly maxWidth?: number;
    public readonly minHeight?: number;
    public readonly maxHeight?: number;
    public readonly aspectWidth?: number;
    public readonly aspectHeight?: number;

    constructor(
        props?: Partial<AssetsFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        if (value.length) {
            return `${value.length} Asset(s)`;
        } else {
            return '0 Assets';
        }
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        if (this.minItems) {
            validators.push(Validators.minLength(this.minItems));
        }

        if (this.maxItems) {
            validators.push(Validators.maxLength(this.maxItems));
        }

        return validators;
    }
}

export class BooleanFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Boolean';

    public readonly inlineEditable: boolean = false;
    public readonly defaultValue?: boolean;

    constructor(editor: string,
        props?: Partial<BooleanFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public formatValue(value: any): string {
        if (value === null || value === undefined) {
            return '';
        }

        return value ? 'Yes' : 'No';
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        return validators;
    }

    public getDefaultValue(): any {
        return this.defaultValue;
    }
}

export class DateTimeFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'DateTime';

    public readonly defaultValue?: string;
    public readonly maxValue?: string;
    public readonly minValue?: string;
    public readonly calculatedDefaultValue?: string;

    constructor(editor: string,
        props?: Partial<DateTimeFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        try {
            const parsed = DateTime.parseISO_UTC(value);

            if (this.editor === 'Date') {
                return parsed.toUTCStringFormat('YYYY-MM-DD');
            } else {
                return parsed.toUTCStringFormat('YYYY-MM-DD HH:mm:ss');
            }
        } catch (ex) {
            return value;
        }
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        return validators;
    }

    public getDefaultValue(now?: DateTime): any {
        now = now || DateTime.now();

        if (this.calculatedDefaultValue === 'Now') {
            return now.toUTCStringFormat('YYYY-MM-DDTHH:mm:ss') + 'Z';
        } else if (this.calculatedDefaultValue === 'Today') {
            return now.toUTCStringFormat('YYYY-MM-DD');
        } else {
            return this.defaultValue;
        }
    }
}

export class GeolocationFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Geolocation';

    constructor(
        props?: Partial<GeolocationFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        return `${value.longitude}, ${value.latitude}`;
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        return validators;
    }
}

export class JsonFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Json';

    constructor(
        props?: Partial<JsonFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        return '<Json />';
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        return validators;
    }
}

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Number';

    public readonly inlineEditable: boolean = false;
    public readonly defaultValue?: number;
    public readonly maxValue?: number;
    public readonly minValue?: number;
    public readonly allowedValues?: number[];

    constructor(editor: string,
        props?: Partial<NumberFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        return value;
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        if (this.minValue) {
            validators.push(Validators.min(this.minValue));
        }

        if (this.maxValue) {
            validators.push(Validators.max(this.maxValue));
        }

        if (this.allowedValues && this.allowedValues.length > 0) {
            const values: (number | null)[] = this.allowedValues;

            if (this.isRequired && !isOptional) {
                validators.push(ValidatorsEx.validValues(values));
            } else {
                validators.push(ValidatorsEx.validValues(values.concat([null])));
            }
        }

        return validators;
    }

    public getDefaultValue(): any {
        return this.defaultValue;
    }
}

export class ReferencesFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'References';

    public readonly minItems?: number;
    public readonly maxItems?: number;
    public readonly schemaId?: string;

    constructor(
        props?: Partial<ReferencesFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        if (value.length) {
            return `${value.length} Reference(s)`;
        } else {
            return '0 References';
        }
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        if (this.minItems) {
            validators.push(Validators.minLength(this.minItems));
        }

        if (this.maxItems) {
            validators.push(Validators.maxLength(this.maxItems));
        }

        return validators;
    }
}

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'String';

    public readonly inlineEditable = false;
    public readonly defaultValue?: string;
    public readonly pattern?: string;
    public readonly patternMessage?: string;
    public readonly minLength?: number;
    public readonly maxLength?: number;
    public readonly allowedValues?: string[];

    constructor(editor: string,
        props?: Partial<StringFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        return value;
    }

    public createValidators(isOptional: false): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        if (this.minLength) {
            validators.push(Validators.minLength(this.minLength));
        }

        if (this.maxLength) {
            validators.push(Validators.maxLength(this.maxLength));
        }

        if (this.pattern && this.pattern.length > 0) {
            validators.push(ValidatorsEx.pattern(this.pattern, this.patternMessage));
        }

        if (this.allowedValues && this.allowedValues.length > 0) {
            const values: (string | null)[] = this.allowedValues;

            if (this.isRequired && !isOptional) {
                validators.push(ValidatorsEx.validValues(values));
            } else {
                validators.push(ValidatorsEx.validValues(values.concat([null])));
            }
        }

        return validators;
    }

    public getDefaultValue(): any {
        return this.defaultValue;
    }
}

export class TagsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Tags';

    public readonly minItems?: number;
    public readonly maxItems?: number;

    constructor(
        props?: Partial<TagsFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        if (value.length) {
            return value.join(', ');
        } else {
            return '';
        }
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        if (this.minItems) {
            validators.push(Validators.minLength(this.minItems));
        }

        if (this.maxItems) {
            validators.push(Validators.maxLength(this.maxItems));
        }

        return validators;
    }
}

export class SchemaPropertiesDto {
    constructor(
        public readonly label?: string,
        public readonly hints?: string
    ) {
    }
}

export class UpdateSchemaDto {
    constructor(
        public readonly label?: string,
        public readonly hints?: string
    ) {
    }
}

export class UpdateSchemaCategoryDto {
    constructor(
        public readonly name?: string
    ) {
    }
}

export class AddFieldDto {
    constructor(
        public readonly name: string,
        public readonly partitioning: string,
        public readonly properties: FieldPropertiesDto
    ) {
    }
}

export class UpdateFieldDto {
    constructor(
        public readonly properties: FieldPropertiesDto
    ) {
    }
}

export class CreateSchemaDto {
    constructor(
        public readonly name: string,
        public readonly fields?: RootFieldDto[],
        public readonly properties?: SchemaPropertiesDto
    ) {
    }
}

export class UpdateSchemaScriptsDto {
    constructor(
        public readonly scriptQuery?: string,
        public readonly scriptCreate?: string,
        public readonly scriptUpdate?: string,
        public readonly scriptDelete?: string,
        public readonly scriptChange?: string
    ) {
    }
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

        return HTTP.getVersioned<any>(this.http, url)
            .map(response => {
                const body = response.payload.body;

                const items: any[] = body;

                return items.map(item => {
                    const properties = new SchemaPropertiesDto(item.properties.label, item.properties.hints);

                    return new SchemaDto(
                        item.id,
                        item.name,
                        item.category, properties,
                        item.isPublished,
                        DateTime.parseISO_UTC(item.created), item.createdBy,
                        DateTime.parseISO_UTC(item.lastModified), item.lastModifiedBy,
                        new Version(item.version.toString()));
                });
            })
            .pretifyError('Failed to load schemas. Please reload.');
    }

    public getSchema(appName: string, id: string): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${id}`);

        return HTTP.getVersioned<any>(this.http, url)
            .map(response => {
                const body = response.payload.body;

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
                                nestedItem.isHidden,
                                nestedItem.isDisabled);
                        });
                    }

                    return new RootFieldDto(
                        item.fieldId,
                        item.name,
                        propertiesDto,
                        item.partitioning,
                        item.isHidden,
                        item.isDisabled,
                        item.isLocked,
                        nested || []);
                });

                const properties = new SchemaPropertiesDto(body.properties.label, body.properties.hints);

                return new SchemaDetailsDto(
                    body.id,
                    body.name,
                    body.category,
                    properties,
                    body.isPublished,
                    DateTime.parseISO_UTC(body.created), body.createdBy,
                    DateTime.parseISO_UTC(body.lastModified), body.lastModifiedBy,
                    response.version,
                    fields,
                    body.scriptQuery,
                    body.scriptCreate,
                    body.scriptUpdate,
                    body.scriptDelete,
                    body.scriptChange);
            })
            .pretifyError('Failed to load schema. Please reload.');
    }

    public postSchema(appName: string, dto: CreateSchemaDto, user: string, now: DateTime): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.postVersioned<any>(this.http, url, dto)
            .map(response => {
                const body = response.payload.body;

                now = now || DateTime.now();

                return new SchemaDetailsDto(
                    body.id,
                    dto.name,
                    '',
                    dto.properties || new SchemaPropertiesDto(),
                    false,
                    now, user,
                    now, user,
                    response.version,
                    dto.fields || [],
                    body.scriptQuery,
                    body.scriptCreate,
                    body.scriptUpdate,
                    body.scriptDelete,
                    body.scriptChange);
            })
            .do(schema => {
                this.analytics.trackEvent('Schema', 'Created', appName);
            })
            .pretifyError('Failed to create schema. Please reload.');
    }

    public deleteSchema(appName: string, schemaName: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'Deleted', appName);
            })
            .pretifyError('Failed to delete schema. Please reload.');
    }

    public putScripts(appName: string, schemaName: string, dto: UpdateSchemaScriptsDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/scripts`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'ScriptsConfigured', appName);
            })
            .pretifyError('Failed to update schema scripts. Please reload.');
    }

    public putSchema(appName: string, schemaName: string, dto: UpdateSchemaDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'Updated', appName);
            })
            .pretifyError('Failed to update schema. Please reload.');
    }

    public publishSchema(appName: string, schemaName: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/publish`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'Published', appName);
            })
            .pretifyError('Failed to publish schema. Please reload.');
    }

    public unpublishSchema(appName: string, schemaName: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/unpublish`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'Unpublished', appName);
            })
            .pretifyError('Failed to unpublish schema. Please reload.');
    }

    public putCategory(appName: string, schemaName: string, dto: UpdateSchemaCategoryDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/category`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'CategoryChanged', appName);
            })
            .pretifyError('Failed to change category. Please reload.');
    }

    public postField(appName: string, schemaName: string, dto: AddFieldDto, parentId: number | undefined, version: Version): Observable<Versioned<AnyFieldDto>> {
        const url = this.buildUrl(appName, schemaName, parentId, '');

        return HTTP.postVersioned<any>(this.http, url, dto, version)
            .map(response => {
                const body = response.payload.body;

                if (parentId) {
                    const field = new NestedFieldDto(body.id, dto.name, dto.properties, parentId);

                    return new Versioned(response.version, field);
                } else {
                    const field = new RootFieldDto(body.id, dto.name, dto.properties, dto.partitioning);

                    return new Versioned(response.version, field);
                }
            })
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldCreated', appName);
            })
            .pretifyError('Failed to add field. Please reload.');
    }

    public putFieldOrdering(appName: string, schemaName: string, dto: number[], parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, '/ordering');

        return HTTP.putVersioned(this.http, url, { fieldIds: dto }, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldsReordered', appName);
            })
            .pretifyError('Failed to reorder fields. Please reload.');
    }

    public putField(appName: string, schemaName: string, fieldId: number, dto: UpdateFieldDto, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldUpdated', appName);
            })
            .pretifyError('Failed to update field. Please reload.');
    }

    public lockField(appName: string, schemaName: string, fieldId: number, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/lock`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldLocked', appName);
            })
            .pretifyError('Failed to lock field. Please reload.');
    }

    public enableField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/enable`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldEnabled', appName);
            })
            .pretifyError('Failed to enable field. Please reload.');
    }

    public disableField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/disable`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldDisabled', appName);
            })
            .pretifyError('Failed to disable field. Please reload.');
    }

    public showField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/show`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldShown', appName);
            })
            .pretifyError('Failed to show field. Please reload.');
    }

    public hideField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}/hide`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldHidden', appName);
            })
            .pretifyError('Failed to hide field. Please reload.');
    }

    public deleteField(appName: string, schemaName: string, fieldId: number, parentId: number | undefined, version: Version): Observable<Versioned<any>> {
        const url = this.buildUrl(appName, schemaName, parentId, `/${fieldId}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldDeleted', appName);
            })
            .pretifyError('Failed to delete field. Please reload.');
    }

    private buildUrl(appName: string, schemaName: string, parentId: number | undefined, suffix: string) {
        const url =
            parentId ?
                this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${parentId}/nested${suffix}`) :
                this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields${suffix}`);

        return url;
    }
}