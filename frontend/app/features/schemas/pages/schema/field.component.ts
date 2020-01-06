/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    createProperties,
    DialogModel,
    EditFieldForm,
    fadeAnimation,
    ModalModel,
    NestedFieldDto,
    PatternDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemasState,
    sorted
} from '@app/shared';

@Component({
    selector: 'sqx-field',
    styleUrls: ['./field.component.scss'],
    templateUrl: './field.component.html',
    animations: [
        fadeAnimation
    ]
})
export class FieldComponent implements OnChanges {
    @Input()
    public field: NestedFieldDto | RootFieldDto;

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public parent: RootFieldDto;

    @Input()
    public patterns: ReadonlyArray<PatternDto>;

    public dropdown = new ModalModel();

    public trackByFieldFn: (index: number, field: NestedFieldDto) => any;

    public isEditing = false;
    public isEditable = false;

    public editForm = new EditFieldForm(this.formBuilder);

    public addFieldDialog = new DialogModel();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
        this.trackByFieldFn = this.trackByField.bind(this);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['field']) {
            this.isEditable = this.field.canUpdate;

            this.editForm.load(this.field.properties);
        }
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;

        if (this.isEditing) {
            this.editForm.load(this.field.properties);
        }
    }

    public deleteField() {
        this.schemasState.deleteField(this.schema, this.field);
    }

    public enableField() {
        this.schemasState.enableField(this.schema, this.field);
    }

    public disableField() {
        this.schemasState.disableField(this.schema, this.field);
    }

    public showField() {
        this.schemasState.showField(this.schema, this.field);
    }

    public hideField() {
        this.schemasState.hideField(this.schema, this.field);
    }

    public sortFields(event: CdkDragDrop<ReadonlyArray<NestedFieldDto>>) {
        this.schemasState.orderFields(this.schema, sorted(event), <any>this.field).subscribe();
    }

    public lockField() {
        this.schemasState.lockField(this.schema, this.field);
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            const properties = createProperties(this.field.properties.fieldType, value);

            this.schemasState.updateField(this.schema, this.field, { properties })
                .subscribe(() => {
                    this.editForm.submitCompleted();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }

    public trackByField(index: number, field: NestedFieldDto) {
        return field.fieldId + this.schema.id;
    }
}