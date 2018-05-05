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
        case 'Number':
            properties = new NumberFieldPropertiesDto(null, null, null, null, false, false, false, 'Input');
            break;
        case 'String':
            properties = new StringFieldPropertiesDto(null, null, null, null, false, false, false, 'Input');
            break;
        case 'Boolean':
            properties = new BooleanFieldPropertiesDto(null, null, null, null, false, false, false, 'Checkbox');
            break;
        case 'DateTime':
            properties = new DateTimeFieldPropertiesDto(null, null, null, null, false, false, 'DateTime');
            break;
        case 'Geolocation':
            properties = new GeolocationFieldPropertiesDto(null, null, null, null, false, false, 'Map');
            break;
        case 'Json':
            properties = new JsonFieldPropertiesDto(null, null, null, null, false, false);
            break;
        case 'References':
            properties = new ReferencesFieldPropertiesDto(null, null, null, null, false, false);
            break;
        case 'Assets':
            properties = new AssetsFieldPropertiesDto(null, null, null, null, false, false);
            break;
        case 'Tags':
            properties = new TagsFieldPropertiesDto(null, null, null, null, false, false);
            break;
        default:
            throw 'Invalid properties type';
    }

    if (values) {
        Object.assign(properties, values);
    }

    return properties;
}

export class SchemaDto {
    public readonly displayName = StringHelper.firstNonEmpty(this.properties.label, this.name);

    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly properties: SchemaPropertiesDto,
        public readonly isPublished: boolean,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly version: Version
    ) {
    }
}

export class SchemaDetailsDto extends SchemaDto {
    public readonly listFields: FieldDto[];

    constructor(id: string, name: string, properties: SchemaPropertiesDto, isPublished: boolean, createdBy: string, lastModifiedBy: string, created: DateTime, lastModified: DateTime, version: Version,
        public readonly fields: FieldDto[],
        public readonly scriptQuery?: string,
        public readonly scriptCreate?: string,
        public readonly scriptUpdate?: string,
        public readonly scriptDelete?: string,
        public readonly scriptChange?: string
    ) {
        super(id, name, properties, isPublished, createdBy, lastModifiedBy, created, lastModified, version);

        this.listFields = this.fields.filter(x => x.properties.isListField);

        if (this.listFields.length === 0 && this.fields.length > 0) {
            this.listFields = [this.fields[0]];
        }

        if (this.listFields.length === 0) {
            this.listFields = [<any>{ properties: {} }];
        }
    }
}

export class FieldDto {
    public readonly displayName = StringHelper.firstNonEmpty(this.properties.label, this.name);
    public readonly displayPlaceholder = this.properties.placeholder || '';

    public readonly isLocalizable = this.partitioning !== 'invariant';

    constructor(
        public readonly fieldId: number,
        public readonly name: string,
        public readonly isLocked: boolean,
        public readonly isHidden: boolean,
        public readonly isDisabled: boolean,
        public readonly partitioning: string,
        public readonly properties: FieldPropertiesDto
    ) {
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
}

export abstract class FieldPropertiesDto {
    constructor(
        public readonly fieldType: string,
        public readonly label: string | null,
        public readonly hints: string | null,
        public readonly placeholder: string | null,
        public readonly editorUrl: string | null,
        public readonly isRequired: boolean,
        public readonly isListField: boolean
    ) {
    }

    public abstract formatValue(value: any): string;

    public abstract createValidators(isOptional: boolean): ValidatorFn[];

