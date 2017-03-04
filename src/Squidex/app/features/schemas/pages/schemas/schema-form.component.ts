/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    AuthService,
    CreateSchemaDto,
    DateTime,
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
    @Input()
    public appName: string;

    @Output()
    public created = new EventEmitter<SchemaDto>();

    @Output()
    public cancelled = new EventEmitter();

    public creationError = '';
    public createFormSubmitted = false;
    public createForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes only (not at the end).')
                ]]
        });

    public schemaName =
        Observable.of(FALLBACK_NAME)
            .merge(this.createForm.get('name').valueChanges.map(n => n || FALLBACK_NAME));

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder,
        private readonly authService: AuthService
    ) {
    }

    public cancel() {
        this.reset();
        this.cancelled.emit();
    }

    public createSchema() {
        this.createFormSubmitted = true;

        if (this.createForm.valid) {
            this.createForm.disable();

            const schemaVersion = new Version();
            const schemaName = this.createForm.get('name').value;

            const requestDto = new CreateSchemaDto(schemaName);

            this.schemas.postSchema(this.appName, requestDto, schemaVersion)
                .subscribe(dto => {
                    this.reset();
                    this.created.emit(this.createSchemaDto(dto.id, schemaName, schemaVersion));
                }, error => {
                    this.createForm.enable();
                    this.creationError = error.displayMessage;
                });
        }
    }

    private reset() {
        this.creationError = '';
        this.createForm.reset();
        this.createFormSubmitted = false;
    }

    private createSchemaDto(id: string, name: string, version: Version) {
        const user = this.authService.user!.token;
        const now = DateTime.now();

        return new SchemaDto(id, name, undefined, false, user, user, now, now, version);
    }
}