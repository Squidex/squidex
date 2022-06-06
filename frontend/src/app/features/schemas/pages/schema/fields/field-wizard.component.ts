/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { AddFieldForm, AppSettingsDto, createProperties, EditFieldForm, FieldDto, fieldTypes, LanguagesState, RootFieldDto, SchemaDto, SchemasState, Types } from '@app/shared';

const DEFAULT_FIELD = { name: '', partitioning: 'invariant', properties: createProperties('String') };

@Component({
    selector: 'sqx-field-wizard[schema][settings]',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html',
})
export class FieldWizardComponent implements OnInit {
    @ViewChild('nameInput', { static: false })
    public nameInput!: ElementRef<HTMLElement>;

    @Input()
    public schema!: SchemaDto;

    @Input()
    public settings!: AppSettingsDto;

    @Input()
    public parent: RootFieldDto | null | undefined;

    @Output()
    public complete = new EventEmitter();

    public fieldTypes = fieldTypes;
    public field!: FieldDto;

    public addFieldForm = new AddFieldForm();

    public editForm?: EditFieldForm;

    public get isLocalizable() {
        return (this.parent && this.parent.isLocalizable) || this.field['isLocalizable'];
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

    public emitComplete() {
        this.complete.emit();
    }

    public addField(addNew: boolean, edit = false) {
        const value = this.addFieldForm.submit();

        if (value) {
            this.schemasState.addField(this.schema, value, this.parent)
                .subscribe({
                    next: dto => {
                        this.field = dto;

                        this.addFieldForm.submitCompleted({ newValue: { ...DEFAULT_FIELD } });

                        if (addNew) {
                            if (Types.isFunction(this.nameInput.nativeElement.focus)) {
                                this.nameInput.nativeElement.focus();
                            }
                        } else if (edit) {
                            this.editForm = new EditFieldForm(this.field.properties);
                            this.editForm.load(this.field.properties);
                        } else {
                            this.emitComplete();
                        }
                    },
                    error: error => {
                        this.addFieldForm.submitFailed(error);
                    },
                });
        }
    }

    public save(addNew = false) {
        if (!this.editForm) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            const properties = createProperties(this.field.properties.fieldType, value);

            this.schemasState.updateField(this.schema, this.field as RootFieldDto, { properties })
                .subscribe({
                    next: () => {
                        this.editForm!.submitCompleted();

                        if (addNew) {
                            this.editForm = undefined;
                        } else {
                            this.emitComplete();
                        }
                    },
                    error: error => {
                        this.editForm!.submitFailed(error);
                    },
                });
        }
    }
}
