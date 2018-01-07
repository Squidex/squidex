/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    ApiUrlConfig,
    AppContext,
    DateTime,
    fadeAnimation,
    SchemaDetailsDto,
    SchemasService,
    ValidatorsEx
} from 'shared';

const FALLBACK_NAME = 'my-schema';

@Component({
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class SchemaFormComponent implements OnInit {
    @Output()
    public created = new EventEmitter<SchemaDetailsDto>();

    @Output()
    public cancelled = new EventEmitter();

    @Input()
    public import: any;

    public showImport = false;

    public createFormError = '';
    public createFormSubmitted = false;
    public createForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes only (not at the end).')
                ]],
            import: [{}]
        });

    public schemaName =
        this.createForm.controls['name'].valueChanges.map(n => n || FALLBACK_NAME)
            .startWith(FALLBACK_NAME);

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly ctx: AppContext,
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.createForm.controls['import'].setValue(this.import || {});

        this.showImport = !!this.import;
    }

    public toggleImport() {
        this.showImport = !this.showImport;

        return false;
    }

    public cancel() {
        this.emitCancelled();
        this.resetCreateForm();
    }

    public createSchema() {
        this.createFormSubmitted = true;

        if (this.createForm.valid) {
            this.createForm.disable();

            const schemaName = this.createForm.controls['name'].value;
            const schemaDto = Object.assign(this.createForm.controls['import'].value || {}, { name: schemaName });

            this.schemas.postSchema(this.ctx.appName, schemaDto, this.ctx.userToken, DateTime.now())
                .subscribe(dto => {
                    this.emitCreated(dto);
                    this.resetCreateForm();
                }, error => {
                    this.enableCreateForm(error.displayMessage);
                });
        }
    }

    private emitCancelled() {
        this.cancelled.emit();
    }

    private emitCreated(schema: SchemaDetailsDto) {
        this.created.emit(schema);
    }

    private enableCreateForm(message: string) {
        this.createForm.enable();
        this.createFormError = message;
    }

    private resetCreateForm() {
        this.createFormError = '';
        this.createForm.reset();
        this.createFormSubmitted = false;
    }
}