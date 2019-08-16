import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form, ValidatorsEx } from '@app/shared';

import { UpdateUserDto } from './../services/users.service';

export class UserForm extends Form<FormGroup, UpdateUserDto> {
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
                    Validators.required
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

    public load(value: any) {
        if (value) {
            this.form.controls['password'].setValidators(Validators.nullValidator);
        } else {
            this.form.controls['password'].setValidators(Validators.required);
        }

        super.load(value);
    }

    protected transformLoad(user: UpdateUserDto) {
        return { ...user, permissions: user.permissions.join('\n') };
    }

    protected transformSubmit(value: any): UpdateUserDto {
        return { ...value, permissions: value['permissions'].split('\n').filter((x: any) => !!x) };
    }
}