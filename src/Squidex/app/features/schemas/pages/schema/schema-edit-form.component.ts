/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppContext,
    SchemaPropertiesDto,
    SchemasService,
    Version
} from 'shared';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html',
    providers: [
        AppContext
    ]
})
export class SchemaEditFormComponent implements OnInit {
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

    constructor(public readonly ctx: AppContext,
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder
    ) {
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

            this.schemas.putSchema(this.ctx.appName, this.name, requestDto, this.version)
                .subscribe(dto => {
                    this.emitSaved(requestDto);
                    this.resetEditForm();
                }, error => {
                    this.ctx.notifyError(error);
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
        this.editForm.reset();
        this.editForm.enable();
        this.editFormSubmitted = false;
    }
}