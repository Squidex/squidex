/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddFieldForm,
    fieldTypes,
    SchemaDetailsDto,
    SchemasState
} from '@app/shared';

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html'
})
export class FieldWizardComponent {
    @ViewChild('nameInput')
    public nameInput: ElementRef;

    @Input()
    public schema: SchemaDetailsDto;

    @Output()
    public completed = new EventEmitter();

    public fieldTypes = fieldTypes;

    public addFieldForm = new AddFieldForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public complete() {
        this.completed.emit();
    }

    public addField(next: boolean) {
        const value = this.addFieldForm.submit();

        if (value) {
            this.schemasState.addField(this.schema, value)
                .subscribe(dto => {
                    this.addFieldForm.submitCompleted({ type: fieldTypes[0].type });

                    if (next) {
                        this.nameInput.nativeElement.focus();
                    } else {
                        this.complete();
                    }
                }, error => {
                    this.addFieldForm.submitFailed(error);
                });
        }
    }
}

