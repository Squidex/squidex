/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ValidatorFn, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    DateTime,
    LocalCacheService,
    HTTP,
    ValidatorsEx,
    Version
} from 'framework';

export const fieldTypes: string[] = [
    'Assets',
    'Boolean',
    'DateTime',
    'Geolocation',
    'Json',
    'Number',
    'References',
    'String'
];

export function createProperties(fieldType: string, values: Object | null = null): FieldPropertiesDto {
    let properties: FieldPropertiesDto;

    switch (fieldType) {
        case 'Number':
            properties = new NumberFieldPropertiesDto(null, null, null, false, false, 'Input');
            break;
        case 'String':
            properties = new StringFieldPropertiesDto(null, null, null, false, false, 'Input');
            break;
        case 'Boolean':
            properties = new BooleanFieldPropertiesDto(null, null, null, false, false, 'Checkbox');
            break;
        case 'DateTime':
            properties = new DateTimeFieldPropertiesDto(null, null, null, false, false, 'DateTime');
            break;
        case 'Geolocation':
            properties = new GeolocationFieldPropertiesDto(null, null, null, false, false, 'Map');
            break;
        case 'Json':
            properties = new JsonFieldPropertiesDto(null, null, null, false, false);
            break;
        case 'References':
            properties = new ReferencesFieldPropertiesDto(null, null, null, false, false);
            break;
        case 'Assets':
            properties = new AssetsFieldPropertiesDto(null, null, null, false, false);
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

    public publish(user: string, now?: DateTime): SchemaDto {
        return new SchemaDto(
            this.id,
            this.name,
            this.properties,
            true,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version);
    }

    public unpublish(user: string, now?: DateTime): SchemaDto {
        return new SchemaDto(
            this.id,
            this.name,
            this.properties,
            false,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version);
    }

    public update(properties: SchemaPropertiesDto, user: string, now?: DateTime): SchemaDto {
        return new SchemaDto(
            this.id,
            this.name,
            properties,
            this.isPublished,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version);
    }
}

export class SchemaDetailsDto extends SchemaDto {
    constructor(id: string, name: string, properties: SchemaPropertiesDto, isPublished: boolean, createdBy: string, lastModifiedBy: string, created: DateTime, lastModified: DateTime, version: Version,
        public readonly fields: FieldDto[]
    ) {
        super(id, name, properties, isPublished, createdBy, lastModifiedBy, created, lastModified, version);
    }

    public publish(user: string, now?: DateTime): SchemaDetailsDto {
        return new SchemaDetailsDto(
            this.id,
            this.name,
            this.properties,
            true,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version,
            this.fields);
    }

    public unpublish(user: string, now?: DateTime): SchemaDetailsDto {
        return new SchemaDetailsDto(
            this.id,
            this.name,
            this.properties,
            false,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version,
            this.fields);
    }

    public update(properties: SchemaPropertiesDto, user: string, now?: DateTime): SchemaDetailsDto {
        return new SchemaDetailsDto(
            this.id,
            this.name,
            properties,
            this.isPublished,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version,
            this.fields);
    }

    public addField(field: FieldDto, user: string, now?: DateTime): SchemaDetailsDto {
        return new SchemaDetailsDto(
            this.id,
            this.name,
            this.properties,
            this.isPublished,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version,
            [...this.fields, field]);
    }

    public updateField(field: FieldDto, user: string, now?: DateTime): SchemaDetailsDto {
        return new SchemaDetailsDto(
            this.id,
            this.name,
            this.properties,
            this.isPublished,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version,
            this.fields.map(f => f.fieldId === field.fieldId ? field : f));
    }

    public replaceFields(fields: FieldDto[], user: string, now?: DateTime): SchemaDetailsDto {
        return new SchemaDetailsDto(
            this.id,
            this.name,
            this.properties,
            this.isPublished,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version,
            fields);
    }

    public removeField(field: FieldDto, user: string, now?: DateTime): SchemaDetailsDto {
        return new SchemaDetailsDto(
            this.id,
            this.name,
            this.properties,
            this.isPublished,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            this.version,
            this.fields.filter(f => f.fieldId !== field.fieldId));
    }
}

export class FieldDto {
    constructor(
        public readonly fieldId: number,
        public readonly name: string,
        public readonly isHidden: boolean,
        public readonly isDisabled: boolean,
        public readonly partitioning: string,
        public readonly properties: FieldPropertiesDto
    ) {
    }

    public show(): FieldDto {
        return new FieldDto(this.fieldId, this.name, false, this.isDisabled, this.partitioning, this.properties);
    }

    public hide(): FieldDto {
        return new FieldDto(this.fieldId, this.name, true, this.isDisabled, this.partitioning, this.properties);
    }

    public enable(): FieldDto {
        return new FieldDto(this.fieldId, this.name, this.isHidden, false, this.partitioning, this.properties);
    }

    public disable(): FieldDto {
        return new FieldDto(this.fieldId, this.name, this.isHidden, true, this.partitioning, this.properties);
    }

    public update(properties: FieldPropertiesDto): FieldDto {
        return new FieldDto(this.fieldId, this.name, this.isHidden, this.isDisabled, this.partitioning, properties);
    }

    public formatValue(value: any): string {
        return this.properties.formatValue(value);
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        return this.properties.createValidators(isOptional);
    }
}

export abstract class FieldPropertiesDto {
    constructor(
        public readonly fieldType: string,
        public readonly label: string | null,
        public readonly hints: string | null,
        public readonly placeholder: string | null,
        public readonly isRequired: boolean,
        public readonly isListField: boolean
    ) {
    }

    public abstract formatValue(value: any): string;

    public abstract createValidators(isOptional: boolean): ValidatorFn[];
}

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly editor: string,
        public readonly defaultValue?: string,
        public readonly pattern?: string,
        public readonly patternMessage?: string,
        public readonly minLength?: number,
        public readonly maxLength?: number,
        public readonly allowedValues?: string[]
    ) {
        super('String', label, hints, placeholder, isRequired, isListField);
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
            validators.push(ValidatorsEx.validValues(this.allowedValues));
        }

        return validators;
    }
}

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly editor: string,
        public readonly defaultValue?: number,
        public readonly maxValue?: number,
        public readonly minValue?: number,
        public readonly allowedValues?: number[]
    ) {
        super('Number', label, hints, placeholder, isRequired, isListField);
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
            validators.push(ValidatorsEx.validValues(this.allowedValues));
        }

        return validators;
    }
}

