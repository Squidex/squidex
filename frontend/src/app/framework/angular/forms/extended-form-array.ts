/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, AbstractControlOptions, AsyncValidatorFn, UntypedFormArray, ValidatorFn } from '@angular/forms';
import { Types } from '@app/framework/internal';

export class ExtendedFormArray extends UntypedFormArray {
    constructor(controls: AbstractControl[], validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null, asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null) {
        super(controls, validatorOrOpts, asyncValidator);

        this['_reduceValue'] = () => {
            return this.controls.map(x => x.value);
        };

        this['_updateValue'] = () => {
            (this as { value: any }).value = this['_reduceValue']();
        };
    }
}

export class UndefinableFormArray extends ExtendedFormArray {
    private isUndefined = false;

    constructor(controls: AbstractControl[], validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null, asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null) {
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

    public push(control: AbstractControl) {
        this.isUndefined = false;

        super.push(control);
    }

    public insert(index: number, control: AbstractControl) {
        this.isUndefined = false;

        super.insert(index, control);
    }

    public setValue(value?: any[], options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.checkUndefined(value);

        if (this.isUndefined) {
            super.reset([], options);
        } else {
            super.setValue(value!, options);
        }
    }

    public patchValue(value?: any[], options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.checkUndefined(value);

        if (this.isUndefined) {
            super.reset([], options);
        } else {
            super.patchValue(value!, options);
        }
    }

    public reset(value?: any[], options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.checkUndefined(value);

        super.reset(value || [], options);
    }

    private checkUndefined(value?: any[]) {
        this.isUndefined = Types.isUndefined(value);

        if (this.isUndefined) {
            this.clear({ emitEvent: false });
        }
    }
}
