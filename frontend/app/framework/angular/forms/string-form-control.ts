/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { AbstractControlOptions, AsyncValidatorFn, FormControl, ValidatorFn } from '@angular/forms';

type ValueOptions = {
    onlySelf?: boolean;
    emitEvent?: boolean;
    emitModelToViewChange?: boolean;
    emitViewToModelChange?: boolean;
};

export class StringFormControl extends FormControl {
    constructor(formState?: any, validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null, asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null) {
        super(formState, validatorOrOpts, asyncValidator);
    }

    public setValue(value: any, options?: ValueOptions) {
        if (value === '') {
            value = undefined;
        }

        super.setValue(value, options);
    }

    public patchValue(value: any, options?: ValueOptions) {
        if (value === '') {
            value = undefined;
        }

        super.patchValue(value, options);
    }
}