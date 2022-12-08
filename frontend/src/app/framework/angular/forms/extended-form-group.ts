/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, AbstractControlOptions, AsyncValidatorFn, UntypedFormGroup, ValidatorFn } from '@angular/forms';
import { Types } from '@app/framework/internal';

export class ExtendedFormGroup extends UntypedFormGroup {
    constructor(controls: { [key: string]: AbstractControl }, validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null, asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null) {
        super(controls, validatorOrOpts, asyncValidator);

        this['_reduceValue'] = () => {
            const result = {};

            for (const [key, control] of Object.entries(this.controls)) {
                result[key] = control.value;
            }

            return result;
        };

        this['_updateValue'] = () => {
            (this as { value: any }).value = this['_reduceValue']();
        };
    }
}

export class UndefinableFormGroup extends ExtendedFormGroup {
    private isUndefined = false;

    constructor(controls: { [key: string]: AbstractControl }, validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null, asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null) {
        super(controls, validatorOrOpts, asyncValidator);

        const reduce = this['_reduceValue'];

        this['_reduceValue'] = () => {
            if (this.isUndefined) {
                return undefined;
            } else {
                return reduce.apply(this);
            }
        };
    }

    public getRawValue() {
        if (this.isUndefined) {
            return undefined as any;
        } else {
            return super.getRawValue();
        }
    }

    public setValue(value?: {}, options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.checkUndefined(value);

        if (this.isUndefined) {
            super.reset({}, options);
        } else {
            super.setValue(value!, options);
        }
    }

    public patchValue(value?: {}, options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.checkUndefined(value);

        if (this.isUndefined) {
            super.reset({}, options);
        } else {
            super.patchValue(value!, options);
        }
    }

    public reset(value?: {}, options: { onlySelf?: boolean; emitEvent?: boolean } = {}) {
        this.checkUndefined(value);

        super.reset(value || {}, options);
    }

    private checkUndefined(value?: {}) {
        this.isUndefined = Types.isUndefined(value);
    }
}
