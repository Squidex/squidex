/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AddFieldForm, AppSettingsDto, ControlErrorsComponent, createProperties, DropdownMenuComponent, EditFieldForm, FieldDto, fieldTypes, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, LanguagesState, ModalDialogComponent, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, SchemasState, TooltipDirective, TranslatePipe, Types, UpdateFieldDto } from '@app/shared';
import { FieldFormComponent } from './forms/field-form.component';


type SaveNavigationMode = 'Close' | 'Add' | 'Edit';

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html',
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        DropdownMenuComponent,
        FieldFormComponent,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormHintComponent,
        FormsModule,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class FieldWizardComponent implements OnInit {
    @ViewChild('nameInput', { static: false })
    public nameInput!: ElementRef<HTMLElement>;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public settings!: AppSettingsDto;

    @Input()
    public parent: FieldDto | null | undefined;

    @Output()
    public dialogClose = new EventEmitter();

    public fieldTypes = fieldTypes;

    public addFieldForm = new AddFieldForm();
    public addFieldModal = new ModalModel();

    public editField!: FieldDto;
    public editForm?: EditFieldForm;

    public get isLocalizable() {
        return (this.parent && this.parent.isLocalizable) || (this.editField as any)['isLocalizable'];
    }

    constructor(
        private readonly schemasState: SchemasState,
        public readonly languagesState: LanguagesState,
    ) {
    }

    public ngOnInit() {
        if (this.parent) {
            this.fieldTypes = this.fieldTypes.filter(x => x.type !== 'Array');
        }
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public addField(navigationMode: SaveNavigationMode) {
        const value = this.addFieldForm.submit();
        if (!value) {
            return;
        }

        this.schemasState.addField(this.schema, value, this.parent)
            .subscribe({
                next: dto => {
                    switch (navigationMode) {
                        case 'Add':
                            this.addFieldForm.submitCompleted({ newValue: { ...DEFAULT_FIELD } });

                            if (Types.isFunction(this.nameInput.nativeElement.focus)) {
                                this.nameInput.nativeElement.focus();
                            }

                            break;

                        case 'Edit':
                            this.editField = dto as any;
                            this.editForm = new EditFieldForm(this.editField.properties);
                            this.editForm.load(this.editField.properties);
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

    public save(navigationMode: SaveNavigationMode) {
        if (!this.editForm) {
            return;
        }

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        const properties = createProperties(this.editField.properties.fieldType as any, value);

        this.schemasState.updateField(this.schema, this.editField, new UpdateFieldDto({ properties }))
            .subscribe({
                next: () => {
                    switch (navigationMode) {
                        case 'Add':
                            this.editForm = undefined;
                            break;
                        case 'Close': {
                            this.emitClose();
                        }
                    }
                },
                error: error => {
                    this.editForm!.submitFailed(error);
                },
            });
    }
}

const DEFAULT_FIELD = { name: '', partitioning: 'invariant', properties: createProperties('String') };