/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, TemplatedFormArray, ValidatorsEx } from '@app/framework';
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
    public get patterns() {
        return this.form.controls['patterns']! as TemplatedFormArray;
    }

    public get patternsControls(): ReadonlyArray<FormGroup> {
        return this.patterns.controls as any;
    }

    public get editors() {
        return this.form.controls['editors']! as TemplatedFormArray;
    }

    public get editorsControls(): ReadonlyArray<FormGroup> {
        return this.editors.controls as any;
    }

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            patterns: new TemplatedFormArray(new PatternTemplate(formBuilder)),
            hideScheduler: false,
            hideDateTimeButtons: false,
            editors: new TemplatedFormArray(new EditorTemplate(formBuilder)),
        }));
    }

    public addPattern() {
        this.patterns.add();
    }

    public addEditor() {
        this.editors.add();
    }

    public removePattern(index: number) {
        this.patterns.removeAt(index);
    }

    public removeEditor(index: number) {
        this.editors.removeAt(index);
    }
}

class PatternTemplate {
    constructor(private readonly formBuilder: FormBuilder) {}

    public createControl() {
        return this.formBuilder.group({
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
        });
    }
}

class EditorTemplate {
    constructor(private readonly formBuilder: FormBuilder) {}

    public createControl() {
        return this.formBuilder.group({
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
        });
    }
}
