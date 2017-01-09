/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    fadeAnimation,
    Notification,
    NotificationService,
    SchemasService
} from 'shared';

import { SchemaPropertiesDto } from './schema-properties';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaEditFormComponent implements OnInit {
    @Output()
    public saved = new EventEmitter<SchemaPropertiesDto>();

    @Output()
    public cancelled = new EventEmitter();

    @Input()
    public schema: SchemaPropertiesDto;

    @Input()
    public appName: string;

    public editForm: FormGroup =
        this.formBuilder.group({
            name: '',
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
        private readonly schemas: SchemasService,
        private readonly formBuilder: FormBuilder,
        private readonly notifications: NotificationService
    ) {
    }

    public ngOnInit() {
        this.editForm.patchValue(this.schema);
    }

    public saveSchema() {
        this.editForm.markAsTouched();

        if (this.editForm.valid) {
            this.editForm.disable();

            const requestDto = this.editForm.value;

            this.schemas.putSchema(this.appName, this.schema.name, requestDto)
                .subscribe(dto => {
                    this.reset();
                    this.saved.emit(new SchemaPropertiesDto(this.schema.name, requestDto.label, requestDto.hints));
                }, error => {
                    this.editForm.enable();
                    this.notifications.notify(Notification.error(error.displayMessage));
                });
        }
    }

    public reset() {
        this.editForm.reset();
    }

    public cancel() {
        this.reset();
        this.cancelled.emit();
    }
}