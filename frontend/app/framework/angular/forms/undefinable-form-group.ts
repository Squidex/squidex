/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { EventEmitter } from '@angular/core';
import { AbstractControl, AbstractControlOptions, AsyncValidatorFn, FormGroup, ValidatorFn } from '@angular/forms';
import { Types } from '@app/framework/internal';

export class UndefinableFormGroup extends FormGroup {
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

        this.setValue(undefined);
    }

    public getRawValue() {
        if (this.isUndefined) {
            return undefined as any;
        } else {
            return super.getRawValue();
        }
    }

    public setValue(value?: {}, options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.isUndefined = Types.isUndefined(value);

        if (this.isUndefined) {
            super.reset([], options);
        } else {
            super.setValue(value!, options);
        }
    }

    public patchValue(value?: {}, options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.isUndefined = Types.isUndefined(value);

        if (this.isUndefined) {
            super.reset([], options);
        } else {
            super.patchValue(value!, options);
        }
    }

    public reset(value?: {}, options: { onlySelf?: boolean; emitEvent?: boolean } = {}) {
        this.isUndefined = Types.isUndefined(value);

        super.reset(value || {}, options);
    }

    public updateValueAndValidity(opts: { onlySelf?: boolean; emitEvent?: boolean } = {}) {
        super.updateValueAndValidity({ emitEvent: false, onlySelf: true });

        if (this.isUndefined) {
            this.unsetValue();
        }

        if (opts.emitEvent !== false) {
            (this.valueChanges as EventEmitter<any>).emit(this.value);
            (this.statusChanges as EventEmitter<string>).emit(this.status);
        }

        if (this.parent && !opts.onlySelf) {
            this.parent.updateValueAndValidity(opts);
        }
    }

    private unsetValue() {
        (this as { value: any }).value = undefined;
    }
}
