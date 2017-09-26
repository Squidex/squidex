/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    ComponentBase,
    DialogService,
    SchemaDetailsDto,
    SchemasService,
    UpdateSchemaScriptsDto
} from 'shared';

@Component({
    selector: 'sqx-schema-scripts-form',
    styleUrls: ['./schema-scripts-form.component.scss'],
    templateUrl: './schema-scripts-form.component.html'
})
export class SchemaScriptsFormComponent extends ComponentBase implements OnInit {
    @Output()
    public saved = new EventEmitter<UpdateSchemaScriptsDto>();

    @Output()
    public cancelled = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public appName: string;

    public selectedField = 'scriptQuery';

    public scripts = [
        'Query',
        'Create',
        'Update',
        'Delete',
        'Change'
    ];

    public editFormSubmitted = false;
    public editForm =
        this.formBuilder.group({
            scriptQuery: '',
            scriptCreate: '',
            scriptUpdate: '',
            scriptDelete: '',
            scriptChange: ''
        });

    constructor(dialogs: DialogService,
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs);
    }

    public ngOnInit() {
        this.editForm.patchValue(this.schema);
    }

    public cancel() {
        this.emitCancelled();
        this.resetEditForm();
    }

    public saveSchema() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            this.editForm.disable();

            const requestDto =
                new UpdateSchemaScriptsDto(
                    this.editForm.controls['scriptQuery'].value,
                    this.editForm.controls['scriptCreate'].value,
                    this.editForm.controls['scriptUpdate'].value,
                    this.editForm.controls['scriptDelete'].value,
                    this.editForm.controls['scriptChange'].value);

            this.schemas.putSchemaScripts(this.appName, this.schema.name, requestDto, this.schema.version)
                .subscribe(dto => {
                    this.emitSaved(requestDto);
                    this.resetEditForm();
                }, error => {
                    this.notifyError(error);
                    this.enableEditForm();
                });
        }
    }

    private emitCancelled() {
        this.cancelled.emit();
    }

    private emitSaved(requestDto: UpdateSchemaScriptsDto) {
        this.saved.emit(requestDto);
    }

    public selectField(field: string) {
        this.selectedField = field;
    }

    private enableEditForm() {
        this.editForm.enable();
        this.editFormSubmitted = false;
    }

    private resetEditForm() {
        this.editForm.reset();
        this.editForm.enable();
        this.editFormSubmitted = false;
    }
}