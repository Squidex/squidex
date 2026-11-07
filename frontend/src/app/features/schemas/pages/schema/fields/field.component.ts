/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, AppSettingsDto, ConfirmClickDirective, createProperties, DialogModel, DropdownMenuComponent, EditFieldForm, FieldDto, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, SchemasState, TooltipDirective, TourStepDirective, TranslatePipe, UpdateFieldDto } from '@app/shared';
import { FieldWizardComponent } from './field-wizard.component';
import { FieldFormComponent } from './forms/field-form.component';
import { SortableFieldListComponent } from './sortable-field-list.component';

@Component({
    selector: 'sqx-field',
    styleUrls: ['./field.component.scss'],
    templateUrl: './field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
    /** The field to render. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();

    /** Renders the field without type icon, status and badges, e.g. for separators. */
    public readonly plain = input(false, { transform: booleanAttribute });

    /** The parent array field, if the field is a nested field. */
    public readonly parent = input<FieldDto>();

    /** The languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The settings of the app. */
    public readonly settings = input.required<AppSettingsDto>();

    public readonly dropdown = new ModalModel();
    public readonly fieldWizard = new DialogModel();

    public readonly isEditing = signal(false);

    public readonly isEditable = computed(() => {
        return this.field().canUpdate;
    });

    public readonly editForm = computed(() => {
        const properties = this.field().properties;

        const editForm = new EditFieldForm(properties);
        editForm.load(properties);
        return editForm;
    });

    public readonly isLocalizable = computed(() => {
        return this.parent()?.isLocalizable || (this.field() as any)['isLocalizable'];
    });

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public toggleEditing() {
        const isEditing = !this.isEditing();

        this.isEditing.set(isEditing);

        if (isEditing) {
            this.editForm().load(this.field().properties);
        }
    }

    public deleteField() {
        this.schemasState.deleteField(this.schema(), this.field());
    }

    public enableField() {
        this.schemasState.enableField(this.schema(), this.field());
    }

    public disableField() {
        this.schemasState.disableField(this.schema(), this.field());
    }

    public showField() {
        this.schemasState.showField(this.schema(), this.field());
    }

    public hideField() {
        this.schemasState.hideField(this.schema(), this.field());
    }

    public sortFields(fields: ReadonlyArray<FieldDto>) {
        this.schemasState.orderFields(this.schema(), fields, this.field() as any);
    }

    public lockField() {
        this.schemasState.lockField(this.schema(), this.field());
    }

    public save() {
        if (!this.isEditable()) {
            return;
        }

        const editForm = this.editForm();

        const value = editForm.submit();
        if (!value) {
            return;
        }

        const properties = createProperties(this.field().properties.fieldType as any, value);

        this.schemasState.updateField(this.schema(), this.field(), new UpdateFieldDto({ properties }))
            .subscribe({
                next: () => {
                    editForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    editForm.submitFailed(error);
                },
            });
    }
}
