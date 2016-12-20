/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    DateTime,
    EntityCreatedDto,
    handleError
} from 'framework';

import { AuthService } from './auth.service';

export function createProperties(fieldType: string, values: {} | null = null): FieldPropertiesDto {
    let properties: FieldPropertiesDto;

    switch (fieldType) {
        case 'number':
            properties =
                new NumberFieldPropertiesDto(
                    undefined, undefined, undefined, false,
                    undefined, undefined, undefined, undefined, undefined);
            break;
        case 'string':
            properties =
                new StringFieldPropertiesDto(
                    undefined, undefined, undefined, false,
                    undefined, undefined, undefined, undefined, undefined, undefined, undefined);
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
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string
    ) {
    }
}

export class SchemaDetailsDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
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
    constructor(label: string, hints: string, placeholder: string, isRequired: boolean,
        public readonly editor: string,
        public readonly defaultValue: number | null,
        public readonly maxValue: number | null,
        public readonly minValue: number | null,
        public readonly allowedValues: number[] | undefined
    ) {
        super(label, hints, placeholder, isRequired);

        this['fieldType'] = 'number';
    }
}

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string, hints: string, placeholder: string, isRequired: boolean,
        public readonly editor: string,
        public readonly defaultValue: string,
        public readonly pattern: string,
        public readonly patternMessage: string,
        public readonly minLength: number | null,
        public readonly maxLength: number | null,
        public readonly allowedValues: string[]
    ) {
        super(label, hints, placeholder, isRequired);

        this['fieldType'] = 'string';
    }
}

export class UpdateSchemaDto {
    constructor(
        public readonly label: string,
        public readonly hints: string
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
                            DateTime.parseISO_UTC(item.created),
                            DateTime.parseISO_UTC(item.lastModified),
                            item.createdBy,
                            item.lastModifiedBy);
                    });
                })
                .catch(response => handleError('Failed to load schemas. Please reload.', response));
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
                        DateTime.parseISO_UTC(response.created),
                        DateTime.parseISO_UTC(response.lastModified),
                        response.createdBy,
                        response.lastModifiedBy,
                        fields);
                })
                .catch(response => handleError('Failed to load schema. Please reload.', response));
    }

    public postSchema(appName: string, dto: CreateSchemaDto): Observable<EntityCreatedDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new EntityCreatedDto(response.id);
                })
                .catch(response => handleError('Failed to create schema. Please reload.', response));
    }

    public postField(appName: string, schemaName: string, dto: AddFieldDto): Observable<EntityCreatedDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => {
                    return new EntityCreatedDto(response.id);
                })
                .catch(response => handleError('Failed to add field. Please reload.', response));
    }

    public putField(appName: string, schemaName: string, fieldId: number, dto: UpdateFieldDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/`);

        return this.authService.authPut(url, dto)
                .catch(response => handleError('Failed to update field. Please reload.', response));
    }

    public enableField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/enable/`);

        return this.authService.authPut(url, {})
                .catch(response => handleError('Failed to enable field. Please reload.', response));
    }

    public disableField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/disable/`);

        return this.authService.authPut(url, {})
                .catch(response => handleError('Failed to disable field. Please reload.', response));
    }

    public showField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/show/`);

        return this.authService.authPut(url, {})
                .catch(response => handleError('Failed to show field. Please reload.', response));
    }

    public hideField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/hide/`);

        return this.authService.authPut(url, {})
                .catch(response => handleError('Failed to hide field. Please reload.', response));
    }

    public deleteField(appName: string, schemaName: string, fieldId: number): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/fields/${fieldId}/`);

        return this.authService.authDelete(url)
                .catch(response => handleError('Failed to delete field. Please reload.', response));
    }
}