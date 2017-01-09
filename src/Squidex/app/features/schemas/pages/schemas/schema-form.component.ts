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
    AuthService,
    CreateSchemaDto,
    DateTime,
    fadeAnimation,
    SchemaDto,
    SchemasService
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
    public createForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*')
                ]]
        });

    public schemaName =
        Observable.of(FALLBACK_NAME)
            .merge(this.createForm.get('name').valueChanges.map(n => n || FALLBACK_NAME));

    constructor(
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder,
        private readonly authService: AuthService
    ) {
    }

    public createSchema() {
        this.createForm.markAsTouched();

        if (this.createForm.valid) {
            this.createForm.disable();

            const name = this.createForm.get('name').value;

            const requestDto = new CreateSchemaDto(name);

            this.schemas.postSchema(this.appName, requestDto)
                .subscribe(dto => {
                    this.reset();
                    this.created.emit(this.createSchemaDto(dto.id, name));
                }, error => {
                    this.createForm.enable();
                    this.creationError = error.displayMessage;
                });
        }
    }

    public reset() {
        this.creationError = '';
        this.createForm.reset();
    }

    public cancel() {
        this.reset();
        this.cancelled.emit();
    }

    private createSchemaDto(id: string, name: string) {
        const user = this.authService.user!.token;
        const now = DateTime.now();

        return new SchemaDto(id, name, undefined, false, user, user, now, now);
    }
}