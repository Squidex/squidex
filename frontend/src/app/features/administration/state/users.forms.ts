/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { signal, WritableSignal } from '@angular/core';
import { email, maxLength, required } from '@angular/forms/signals';
import { CreateUserDto, SignalForm, SignalValidators, UserDto } from '@app/shared';

export type UserFormModel = {
    email: string;
    displayName: string;
    password: string;
    passwordConfirm: string;
    permissions: string;
};

export class UserForm extends SignalForm<UserFormModel, CreateUserDto, UserDto> {
    private readonly isPasswordRequired: WritableSignal<boolean>;

    constructor() {
        const isPasswordRequired = signal(true);

        super({ email: '', displayName: '', password: '', passwordConfirm: '', permissions: '' }, path => {
            email(path.email);
            SignalValidators.match(path.passwordConfirm, path.password, 'i18n:users.passwordConfirmValidationMessage');
            maxLength(path.email, 100);
            maxLength(path.displayName, 100);
            maxLength(path.password, 100);
            maxLength(path.passwordConfirm, 100);
            required(path.email);
            required(path.displayName);
            required(path.password, { when: () => isPasswordRequired() });
        });

        this.isPasswordRequired = isPasswordRequired;
    }

    public load(value: Partial<UserDto> | undefined) {
        this.isPasswordRequired.set(!value);

        super.load(value);
    }

    protected transformLoad(user: Partial<UserDto>): UserFormModel {
        const permissions = user.permissions?.join('\n') || '';

        return {
            email: user.email ?? '',
            displayName: user.displayName ?? '',
            password: '',
            passwordConfirm: '',
            permissions,
        };
    }

    protected transformSubmit(value: UserFormModel) {
        const permissions = [...value.permissions.split('\n').defined()];

        return new CreateUserDto({ email: value.email, displayName: value.displayName, password: value.password, permissions });
    }
}
