/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    ApiUrlConfig,
    AuthService,
    fadeAnimation,
    SchemaDto,
    SchemasService,
    ValidatorsEx,
    Version
} from 'shared';

const FALLBACK_NAME = 'my-schema';

@Component({
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaFormComponent {
    @Output()
    public created = new EventEmitter<SchemaDto>();

    @Output()
    public cancelled = new EventEmitter();

    @Input()
    public appName: string;

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
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder,
        private readonly authService: AuthService
    ) {
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

            const schemaVersion = new Version();
            const schemaName = this.createForm.controls['name'].value;

            const requestDto = Object.assign(this.createForm.controls['import'].value || {}, { name: schemaName });

            const me = this.authService.user!.token;

            this.schemas.postSchema(this.appName, requestDto, me, undefined, schemaVersion)
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

    private emitCreated(schema: SchemaDto) {
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