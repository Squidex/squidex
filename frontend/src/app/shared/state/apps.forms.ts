/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, TemplatedFormArray, ValidatorsEx } from '@app/framework';
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
            description: new FormControl('',
                Validators.nullValidator,
            ),
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
            patterns: new TemplatedFormArray(
                PatternTemplate.INSTANCE,
            ),
            hideScheduler: new FormControl(false,
                Validators.nullValidator,
            ),
            hideDateTimeButtons: new FormControl(false,
                Validators.nullValidator,
            ),
            editors: new TemplatedFormArray(
                EditorTemplate.INSTANCE,
            ),
        }));
    }
}

class PatternTemplate {
    public static readonly INSTANCE = new PatternTemplate();

    public createControl() {
        return new ExtendedFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
            regex: new FormControl('',
                Validators.required,
            ),
            message: new FormControl('',
                Validators.nullValidator,
            ),
        });
    }
}

class EditorTemplate {
    public static readonly INSTANCE = new EditorTemplate();

    public createControl() {
        return new ExtendedFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
            url: new FormControl('',
                Validators.required,
            ),
        });
    }
}
