/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { SchemaDetailsDto, UpdateSchemaDto } from 'shared';

import { SchemasState } from './../../state/schemas.state';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html'
})
export class SchemaEditFormComponent implements OnInit {
    @Output()
    public completed = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public editFormSubmitted = false;
    public editForm =
        this.formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(100)
                ]],
            hints: ['',
                [
                    Validators.maxLength(1000)
                ]]
        });

    constructor(
        private readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.editForm.patchValue(this.schema.properties);
    }

    public complete() {
        this.completed.emit();
    }

    public saveSchema() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            this.editForm.disable();

            const requestDto = <UpdateSchemaDto>this.editForm.value;

            this.schemasState.update(this.schema, requestDto)
                .subscribe(dto => {
                    this.complete();
                }, error => {
                    this.enableEditForm();
                });
        }
    }

    private enableEditForm() {
        this.editForm.enable();
        this.editFormSubmitted = false;
    }
}