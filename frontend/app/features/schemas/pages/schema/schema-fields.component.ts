/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:no-shadowed-variable

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, Input } from '@angular/core';

import {
    DialogModel,
    fadeAnimation,
    FieldDto,
    fieldTypes,
    PatternsState,
    SchemaDetailsDto,
    SchemasState,
    sorted
} from '@app/shared';

@Component({
    selector: 'sqx-schema-fields',
    styleUrls: ['./schema-fields.component.scss'],
    templateUrl: './schema-fields.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaFieldsComponent {
    public fieldTypes = fieldTypes;

    @Input()
    public schema: SchemaDetailsDto;

    public addFieldDialog = new DialogModel();

    public trackByFieldFn: (index: number, field: FieldDto) => any;

    constructor(
        public readonly schemasState: SchemasState,
        public readonly patternsState: PatternsState
    ) {
        this.trackByFieldFn = this.trackByField.bind(this);
    }

    public sortFields(event: CdkDragDrop<ReadonlyArray<FieldDto>>) {
        this.schemasState.orderFields(this.schema, sorted(event)).subscribe();
    }

    public trackByField(index: number, field: FieldDto) {
        return field.fieldId + this.schema.id;
    }
}