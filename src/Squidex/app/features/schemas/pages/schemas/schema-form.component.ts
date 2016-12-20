/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

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
export class SchemaFormComponent implements OnInit {
    @Input()
    public showClose = false;

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

    public schemaName = FALLBACK_NAME;

    constructor(
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder,
        private readonly authService: AuthService
    ) {
    }

    public ngOnInit() {
        this.createForm.get('name').valueChanges.subscribe(value => {
            this.schemaName = value || FALLBACK_NAME;
        });
    }

    public createSchema() {
        this.createForm.markAsTouched();

        if (this.createForm.valid && this.authService.user) {
            this.createForm.disable();

            const name = this.createForm.get('name').value;
            const dto = new CreateSchemaDto(name);
            const now = DateTime.now();

            const me = `subject:${this.authService.user.id}`;

            this.schemas.postSchema(this.appName, dto)
                .subscribe(dto => {
                    this.createForm.reset();
                    this.created.emit(new SchemaDto(dto.id, name, now, now, me, me));
                }, error => {
                    this.reset();
                    this.creationError = error.displayMessage;
                });
        }
    }

    private reset() {
        this.createForm.enable();
        this.creationError = '';
    }

    public cancel() {
        this.reset();
        this.cancelled.emit();
    }
}