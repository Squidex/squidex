/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormControl, Validators } from '@angular/forms';
import { Form, TemplatedFormArray, ExtendedFormGroup, ValidatorsEx } from '@app/framework';
import { AppDto, AppSettingsDto, CreateAppDto, UpdateAppDto, UpdateAppSettingsDto } from './../services/apps.service';

export class CreateAppForm extends Form<ExtendedFormGroup, CreateAppDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new FormControl('', [
                Validators.required,
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage'),
            ]),
        }));
    }
}

export class UpdateAppForm extends Form<ExtendedFormGroup, UpdateAppDto, AppDto> {
    constructor() {
        super(new ExtendedFormGroup({
            label: new FormControl('',
                Validators.maxLength(40),
            ),
            description: new FormControl(''),
        }));
    }
}

export class EditAppSettingsForm extends Form<ExtendedFormGroup, UpdateAppSettingsDto, AppSettingsDto> {
    public get patterns() {
        return this.form.controls['patterns'] as TemplatedFormArray;
    }

    public get patternsControls(): ReadonlyArray<ExtendedFormGroup> {
        return this.patterns.controls as any;
    }

    public get editors() {
        return this.form.controls['editors'] as TemplatedFormArray;
    }

    public get editorsControls(): ReadonlyArray<ExtendedFormGroup> {
        return this.editors.controls as any;
    }

    constructor() {
        super(new ExtendedFormGroup({
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
