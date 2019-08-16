/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddFieldForm,
    createProperties,
    EditFieldForm,
    FieldDto,
    fieldTypes,
    PatternsState,
    RootFieldDto,
    SchemaDetailsDto,
    SchemasState,
    Types
} from '@app/shared';

const DEFAULT_FIELD = { name: '', partitioning: 'invariant', properties: createProperties('String') };

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html'
})
export class FieldWizardComponent implements OnInit {
    @ViewChild('nameInput', { static: false })
    public nameInput: ElementRef<HTMLElement>;

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public parent: RootFieldDto;

    @Output()
    public complete = new EventEmitter();

    public fieldTypes = fieldTypes;
    public field: FieldDto;

    public addFieldForm = new AddFieldForm(this.formBuilder);

    public editing = false;
    public editForm = new EditFieldForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState,
        public readonly patternsState: PatternsState
    ) {}

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
                .subscribe(dto => {
                    this.field = dto;

                    this.addFieldForm.submitCompleted({ newValue: { ...DEFAULT_FIELD } });

                    if (addNew) {
                        if (Types.isFunction(this.nameInput.nativeElement.focus)) {
                            this.nameInput.nativeElement.focus();
                        }
                    } else if (edit) {
                        this.editing = true;
                    } else {
                        this.emitComplete();
                    }
                }, error => {
                    this.addFieldForm.submitFailed(error);
                });
        }
    }

    public save(addNew = false) {
        const value = this.editForm.submit();

        if (value) {
            const properties = createProperties(this.field.properties['fieldType'], value);

            this.schemasState.updateField(this.schema, this.field as RootFieldDto, { properties })
                .subscribe(() => {
                    this.editForm.submitCompleted();

                    if (addNew) {
                        this.editing = false;
                    } else {
                        this.emitComplete();
                    }
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}
