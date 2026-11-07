/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, computed, ElementRef, input, output, signal, viewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AddFieldForm, AppSettingsDto, createProperties, DropdownMenuComponent, EditFieldForm, FieldDto, fieldTypes, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormRowComponent, LanguagesState, ModalDialogComponent, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, SchemasState, TooltipDirective, TranslatePipe, Types, UpdateFieldDto } from '@app/shared';
import { FieldFormComponent } from './forms/field-form.component';


type SaveNavigationMode = 'Close' | 'Add' | 'Edit';

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html',
    imports: [
        AsyncPipe,
        DropdownMenuComponent,
        FieldFormComponent,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class FieldWizardComponent {
    private readonly nameInput = viewChild<ElementRef<HTMLElement>>('nameInput');

    /** The schema to add the field to. */
    public readonly schema = input.required<SchemaDto>();

    /** The settings of the app. */
    public readonly settings = input.required<AppSettingsDto>();

    /** The parent array field, if a nested field is added. */
    public readonly parent = input<FieldDto | null>();

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    protected readonly fieldTypes = computed(() => {
        if (this.parent()) {
            return fieldTypes.filter(x => x.type !== 'Array');
        }

        return fieldTypes;
    });

    protected readonly addFieldForm = new AddFieldForm();
    protected readonly addFieldModal = new ModalModel();

    protected readonly editing = signal<{ field: FieldDto; form: EditFieldForm } | null>(null);

    protected readonly isLocalizable = computed(() => {
        return this.parent()?.isLocalizable || (this.editing()?.field as any)?.['isLocalizable'];
    });

    constructor(
        private readonly schemasState: SchemasState,
        protected readonly languagesState: LanguagesState,
    ) {
    }

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected addField(navigationMode: SaveNavigationMode) {
        const value = this.addFieldForm.submit();
        if (!value) {
            return;
        }

        this.schemasState.addField(this.schema(), value, this.parent())
            .subscribe({
                next: dto => {
                    switch (navigationMode) {
                        case 'Add':
                            this.addFieldForm.submitCompleted({ newValue: { ...DEFAULT_FIELD } });
                            this.focusName();
                            break;

                        case 'Edit':
                            this.startEditing(dto as any);
                            break;

                        case 'Close':
                            this.emitClose();
                    }
                },
                error: error => {
                    this.addFieldForm.submitFailed(error);
                },
            });
    }

    protected save(navigationMode: SaveNavigationMode) {
        const editing = this.editing();
        if (!editing) {
            return;
        }

        const value = editing.form.submit();
        if (!value) {
            return;
        }

        const properties = createProperties(editing.field.properties.fieldType as any, value);

        this.schemasState.updateField(this.schema(), editing.field, new UpdateFieldDto({ properties }))
            .subscribe({
                next: () => {
                    switch (navigationMode) {
                        case 'Add':
                            this.editing.set(null);
                            break;
                        case 'Close': {
                            this.emitClose();
                        }
                    }
                },
                error: error => {
                    editing.form.submitFailed(error);
                },
            });
    }

    private startEditing(field: FieldDto) {
        const form = new EditFieldForm(field.properties);
        form.load(field.properties);

        this.editing.set({ field, form });
    }

    private focusName() {
        const nameInput = this.nameInput()?.nativeElement;
        if (nameInput && Types.isFunction(nameInput.focus)) {
            nameInput.focus();
        }
    }
}

const DEFAULT_FIELD = { name: '', partitioning: 'invariant', properties: createProperties('String') };