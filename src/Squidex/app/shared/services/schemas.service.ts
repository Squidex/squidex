/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    DateTime,
    EntityCreatedDto
} from 'framework';

import { AuthService } from './auth.service';

export function createProperties(fieldType: string, values: Object | null = null): FieldPropertiesDto {
    let properties: FieldPropertiesDto;

    switch (fieldType) {
        case 'number':
            properties =
                new NumberFieldPropertiesDto(
                    undefined, undefined, undefined, false, 'Input',
                    undefined, undefined, undefined, undefined);
            break;
        case 'string':
            properties =
                new StringFieldPropertiesDto(
                    undefined, undefined, undefined, false, 'Input',
                    undefined, undefined, undefined, undefined, undefined, undefined);
            break;
        case 'boolean':
            properties =
                new BooleanFieldPropertiesDto(
                    undefined, undefined, undefined, false, 'Checkbox',
                    undefined);
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
        public readonly label: string | undefined,
        public readonly isPublished: boolean,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime
    ) {
    }
}

export class SchemaDetailsDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly label: string,
        public readonly hints: string,
        public readonly isPublished: boolean,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly fields: FieldDto[]
    ) {
    }
}

export class FieldDto {
    constructor(
        public readonly fieldId: number,
        public readonly name: string,
        public readonly isHidden: boolean,
        public readonly isDisabled: boolean,
        public readonly properties: FieldPropertiesDto
    ) {
    }
}

export abstract class FieldPropertiesDto {
    constructor(
        public readonly label?: string,
        public readonly hints?: string,
        public readonly placeholder?: string,
        public readonly isRequired: boolean = false
    ) {
    }
}

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | undefined, hints: string | undefined, placeholder: string | undefined, isRequired: boolean,
        public readonly editor: string,
        public readonly defaultValue?: number,
        public readonly maxValue?: number,
        public readonly minValue?: number,
        public readonly allowedValues?: number[]
    ) {
        super(label, hints, placeholder, isRequired);

        this['fieldType'] = 'number';
    }
}

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | undefined, hints: string | undefined, placeholder: string | undefined, isRequired: boolean,
        public readonly editor: string,
        public readonly defaultValue?: string,
        public readonly pattern?: string,
        public readonly patternMessage?: string,
        public readonly minLength?: number | null,
        public readonly maxLength?: number | null,
        public readonly allowedValues?: string[]
    ) {
        super(label, hints, placeholder, isRequired);

        this['fieldType'] = 'string';
    }
}

export class BooleanFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string | undefined, hints: string | undefined, placeholder: string | undefined, isRequired: boolean,
        public readonly editor: string,
        public readonly defaultValue?: boolean
    ) {
        super(label, hints, placeholder, isRequired);

        this['fieldType'] = 'boolean';
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
        public readonly name: string
    ) {
    }
}

@Injectable()
export class SchemasService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getSchemas(appName: string): Observable<SchemaDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new SchemaDto(
                            item.id,
                            item.name,
                            item.label,
                            item.isPublished,
                            item.createdBy,
                            item.lastModifiedBy,
                            DateTime.parseISO_UTC(item.created),
                            DateTime.parseISO_UTC(item.lastModified));
                    });
                })
                .catchError('Failed to load schemas. Please reload.');
    }

    public getSchema(appName: string, id: string): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${id}`);

        return this.authService.authGet(url)
                .map(response => response.json())
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
                            propertiesDto);
                    });

                    return new SchemaDetailsDto(
                        response.id,
                        response.name,
                        response.label,
                        response.hints,
                        response.isPublished,
                        response.createdBy,
                        response.lastModifiedBy,
                        DateTime.parseISO_UTC(response.created),
                        DateTime.parseISO_UTC(response.lastModified),
                        fields);
                })
                .catchError('Failed to load schema. Please reload.');
    }

    public postSchema(appName: string, dto: CreateSchemaDto): Observable<EntityCreatedDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new EntityCreatedDto(response.id);
                })
                .catchError('Failed to create schema. Please reload.');
    }

    public postField(appName: string, schemaName: string, dto: AddFieldDto): Observable<EntityCreatedDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new EntityCreatedDto(response.id);
                })
                .catchError('Failed to add field. Please reload.');
    }

    public putSchema(appName: string, schemaName: string, dto: UpdateSchemaDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}`);

        return this.authService.authPut(url, dto)
                .catchError('Failed to update schema. Please reload.');
    }

    public publishSchema(appName: string, schemaName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/publish`);

        return this.authService.authPut(url, {})
                .catchError('Failed to publish schema. Please reload.');
    }

    public unpublishSchema(appName: string, schemaName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/unpublish`);

        return this.authService.authPut(url, {})
                .catchError('Failed to unpublish schema. Please reload.');
    }

    public putField(appName: string, schemaName: string, fieldId: number, dto: UpdateFieldDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}`);

        return this.authService.authPut(url, dto)
                .catchError('Failed to update field. Please reload.');
    }

    public enableField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/enable`);

        return this.authService.authPut(url, {})
                .catchError('Failed to enable field. Please reload.');
    }

    public disableField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/disable`);

        return this.authService.authPut(url, {})
                .catchError('Failed to disable field. Please reload.');
    }

    public showField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/show`);

        return this.authService.authPut(url, {})
                .catchError('Failed to show field. Please reload.');
    }

    public hideField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/hide`);

        return this.authService.authPut(url, {})
                .catchError('Failed to hide field. Please reload.');
    }

    public deleteField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}`);

        return this.authService.authDelete(url)
                .catchError('Failed to delete field. Please reload.');
    }
}