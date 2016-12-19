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
    FieldDto
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
    private oldValue: any;

    @Input()
    public field: FieldDto;

    @Output()
    public saved = new EventEmitter<FieldDto>();

    public isEditing: boolean = true;
    public selectedTab = 2;

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
            isRequired: [false]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.resetForm(this.field.properties);
    }

    public save() {
        this.editForm.markAsTouched();

        if (this.editForm.valid) {
            const properties = createProperties(this.field.properties['fieldType'], this.editForm.value);

            const field =
                new FieldDto(
                    this.field.name,
                    this.field.isHidden,
                    this.field.isHidden,
                    properties);

            this.saved.emit(field);
        }
    }

    public cancel() {
        this.resetForm(this.oldValue);
    }

    public toggleEditing() {
        this.isEditing = true;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    private resetForm(properties: any) {
        this.editForm.reset();

        for (let property in properties) {
            if (properties.hasOwnProperty(property)) {
                const controlName = property + '';

                if (this.editForm.contains(controlName)) {
                    this.editForm.get(controlName).setValue(properties[property]);
                }
            }
        }

        this.oldValue = Object.assign({}, this.editForm.value);
    }
}

