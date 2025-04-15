/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, hasNoValue$, TemplatedFormArray } from '@app/framework';
import { IAddRoleDto, IUpdateRoleDto, RoleDto } from '../model';

export class EditRoleForm extends Form<TemplatedFormArray, IUpdateRoleDto, RoleDto> {
    public get controls() {
        return this.form.controls as UntypedFormControl[];
    }

    constructor() {
        super(new TemplatedFormArray(PermissionTemplate.INSTANCE));
    }

    public transformSubmit(value: any) {
        return { permissions: value, properties: {} };
    }

    public transformLoad(value: Partial<IUpdateRoleDto>) {
        return value.permissions || [];
    }
}

class PermissionTemplate {
    public static readonly INSTANCE = new PermissionTemplate();

    public createControl(_: any, initialValue: string) {
        return new UntypedFormControl(initialValue, Validators.required);
    }
}

export class AddRoleForm extends Form<ExtendedFormGroup, IAddRoleDto> {
    public get name() {
        return this.form.controls['name'];
    }

    public hasNoName = hasNoValue$(this.name);

    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('',
                Validators.required,
            ),
        }));
    }
}
