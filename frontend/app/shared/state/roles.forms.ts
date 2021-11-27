/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, hasNoValue$, hasValue$, TemplatedFormArray } from '@app/framework';
import { CreateRoleDto, RoleDto, UpdateRoleDto } from './../services/roles.service';

export class EditRoleForm extends Form<TemplatedFormArray, UpdateRoleDto, RoleDto> {
    public get controls() {
        return this.form.controls as FormControl[];
    }

    constructor() {
        super(new TemplatedFormArray(new PermissionTemplate()));
    }

    public transformSubmit(value: any) {
        return { permissions: value, properties: {} };
    }

    public transformLoad(value: Partial<UpdateRoleDto>) {
        return value.permissions || [];
    }
}

class PermissionTemplate {
    public createControl(_: any, initialValue: string) {
        return new FormControl(initialValue, Validators.required);
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
