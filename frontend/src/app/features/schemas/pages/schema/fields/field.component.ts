/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { AppSettingsDto, createProperties, DialogModel, EditFieldForm, FieldDto, LanguageDto, ModalModel, NestedFieldDto, RootFieldDto, SchemaDto, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-field[field][languages][schema][settings]',
    styleUrls: ['./field.component.scss'],
    templateUrl: './field.component.html',
})
export class FieldComponent implements OnChanges {
    @Input()
    public field!: NestedFieldDto | RootFieldDto;

    @Input()
    public schema!: SchemaDto;

    @Input()
    public plain = false;

    @Input()
    public parent?: RootFieldDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public settings!: AppSettingsDto;

    public dropdown = new ModalModel();

    public isEditing = false;
    public isEditable?: boolean | null;

    public editForm!: EditFieldForm;

    public fieldWizard = new DialogModel();

    public get isLocalizable() {
        return (this.parent && this.parent.isLocalizable) || this.field['isLocalizable'];
    }

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['field']) {
            this.isEditable = this.field.canUpdate;

            this.editForm = new EditFieldForm(this.field.properties);
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

    public sortFields(fields: ReadonlyArray<FieldDto>) {
        this.schemasState.orderFields(this.schema, fields, this.field as any);
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
}
