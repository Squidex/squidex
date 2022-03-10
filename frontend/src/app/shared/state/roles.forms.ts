/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, hasNoValue$, hasValue$, TemplatedFormArray } from '@app/framework';
import { CreateRoleDto, RoleDto, UpdateRoleDto } from './../services/roles.service';

export class EditRoleForm extends Form<TemplatedFormArray, UpdateRoleDto, RoleDto> {
    public get controls() {
        return this.form.controls as FormControl[];
    }

    constructor() {
        super(new TemplatedFormArray(PermissionTemplate.INSTANCE));
    }

    public transformSubmit(value: any) {
        return { permissions: value, properties: {} };
    }

    public transformLoad(value: Partial<UpdateRoleDto>) {
        return value.permissions || [];
    }
}

class PermissionTemplate {
    public static readonly INSTANCE = new PermissionTemplate();

    public createControl(_: any, initialValue: string) {
        return new FormControl(initialValue, Validators.required);
    }
}

type AddPermissionFormType = { permission: string };

export class AddPermissionForm extends Form<ExtendedFormGroup, AddPermissionFormType> {
    public get permission() {
        return this.form.controls['permission'];
    }

    public hasPermission = hasValue$(this.permission);

    constructor() {
        super(new ExtendedFormGroup({
            permission: new FormControl('',
                Validators.required,
            ),
        }));
    }
}

export class AddRoleForm extends Form<ExtendedFormGroup, CreateRoleDto> {
    public get name() {
        return this.form.controls['name'];
    }

    public hasNoName = hasNoValue$(this.name);

    constructor() {
        super(new ExtendedFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
        }));
    }
}
