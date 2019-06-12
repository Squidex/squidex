/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    createProperties,
    DialogModel,
    EditFieldForm,
    fadeAnimation,
    ImmutableArray,
    ModalModel,
    NestedFieldDto,
    PatternDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemasState
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
    public patterns: ImmutableArray<PatternDto>;

    public dropdown = new ModalModel();

    public editing = false;
    public editForm = new EditFieldForm(this.formBuilder);

    public addFieldDialog = new DialogModel();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['field']) {
            this.editForm.load(this.field.properties);

            if (this.field.isLocked) {
                this.editForm.form.disable();
            }
        }
    }

    public toggleEditing() {
        this.editing = !this.editing;

        if (this.editing) {
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

    public sortFields(fields: NestedFieldDto[]) {
        this.schemasState.sortFields(this.schema, fields, <any>this.field).subscribe();
    }

    public lockField() {
        this.schemasState.lockField(this.schema, this.field);
    }

    public trackByField(index: number, field: NestedFieldDto) {
        return field.fieldId + this.schema.id;
    }

    public save() {
        const value = this.editForm.submit();

        if (value) {
            const properties = createProperties(this.field.properties['fieldType'], value);

            this.schemasState.updateField(this.schema, this.field, { properties })
                .subscribe(() => {
                    this.editForm.submitCompleted();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}

