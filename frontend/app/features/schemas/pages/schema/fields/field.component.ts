/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AppSettingsDto, createProperties, DialogModel, EditFieldForm, fadeAnimation, LanguageDto, ModalModel, NestedFieldDto, RootFieldDto, SchemaDto, SchemasState, sorted } from '@app/shared';

@Component({
    selector: 'sqx-field[field][languages][schema][settings]',
    styleUrls: ['./field.component.scss'],
    templateUrl: './field.component.html',
    animations: [
        fadeAnimation,
    ],
})
export class FieldComponent implements OnChanges {
    @Input()
    public field: NestedFieldDto | RootFieldDto;

    @Input()
    public schema: SchemaDto;

    @Input()
    public parent: RootFieldDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public settings: AppSettingsDto;

    public get isLocalizable() {
        return (this.parent && this.parent.isLocalizable) || this.field['isLocalizable'];
    }

    public dropdown = new ModalModel();

    public trackByFieldFn: (_index: number, field: NestedFieldDto) => any;

    public isEditing = false;
    public isEditable?: boolean | null;

    public editForm: EditFieldForm;

    public addFieldDialog = new DialogModel();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState,
    ) {
        this.trackByFieldFn = this.trackByField.bind(this);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['field']) {
            this.isEditable = this.field.canUpdate;

            this.editForm = new EditFieldForm(this.formBuilder, this.field.properties);
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
        this.schemasState.orderFields(this.schema, sorted(event), this.field as any).subscribe();
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
                .subscribe({
                    next: () => {
                        this.editForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        this.editForm.submitFailed(error);
                    },
                });
        }
    }

    public trackByField(_index: number, field: NestedFieldDto) {
        return field.fieldId + this.schema.id;
    }
}
