/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, ValidatorsEx } from '@app/framework';
import { AppDto, AppSettingsDto, CreateAppDto, UpdateAppDto, UpdateAppSettingsDto } from './../services/apps.service';

export class CreateAppForm extends Form<FormGroup, CreateAppDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage'),
                ],
            ],
        }));
    }
}

export class UpdateAppForm extends Form<FormGroup, UpdateAppDto, AppDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(40),
                ],
            ],
            description: '',
        }));
    }
}

export class EditAppSettingsForm extends Form<FormGroup, UpdateAppSettingsDto, AppSettingsDto> {
    public get patterns(): FormArray {
        return this.form.controls['patterns']! as FormArray;
    }

    public get patternsControls(): ReadonlyArray<FormGroup> {
        return this.patterns.controls as any;
    }

    public get editors(): FormArray {
        return this.form.controls['editors']! as FormArray;
    }

    public get editorsControls(): ReadonlyArray<FormGroup> {
        return this.editors.controls as any;
    }

    constructor(
        private readonly formBuilder: FormBuilder,
    ) {
        super(formBuilder.group({
            patterns: formBuilder.array([]),
            hideScheduler: false,
            hideDateTimeButtons: false,
            editors: formBuilder.array([]),
        }));
    }

    public addPattern() {
        this.patterns.push(
            this.formBuilder.group({
                name: ['',
                    [
                        Validators.required,
                    ],
                ],
                regex: ['',
                    [
                        Validators.required,
                    ],
                ],
                message: '',
            }));
    }

    public addEditor() {
        this.editors.push(
            this.formBuilder.group({
                name: ['',
                    [
                        Validators.required,
                    ],
                ],
                url: ['',
                    [
                        Validators.required,
                    ],
                ],
            }));
    }

    public removePattern(index: number) {
        this.patterns.removeAt(index);
    }

    public removeEditor(index: number) {
        this.editors.removeAt(index);
    }

    public transformLoad(value: AppSettingsDto) {
        const patterns = this.patterns;

        while (patterns.controls.length < value.patterns.length) {
            this.addPattern();
        }

        while (patterns.controls.length > value.patterns.length) {
            this.removePattern(patterns.controls.length - 1);
        }

        const editors = this.editors;

        while (editors.controls.length < value.editors.length) {
            this.addEditor();
        }

        while (editors.controls.length > value.editors.length) {
            this.removeEditor(editors.controls.length - 1);
        }

        return value;
    }
}
