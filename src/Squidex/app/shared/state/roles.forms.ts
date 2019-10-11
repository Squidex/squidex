/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';

import {
    Form,
    hasNoValue$,
    hasValue$
} from '@app/framework';

export class EditPermissionsForm extends Form<FormArray, ReadonlyArray<string>> {
    public get controls() {
        return this.form.controls as FormControl[];
    }

    constructor() {
        super(new FormArray([]));
    }

    public add(value?: string) {
        this.form.push(new FormControl(value, Validators.required));
    }

    public remove(index: number) {
        this.form.removeAt(index);
    }

    public load(permissions: ReadonlyArray<string>) {
        while (this.form.controls.length < permissions.length) {
            this.add();
        }

        while (permissions.length > this.form.controls.length) {
            this.form.removeAt(this.form.controls.length - 1);
        }

        super.load(permissions);
    }
}

export class AddPermissionForm extends Form<FormGroup, { permission: string }> {
    public hasPermission = hasValue$(this.form.controls['permission']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            permission: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }
}

export class AddRoleForm extends Form<FormGroup, { name: string }> {
    public hasNoName = hasNoValue$(this.form.controls['name']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }
}