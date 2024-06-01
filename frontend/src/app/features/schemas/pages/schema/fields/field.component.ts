/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, forwardRef, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppSettingsDto, ConfirmClickDirective, createProperties, DialogModel, DropdownMenuComponent, EditFieldForm, FieldDto, LanguageDto, ModalDirective, ModalModel, ModalPlacementDirective, NestedFieldDto, RootFieldDto, SchemaDto, SchemasState, TooltipDirective, TourStepDirective, TranslatePipe, TypedSimpleChanges } from '@app/shared';
import { FieldWizardComponent } from './field-wizard.component';
import { FieldFormComponent } from './forms/field-form.component';
import { SortableFieldListComponent } from './sortable-field-list.component';

@Component({
    standalone: true,
    selector: 'sqx-field',
    styleUrls: ['./field.component.scss'],
    templateUrl: './field.component.html',
    imports: [
        ConfirmClickDirective,
        DropdownMenuComponent,
        FieldFormComponent,
        FieldWizardComponent,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
        forwardRef(() => SortableFieldListComponent),
    ],
})
export class FieldComponent {
    @Input({ required: true })
    public field!: NestedFieldDto | RootFieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ transform: booleanAttribute })
    public plain = false;

    @Input()
    public parent?: RootFieldDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ required: true })
    public settings!: AppSettingsDto;

    public dropdown = new ModalModel();

    public isEditing = false;
    public isEditable?: boolean | null;

    public editForm!: EditFieldForm;

    public fieldWizard = new DialogModel();

    public get isLocalizable() {
        return (this.parent && this.parent.isLocalizable) || (this.field as any)['isLocalizable'];
    }

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.field) {
            this.isEditable = this.field.canUpdate;

            this.editForm = new EditFieldForm(this.field.properties);
        }
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;

        this.editForm.load(this.field.properties);
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
