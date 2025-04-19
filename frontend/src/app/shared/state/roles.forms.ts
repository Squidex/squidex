/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, hasNoValue$, TemplatedFormArray } from '@app/framework';
import { AddRoleDto, RoleDto, UpdateRoleDto } from '../model';

export class EditRoleForm extends Form<TemplatedFormArray, UpdateRoleDto, RoleDto> {
    public get controls() {
        return this.form.controls as UntypedFormControl[];
    }

    constructor() {
        super(new TemplatedFormArray(PermissionTemplate.INSTANCE));
    }

    public transformLoad(value: Partial<RoleDto>) {
        return value.permissions || [];
    }

    public transformSubmit(value: any) {
        return new UpdateRoleDto({ permissions: value, properties: {} });
    }
}

class PermissionTemplate {
    public static readonly INSTANCE = new PermissionTemplate();

    public createControl(_: any, initialValue: string) {
        return new UntypedFormControl(initialValue, Validators.required);
    }
}

export class AddRoleForm extends Form<ExtendedFormGroup, AddRoleDto> {
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

    public transformSubmit(value: any) {
        return new AddRoleDto(value);
    }
}
