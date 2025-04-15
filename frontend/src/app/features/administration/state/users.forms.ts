/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, IUpdateUserDto, IUserDto, ValidatorsEx } from '@app/shared';

export class UserForm extends Form<ExtendedFormGroup, IUpdateUserDto, IUserDto> {
    constructor() {
        super(new ExtendedFormGroup({
            email: new UntypedFormControl('', [
                Validators.email,
                Validators.required,
                Validators.maxLength(100),
            ]),
            displayName: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(100),
            ]),
            password: new UntypedFormControl('',
                Validators.required,
            ),
            passwordConfirm: new UntypedFormControl('',
                ValidatorsEx.match('password', 'i18n:users.passwordConfirmValidationMessage'),
            ),
            permissions: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        }));
    }

    public load(value: Partial<IUpdateUserDto>) {
        if (value) {
            this.form.controls['password'].setValidators(Validators.nullValidator);
        } else {
            this.form.controls['password'].setValidators(Validators.required);
        }

        super.load(value);
    }

    protected transformLoad(user: Partial<IUserDto>) {
        const permissions = user.permissions?.join('\n') || '';

        return { ...user, permissions };
    }

    protected transformSubmit(value: any) {
        const permissions = value['permissions'].split('\n').defined();

        return { ...value, permissions };
    }
}
