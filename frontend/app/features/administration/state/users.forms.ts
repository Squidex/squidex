/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, UndefinableFormGroup, ValidatorsEx } from '@app/shared';
import { UpdateUserDto, UserDto } from './../services/users.service';

export class UserForm extends Form<FormGroup, UpdateUserDto, UserDto> {
    constructor() {
        super(new UndefinableFormGroup({
            email: new FormControl('', [
                Validators.email,
                Validators.required,
                Validators.maxLength(100),
            ]),
            displayName: new FormControl('', [
                Validators.required,
                Validators.maxLength(100),
            ]),
            password: new FormControl('',
                Validators.required,
            ),
            passwordConfirm: new FormControl('',
                ValidatorsEx.match('password', 'i18n:users.passwordConfirmValidationMessage'),
            ),
            permissions: new FormControl(''),
        }));
    }

    public load(value: Partial<UserDto>) {
        if (value) {
            this.form.controls['password'].setValidators(Validators.nullValidator);
        } else {
            this.form.controls['password'].setValidators(Validators.required);
        }

        super.load(value);
    }

    protected transformLoad(user: Partial<UserDto>) {
        const permissions = user.permissions?.join('\n') || '';

        return { ...user, permissions };
    }

    protected transformSubmit(value: any) {
        const permissions = value['permissions'].split('\n').filter((x: any) => !!x);

        return { ...value, permissions };
    }
}
