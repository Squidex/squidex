/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, hasNoValue$, hasValue$ } from '@app/framework';
import { CreateRoleDto, RoleDto, UpdateRoleDto } from './../services/roles.service';

export class EditRoleForm extends Form<FormArray, UpdateRoleDto, RoleDto> {
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

    public transformSubmit(value: any) {
        return { permissions: value, properties: {} };
    }

    public transformLoad(value: Partial<UpdateRoleDto>) {
        const permissions = value.permissions || [];

        while (this.form.controls.length < permissions.length) {
            this.add();
        }

        while (permissions.length > this.form.controls.length) {
            this.form.removeAt(this.form.controls.length - 1);
        }

        return value.permissions;
    }
}

type AddPermissionFormType = { permission: string };

export class AddPermissionForm extends Form<FormGroup, AddPermissionFormType> {
    public hasPermission = hasValue$(this.form.controls['permission']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            permission: ['',
                [
                    Validators.required,
                ],
            ],
        }));
    }
}

export class AddRoleForm extends Form<FormGroup, CreateRoleDto> {
    public hasNoName = hasNoValue$(this.form.controls['name']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                ],
            ],
        }));
    }
}
