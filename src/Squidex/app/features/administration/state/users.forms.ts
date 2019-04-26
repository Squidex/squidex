import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form, ValidatorsEx } from '@app/shared';

import { UserDto } from './../services/users.service';

export class UserForm extends Form<FormGroup> {
    constructor(
        formBuilder: FormBuilder
    ) {
        super(formBuilder.group({
            email: ['',
                [
                    Validators.email,
                    Validators.required,
                    Validators.maxLength(100)
                ]
            ],
            displayName: ['',
                [
                    Validators.required,
                    Validators.maxLength(100)
                ]
            ],
            password: ['',
                [
                    Validators.nullValidator
                ]
            ],
            passwordConfirm: ['',
                [
                    ValidatorsEx.match('password', 'Passwords must be the same.')
                ]
            ],
            permissions: ['']
        }));
    }

    public load(user?: UserDto) {
        if (user) {
            this.form.controls['password'].setValidators(null);

            super.load({ ...user, permissions: user.permissions.join('\n') });
        } else {
            this.form.controls['password'].setValidators(Validators.required);

            super.load(undefined);
        }
    }

    public submit() {
        const result = super.submit();

        if (result) {
            result['permissions'] = result['permissions'].split('\n').filter((x: any) => !!x);
        }

        return result;
    }
}