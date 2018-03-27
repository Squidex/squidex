/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';

import {
    AddFieldDto,
    AppContext,
    createProperties,
    fadeAnimation,
    FieldDto,
    fieldTypes,
    SchemaDetailsDto,
    UpdateFieldDto,
    ValidatorsEx
} from 'shared';

import { SchemasState } from './../../state/schemas.state';

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class FieldWizardComponent {
    public fieldTypes = fieldTypes;

    public editFormSubmitted = false;
    public editStep = 0;
    public editForm: FormGroup | null;
    public editField: FieldDto | null;

    public addFieldError = '';
    public addFieldFormSubmitted = false;
    public addFieldForm =
        this.formBuilder.group({
            type: ['String',
                [
                    Validators.required
                ]],
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'Name must be a valid javascript name in camel case.')
                ]],
            isLocalizable: [false]
        });

    @Input()
    public schema: SchemaDetailsDto;

    @Output()
    public completed = new EventEmitter();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public complete() {
        this.completed.emit();
    }

    public next() {
        this.editStep++;
    }

    public addField(next: boolean, configure: boolean) {
        this.addFieldFormSubmitted = true;

        if (this.addFieldForm.valid) {
            this.addFieldForm.disable();

            const properties = createProperties(this.addFieldForm.controls['type'].value);

            const partitioning =
                this.addFieldForm.controls['isLocalizable'].value ?
                'language' :
                'invariant';

            const requestDto = new AddFieldDto(this.addFieldForm.controls['name'].value, partitioning, properties);

            this.schemasState.addField(this.schema, requestDto)
                .subscribe(dto => {
                    this.resetFieldForm();

                    if (configure) {
                        this.editField = dto;
                        this.editStep = 1;
                        this.editForm = new FormGroup({});
                    } else if (!next) {
                        this.complete();
                    }
                }, error => {
                    this.resetFieldForm(error.displayMessage);
                });
        }
    }

    public configureField(next: boolean) {
        this.editFormSubmitted = true;

        if (this.editForm!.valid) {
            const properties = createProperties(this.editField!.properties['fieldType'], this.editForm!.value);

            this.schemasState.updateField(this.schema, this.editField!, new UpdateFieldDto(properties))
                .subscribe(() => {
                    this.resetEditForm();

                    if (this.next) {
                        this.editField = null;
                        this.editForm = null;
                        this.editStep = 1;
                    } else {
                        this.complete();
                    }
                }, error => {
                    this.resetEditForm();
                });
        }
    }

    private resetEditForm() {
        this.editForm!.enable();
        this.editForm!.reset(this.editField!.properties);
        this.editFormSubmitted = false;
    }

    private resetFieldForm(error = '') {
        this.addFieldError = error;
        this.addFieldForm.enable();
        this.addFieldForm.reset({ type: 'String' });
        this.addFieldFormSubmitted = false;
    }
}

