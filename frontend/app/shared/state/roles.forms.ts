/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, Validators } from '@angular/forms';
import { Form, hasNoValue$, hasValue$, TemplatedFormArray, UndefinableFormGroup } from '@app/framework';
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

export class AddPermissionForm extends Form<UndefinableFormGroup, AddPermissionFormType> {
    public get permission() {
        return this.form.controls['permission'];
    }

    public hasPermission = hasValue$(this.permission);

    constructor() {
        super(new UndefinableFormGroup({
            permission: new FormControl('',
                Validators.required,
            ),
        }));
    }
}

export class AddRoleForm extends Form<UndefinableFormGroup, CreateRoleDto> {
    public get name() {
        return this.form.controls['name'];
    }

    public hasNoName = hasNoValue$(this.name);

    constructor() {
        super(new UndefinableFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
        }));
    }
}
