/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    ComponentBase,
    DialogService,
    SchemaPropertiesDto,
    SchemasService,
    Version
} from 'shared';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html'
})
export class SchemaEditFormComponent extends ComponentBase implements OnInit {
    @Output()
    public saved = new EventEmitter<SchemaPropertiesDto>();

    @Output()
    public cancelled = new EventEmitter();

    @Input()
    public name: string;

    @Input()
    public properties: SchemaPropertiesDto;

    @Input()
    public version: Version;

    @Input()
    public appName: string;

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

    constructor(dialogs: DialogService,
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs);
    }

    public ngOnInit() {
        this.editForm.patchValue(this.properties);
    }

    public cancel() {
        this.emitCancelled();
        this.resetEditForm();
    }

    public saveSchema() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            this.editForm.disable();

            const requestDto = this.editForm.value;

            this.schemas.putSchema(this.appName, this.name, requestDto, this.version)
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

    private emitSaved(requestDto: any) {
        this.saved.emit(new SchemaPropertiesDto(requestDto.label, requestDto.hints));
    }

    private enableEditForm() {
        this.editForm.enable();
        this.editFormSubmitted = false;
    }

    private resetEditForm() {
        this.editFormSubmitted = false;
        this.editForm.reset();
        this.editForm.enable();
    }
}