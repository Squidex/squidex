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
    fieldTypes,
    RootFieldDto,
    SchemaDetailsDto,
    SchemasState,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html'
})
export class FieldWizardComponent implements OnInit {
    @ViewChild('nameInput')
    public nameInput: ElementRef;

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public parent: RootFieldDto;

    @Output()
    public completed = new EventEmitter();

    public fieldTypes = fieldTypes;

    public addFieldForm = new AddFieldForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        if (this.parent) {
            this.fieldTypes = this.fieldTypes.filter(x => x.type !== 'Array');
        }
    }

    public complete() {
        this.completed.emit();
    }

    public addField(next: boolean) {
        const value = this.addFieldForm.submit();

        if (value) {
            this.schemasState.addField(this.schema, value, this.parent)
                .subscribe(dto => {
                    this.addFieldForm.submitCompleted({ type: fieldTypes[0].type });

                    if (next) {
                        if (Types.isFunction(this.nameInput.nativeElement.focus)) {
                            this.nameInput.nativeElement.focus();
                        }
                    } else {
                        this.complete();
                    }
                }, error => {
                    this.addFieldForm.submitFailed(error);
                });
        }
    }
}

