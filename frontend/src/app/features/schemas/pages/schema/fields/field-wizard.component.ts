/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, ElementRef, inject, Injector, input, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { AddFieldForm, AnyFieldDto, AppSettingsDto, createProperties, DropdownMenuComponent, EditFieldForm, FieldDto, fieldTypes, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormRowComponent, LanguagesState, ModalDialogComponent, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, SchemasState, TooltipDirective, TranslatePipe, Types, UpdateFieldDto } from '@app/shared';
import { FieldFormComponent } from './forms/field-form.component';


type SaveNavigationMode = 'Close' | 'Add' | 'Edit';

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html',
    imports: [
        DropdownMenuComponent,
        FieldFormComponent,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class FieldWizardComponent {
    private readonly injector = inject(Injector);
    private readonly schemasState = inject(SchemasState);
    private readonly nameInput = viewChild<ElementRef<HTMLElement>>('nameInput');

    protected readonly languagesState = inject(LanguagesState);

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

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected async addField(navigationMode: SaveNavigationMode) {
        let added: AnyFieldDto | undefined;

        const isAdded = await this.addFieldForm.submit(async value => {
            added = await firstValueFrom(this.schemasState.addField(this.schema(), value, this.parent()));
        });

        if (!isAdded) {
            return;
        }

        switch (navigationMode) {
            case 'Add':
                this.addFieldForm.load({ ...DEFAULT_FIELD });
                this.focusName();
                break;

            case 'Edit':
                this.startEditing(added as FieldDto);
                break;

            case 'Close':
                this.emitClose();
        }
    }

    protected async save(navigationMode: SaveNavigationMode) {
        const editing = this.editing();
        if (!editing) {
            return;
        }

        const isSaved = await editing.form.submit(value => {
            const properties = createProperties(editing.field.properties.fieldType as any, value);

            return this.schemasState.updateField(this.schema(), editing.field, new UpdateFieldDto({ properties }));
        });

        if (!isSaved) {
            return;
        }

        switch (navigationMode) {
            case 'Add':
                this.editing.set(null);
                break;
            case 'Close': {
                this.emitClose();
            }
        }
    }

    private startEditing(field: FieldDto) {
        const form = new EditFieldForm(field.properties, this.injector);
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