    public getDefaultValue(): any {
        return null;
    }
}

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly inlineEditable: boolean,
        public readonly editor: string,
        public readonly defaultValue?: string,
        public readonly pattern?: string,
        public readonly patternMessage?: string,
        public readonly minLength?: number,
        public readonly maxLength?: number,
        public readonly allowedValues?: string[]
    ) {
        super('String', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly inlineEditable: boolean,
        public readonly editor: string,
        public readonly defaultValue?: number,
        public readonly maxValue?: number,
        public readonly minValue?: number,
        public readonly allowedValues?: number[]
    ) {
        super('Number', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class DateTimeFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly editor: string,
        public readonly defaultValue?: string,
        public readonly maxValue?: string,
        public readonly minValue?: string,
        public readonly calculatedDefaultValue?: string
    ) {
        super('DateTime', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class BooleanFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly inlineEditable: boolean,
        public readonly editor: string,
        public readonly defaultValue?: boolean
    ) {
        super('Boolean', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class GeolocationFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly editor: string
    ) {
        super('Geolocation', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class ReferencesFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly minItems?: number,
        public readonly maxItems?: number,
        public readonly schemaId?: string
    ) {
        super('References', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class AssetsFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly minItems?: number,
        public readonly maxItems?: number,
        public readonly minSize?: number,
        public readonly maxSize?: number,
        public readonly allowedExtensions?: string[],
        public readonly mustBeImage?: boolean,
        public readonly minWidth?: number,
        public readonly maxWidth?: number,
        public readonly minHeight?: number,
        public readonly maxHeight?: number,
        public readonly aspectWidth?: number,
        public readonly aspectHeight?: number
    ) {
        super('Assets', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class TagsFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly minItems?: number,
        public readonly maxItems?: number
    ) {
        super('Tags', label, hints, placeholder, editorUrl, isRequired, isListField);
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

export class JsonFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null, editorUrl: string | null,
        isRequired: boolean,
        isListField: boolean
    ) {
        super('Json', label, hints, placeholder, editorUrl, isRequired, isListField);
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
        public readonly fields?: FieldDto[],
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
                        item.name, properties,
                        item.isPublished,
                        item.createdBy,
                        item.lastModifiedBy,
                        DateTime.parseISO_UTC(item.created),
                        DateTime.parseISO_UTC(item.lastModified),
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

                    return new FieldDto(
                        item.fieldId,
                        item.name,
                        item.isLocked,
                        item.isHidden,
                        item.isDisabled,
                        item.partitioning,
                        propertiesDto);
                });

                const properties = new SchemaPropertiesDto(body.properties.label, body.properties.hints);

                return new SchemaDetailsDto(
                    body.id,
                    body.name, properties,
                    body.isPublished,
                    body.createdBy,
                    body.lastModifiedBy,
                    DateTime.parseISO_UTC(body.created),
                    DateTime.parseISO_UTC(body.lastModified),
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
                    dto.properties || new SchemaPropertiesDto(),
                    false,
                    user,
                    user,
                    now,
                    now,
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

    public postField(appName: string, schemaName: string, dto: AddFieldDto, version: Version): Observable<Versioned<FieldDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields`);

        return HTTP.postVersioned<any>(this.http, url, dto, version)
            .map(response => {
                const body = response.payload.body;

                const field = new FieldDto(
                    body.id,
                    dto.name,
                    false,
                    false,
                    false,
                    dto.partitioning,
                    dto.properties);

                return new Versioned(response.version, field);
            })
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldCreated', appName);
            })
            .pretifyError('Failed to add field. Please reload.');
    }

    public deleteSchema(appName: string, schemaName: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'Deleted', appName);
            })
            .pretifyError('Failed to delete schema. Please reload.');
    }

    public putSchemaScripts(appName: string, schemaName: string, dto: UpdateSchemaScriptsDto, version: Version): Observable<Versioned<any>> {
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

    public putFieldOrdering(appName: string, schemaName: string, dto: number[], version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/ordering`);

        return HTTP.putVersioned(this.http, url, { fieldIds: dto }, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldsReordered', appName);
            })
            .pretifyError('Failed to reorder fields. Please reload.');
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

    public putField(appName: string, schemaName: string, fieldId: number, dto: UpdateFieldDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}`);

        return HTTP.putVersioned(this.http, url, dto, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldUpdated', appName);
            })
            .pretifyError('Failed to update field. Please reload.');
    }

    public enableField(appName: string, schemaName: string, fieldId: number, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/enable`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldEnabled', appName);
            })
            .pretifyError('Failed to enable field. Please reload.');
    }

    public disableField(appName: string, schemaName: string, fieldId: number, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/disable`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldDisabled', appName);
            })
            .pretifyError('Failed to disable field. Please reload.');
    }

    public lockField(appName: string, schemaName: string, fieldId: number, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/lock`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldLocked', appName);
            })
            .pretifyError('Failed to lock field. Please reload.');
    }

    public showField(appName: string, schemaName: string, fieldId: number, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/show`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldShown', appName);
            })
            .pretifyError('Failed to show field. Please reload.');
    }

    public hideField(appName: string, schemaName: string, fieldId: number, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/hide`);

        return HTTP.putVersioned(this.http, url, {}, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldHidden', appName);
            })
            .pretifyError('Failed to hide field. Please reload.');
    }

    public deleteField(appName: string, schemaName: string, fieldId: number, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}`);

        return HTTP.deleteVersioned(this.http, url, version)
            .do(() => {
                this.analytics.trackEvent('Schema', 'FieldDeleted', appName);
            })
            .pretifyError('Failed to delete field. Please reload.');
    }
}