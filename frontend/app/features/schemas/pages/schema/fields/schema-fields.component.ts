/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, Input, OnInit } from '@angular/core';
import { DialogModel, FieldDto, fieldTypes, LanguagesState, PatternsState, SchemaDetailsDto, SchemasState, sorted } from '@app/shared';

@Component({
    selector: 'sqx-schema-fields',
    styleUrls: ['./schema-fields.component.scss'],
    templateUrl: './schema-fields.component.html'
})
export class SchemaFieldsComponent implements OnInit {
    public fieldTypes = fieldTypes;

    @Input()
    public schema: SchemaDetailsDto;

    public addFieldDialog = new DialogModel();

    public trackByFieldFn: (_index: number, field: FieldDto) => any;

    constructor(
        public readonly schemasState: SchemasState,
        public readonly languageState: LanguagesState,
        public readonly patternsState: PatternsState
    ) {
        this.trackByFieldFn = this.trackByField.bind(this);
    }

    public ngOnInit() {
        this.patternsState.load();

        this.languageState.load();
    }

    public sortFields(event: CdkDragDrop<ReadonlyArray<FieldDto>>) {
        this.schemasState.orderFields(this.schema, sorted(event)).subscribe();
    }

    public trackByField(_index: number, field: FieldDto) {
        return field.fieldId + this.schema.id;
    }
}