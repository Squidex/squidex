/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, ValidatorsEx } from '@app/shared';
import { UpdateUserDto, UserDto } from './../services/users.service';

export class UserForm extends Form<FormGroup, UpdateUserDto, UserDto> {
    constructor(
        formBuilder: FormBuilder,
    ) {
        super(formBuilder.group({
            email: ['',
                [
                    Validators.email,
                    Validators.required,
                    Validators.maxLength(100),
                ],
            ],
            displayName: ['',
                [
                    Validators.required,
                    Validators.maxLength(100),
                ],
            ],
            password: ['',
                [
                    Validators.required,
                ],
            ],
            passwordConfirm: ['',
                [
                    ValidatorsEx.match('password', 'i18n:users.passwordConfirmValidationMessage'),
                ],
            ],
            permissions: [''],
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