export class DateTimeFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly editor: string,
        public readonly defaultValue?: string,
        public readonly maxValue?: string,
        public readonly minValue?: string,
        public readonly calculatedDefaultValue?: string
    ) {
        super('DateTime', label, hints, placeholder, isRequired, isListField);
    }

    public formatValue(value: any): string {
        if (!value) {
            return '';
        }

        try {
            const parsed = DateTime.parseISO_UTC(value);

            if (this.editor === 'Date') {
                return parsed.toStringFormat('YYYY-MM-DD');
            } else {
                return parsed.toStringFormat('YYYY-MM-DD HH:mm:ss');
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
}

export class BooleanFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly editor: string,
        public readonly defaultValue?: boolean
    ) {
        super('Boolean', label, hints, placeholder, isRequired, isListField);
    }

    public formatValue(value: any): string {
        if (value === null || value === undefined) {
            return '';
        }

        return value ? '✔' : '-';
    }

    public createValidators(isOptional: boolean): ValidatorFn[] {
        const validators: ValidatorFn[] = [];

        if (this.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        return validators;
    }
}

export class GeolocationFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly editor: string
    ) {
        super('Geolocation', label, hints, placeholder, isRequired, isListField);
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
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly minItems?: number,
        public readonly maxItems?: number,
        public readonly schemaId?: string
    ) {
        super('References', label, hints, placeholder, isRequired, isListField);
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
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean,
        public readonly minItems?: number,
        public readonly maxItems?: number
    ) {
        super('Assets', label, hints, placeholder, isRequired, isListField);
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

export class JsonFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | null, hints: string | null, placeholder: string | null,
        isRequired: boolean,
        isListField: boolean
    ) {
        super('Json', label, hints, placeholder, isRequired, isListField);
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
        public readonly label: string,
        public readonly hints: string
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

@Injectable()
export class SchemasService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly localCache: LocalCacheService
    ) {
    }

    public getSchemas(appName: string): Observable<SchemaDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.getVersioned(this.http, url)
                .map(response => {
                    const items: any[] = response;

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

    public getSchema(appName: string, id: string, version?: Version): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${id}`);

        return HTTP.getVersioned(this.http, url)
                .map(response => {
                    const fields = response.fields.map((item: any) => {
                        const propertiesDto =
                            createProperties(
                                item.properties.fieldType,
                                item.properties);

                        return new FieldDto(
                            item.fieldId,
                            item.name,
                            item.isHidden,
                            item.isDisabled,
                            item.partitioning,
                            propertiesDto);
                    });

                    const properties = new SchemaPropertiesDto(response.properties.label, response.properties.hints);

                    return new SchemaDetailsDto(
                        response.id,
                        response.name, properties,
                        response.isPublished,
                        response.createdBy,
                        response.lastModifiedBy,
                        DateTime.parseISO_UTC(response.created),
                        DateTime.parseISO_UTC(response.lastModified),
                        new Version(response.version.toString()),
                        fields);
                })
                .catch(error => {
                    if (error instanceof HttpErrorResponse && error.status === 404) {
                        const cached = this.localCache.get(`schema.${appName}.${id}`);

                        if (cached) {
                            return Observable.of(cached);
                        }
                    }

                    return Observable.throw(error);
                })
                .pretifyError('Failed to load schema. Please reload.');
    }

    public postSchema(appName: string, dto: CreateSchemaDto, user: string, now?: DateTime, version?: Version): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return HTTP.postVersioned(this.http, url, dto, version)
                .map(response => {
                    now = now || DateTime.now();

                    return new SchemaDetailsDto(
                        response.id,
                        dto.name,
                        dto.properties || new SchemaPropertiesDto(null, null),
                        false,
                        user,
                        user,
                        now,
                        now,
                        version,
                        dto.fields || []);
                })
                .do(schema => {
                    this.localCache.set(`service.${appName}.${schema.id}`, schema, 5000);
                    this.localCache.set(`service.${appName}.${schema.name}`, schema, 5000);
                })
                .pretifyError('Failed to create schema. Please reload.');
    }

    public postField(appName: string, schemaName: string, dto: AddFieldDto, version?: Version): Observable<FieldDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields`);

        return HTTP.postVersioned(this.http, url, dto, version)
                .map(response => {
                    return new FieldDto(
                        response.id,
                        dto.name,
                        false,
                        false,
                        dto.partitioning,
                        dto.properties);
                })
                .pretifyError('Failed to add field. Please reload.');
    }

    public putSchema(appName: string, schemaName: string, dto: UpdateSchemaDto, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .pretifyError('Failed to update schema. Please reload.');
    }

    public putFieldOrdering(appName: string, schemaName: string, dto: number[], version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/ordering`);

        return HTTP.putVersioned(this.http, url, { fieldIds: dto }, version)
                .pretifyError('Failed to reorder fields. Please reload.');
    }

    public publishSchema(appName: string, schemaName: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/publish`);

        return HTTP.putVersioned(this.http, url, {}, version)
                .pretifyError('Failed to publish schema. Please reload.');
    }

    public unpublishSchema(appName: string, schemaName: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/unpublish`);

        return HTTP.putVersioned(this.http, url, {}, version)
                .pretifyError('Failed to unpublish schema. Please reload.');
    }

    public putField(appName: string, schemaName: string, fieldId: number, dto: UpdateFieldDto, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .pretifyError('Failed to update field. Please reload.');
    }

    public enableField(appName: string, schemaName: string, fieldId: number, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/enable`);

        return HTTP.putVersioned(this.http, url, {}, version)
                .pretifyError('Failed to enable field. Please reload.');
    }

    public disableField(appName: string, schemaName: string, fieldId: number, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/disable`);

        return HTTP.putVersioned(this.http, url, {}, version)
                .pretifyError('Failed to disable field. Please reload.');
    }

    public showField(appName: string, schemaName: string, fieldId: number, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/show`);

        return HTTP.putVersioned(this.http, url, {}, version)
                .pretifyError('Failed to show field. Please reload.');
    }

    public hideField(appName: string, schemaName: string, fieldId: number, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/hide`);

        return HTTP.putVersioned(this.http, url, {}, version)
                .pretifyError('Failed to hide field. Please reload.');
    }

    public deleteField(appName: string, schemaName: string, fieldId: number, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .pretifyError('Failed to delete field. Please reload.');
    }

    public deleteSchema(appName: string, schemaName: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .pretifyError('Failed to delete schema. Please reload.');
    }
}