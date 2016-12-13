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
    handleError
} from 'framework';

import { AuthService } from './auth.service';

export class SchemaDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime
    ) {
    }
}

export class SchemaDetailsDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly fields: FieldDto[]
    ) {
    }
}

export class FieldDto {
    constructor(
        public readonly name: string,
        public readonly isHidden: boolean,
        public readonly isDisabled: boolean,
        public readonly properties: FieldPropertiesDto
    ) {
    }
}

export abstract class FieldPropertiesDto {
    constructor(
        public readonly label: string,
        public readonly hints: string,
        public readonly placeholder: string,
        public readonly isRequired: boolean
    ) {
    }
}

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string, hints: string, placeholder: string, isRequired: boolean,
        public readonly defaultValue: number | null,
        public readonly maxValue: number | null,
        public readonly minValue: number | null,
        public readonly allowedValues: number[]
    ) {
        super(label, hints, placeholder, isRequired);

        this['$type'] = 'Number';
    }
}

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    constructor(label: string, hints: string, placeholder: string, isRequired: boolean,
        public readonly defaultValue: string,
        public readonly pattern: string,
        public readonly patternMessage: string,
        public readonly minLength: number | null,
        public readonly maxLength: number | null,
        public readonly allowedValues: number[]
    ) {
        super(label, hints, placeholder, isRequired);

        this['$type'] = 'String';
    }
}

export class UpdateSchemaDto {
    constructor(
        public readonly label: string,
        public readonly hints: string
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
                            DateTime.parseISO_UTC(item.lastModified));
                    });
                })
                .catch(response => handleError('Failed to load schemas. Please reload.', response));
    }

    public getSchema(appName: string, id: string): Observable<SchemaDetailsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/{id}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const fields = response.fields.map((item: any) => {
                        const typeName = item['$type'];

                        let properties = item.properties;
                        let propertiesDto: FieldPropertiesDto;

                        if (typeName === 'String') {
                            propertiesDto = new StringFieldPropertiesDto(
                                properties.label,
                                properties.hints,
                                properties.placeholder,
                                properties.isRequired,
                                properties.defaultValue,
                                properties.pattern,
                                properties.patternMessage,
                                properties.minLength,
                                properties.maxLength,
                                properties.allowedValues);
                        } else {
                            propertiesDto = new NumberFieldPropertiesDto(
                                properties.label,
                                properties.hints,
                                properties.placeholder,
                                properties.isRequired,
                                properties.defaultValue,
                                properties.minValue,
                                properties.maxValue,
                                properties.allowedValues);
                        }

                        return new FieldDto(
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
                        fields);
                })
                .catch(response => handleError('Failed to load schema. Please reload.', response));
    }
}