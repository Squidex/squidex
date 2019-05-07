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

    protected transformLoad(user: UpdateUserDto) {
        return { ...user, permissions: user.permissions.join('\n') };
    }

    protected transformSubmit(value: any): UpdateUserDto {
        return { ...value, permissions: value['permissions'].split('\n').filter((x: any) => !!x) };
    }
}