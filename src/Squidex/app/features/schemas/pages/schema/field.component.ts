/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    createProperties,
    fadeAnimation,
    FieldDto,
    ModalView
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
    public dropdown = new ModalView(false, true);

    @Input()
    public field: FieldDto;

    @Output()
    public hiding = new EventEmitter<FieldDto>();

    @Output()
    public showing = new EventEmitter<FieldDto>();

    @Output()
    public saving= new EventEmitter<FieldDto>();

    @Output()
    public enabling = new EventEmitter<FieldDto>();

    @Output()
    public disabling = new EventEmitter<FieldDto>();

    @Output()
    public deleting = new EventEmitter<FieldDto>();

    public isEditing: boolean = false;
    public selectedTab = 0;

    public get displayName() {
        return this.field.properties.label && this.field.properties.label.length > 0 ? this.field.properties.label : this.field.name;
    }

    public editFormSubmitted = false;
    public editForm: FormGroup =
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
            isListField: [false],
            isLocalizable: [false]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.resetForm();
    }

    public save() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            const properties = createProperties(this.field.properties['fieldType'], this.editForm.value);

            const field =
                new FieldDto(
                    this.field.fieldId,
                    this.field.name,
                    this.field.isHidden,
                    this.field.isHidden,
                    properties);

            this.saving.emit(field);
        }
    }

    public cancel() {
        this.resetForm();
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    private resetForm() {
        this.editFormSubmitted = false;
        this.editForm.reset(this.field.properties);

        this.isEditing = false;
    }
}

