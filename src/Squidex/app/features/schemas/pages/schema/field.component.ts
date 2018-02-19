/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppPatternDto,
    createProperties,
    fadeAnimation,
    FieldDto,
    FieldPropertiesDto,
    ModalView,
    SchemaDto
} from 'shared';

@Component({
    selector: 'sqx-field',
    styleUrls: ['./field.component.scss'],
    templateUrl: './field.component.html',
    animations: [
        fadeAnimation
    ]
})
export class FieldComponent implements OnInit {
    @Input()
    public field: FieldDto;

    @Input()
    public schemas: SchemaDto[];

    @Input()
    public regexSuggestions: AppPatternDto[] = [];

    @Output()
    public locking = new EventEmitter();

    @Output()
    public hiding = new EventEmitter();

    @Output()
    public showing = new EventEmitter();

    @Output()
    public saving = new EventEmitter<FieldPropertiesDto>();

    @Output()
    public enabling = new EventEmitter();

    @Output()
    public disabling = new EventEmitter();

    @Output()
    public deleting = new EventEmitter();

    public dropdown = new ModalView(false, true);

    public isEditing = false;
    public selectedTab = 0;

    public get displayName() {
        return this.field.properties.label && this.field.properties.label.length > 0 ? this.field.properties.label : this.field.name;
    }

    public editFormSubmitted = false;
    public editForm =
        this.formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(100)
                ]],
            hints: ['',
                [
                    Validators.maxLength(100)
                ]],
            isRequired: [false],
            isListField: [false]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.resetEditForm();
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public cancel() {
        this.resetEditForm();
    }

    public save() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            const properties = createProperties(this.field.properties['fieldType'], this.editForm.value);

            this.emitSaving(properties);
        }
    }

    private emitSaving(properties: FieldPropertiesDto) {
        this.saving.emit(properties);
    }

    private resetEditForm() {
        this.editFormSubmitted = false;
        this.editForm.reset(this.field.properties);

        if (this.field.isLocked) {
            this.editForm.disable();
        }

        this.isEditing = false;
    }
}

