/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, TemplatedFormArray, UndefinableFormGroup, ValidatorsEx } from '@app/framework';
import { AppDto, AppSettingsDto, CreateAppDto, UpdateAppDto, UpdateAppSettingsDto } from './../services/apps.service';

export class CreateAppForm extends Form<FormGroup, CreateAppDto> {
    constructor() {
        super(new UndefinableFormGroup({
            name: new FormControl('', [
                Validators.required,
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage'),
            ]),
        }));
    }
}

export class UpdateAppForm extends Form<FormGroup, UpdateAppDto, AppDto> {
    constructor() {
        super(new UndefinableFormGroup({
            label: new FormControl('',
                Validators.maxLength(40),
            ),
            description: new FormControl(''),
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

    constructor() {
        super(new UndefinableFormGroup({
            patterns: new TemplatedFormArray(new PatternTemplate()),
            hideScheduler: new FormControl(false),
            hideDateTimeButtons: new FormControl(false),
            editors: new TemplatedFormArray(new EditorTemplate()),
        }));
    }
}

class PatternTemplate {
    public createControl() {
        return new FormControl({
            name: new FormControl('',
                Validators.required,
            ),
            regex: new FormControl('',
                Validators.required,
            ),
            message: new FormControl(''),
        });
    }
}

class EditorTemplate {
    public createControl() {
        return new FormControl({
            name: new FormControl('',
                Validators.required,
            ),
            url: new FormControl('',
                Validators.required,
            ),
        });
    }
}
