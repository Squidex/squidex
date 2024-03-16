/*
* Squidex Headless CMS
*
* @license
* Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
*/

import { AbstractControl, AbstractControlOptions, AsyncValidatorFn, ValidatorFn } from '@angular/forms';
import { Types } from '@app/framework/internal';
import { UndefinableFormArray } from './extended-form-array';

export interface FormArrayTemplate {
    createControl(value: any, initialValue?: any): AbstractControl;

    removeControl?(index: number, control: AbstractControl) : void;

    clearControls?(): void;
}

export class TemplatedFormArray extends UndefinableFormArray {
    constructor(public readonly template: FormArrayTemplate,
        validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null, asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null,
    ) {
        super([], validatorOrOpts, asyncValidator);
    }

    public setValue(value?: any[], options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.prepare(value);

        super.setValue(value, options);
    }

    public patchValue(value?: any[], options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.prepare(value);

        super.patchValue(value, options);
    }

    public reset(value?: any[], options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.prepare(value);

        super.reset(value, options);
    }

    public add(initialValue?: any) {
        const control = this.template.createControl({}, initialValue);

        this.push(control);

        return control;
    }

    public removeAt(index: number, options?: { emitEvent?: boolean }) {
        if (this.template.removeControl && index >= 0 && index < this.controls.length) {
            this.template.removeControl(index, this.controls[index]);
        }

        super.removeAt(index, options);
    }

    public clear(options?: { emitEvent?: boolean }) {
        if (this.template.clearControls && this.controls.length > 0) {
            this.template.clearControls();
        }

        super.clear(options);
    }

    private prepare(value?: any[]) {
        if (Types.isArray(value) && value.length > 0) {
            let index = this.controls.length;

            while (this.controls.length < value.length) {
                this.add(value[index]);

                index++;
            }

            while (this.controls.length > value.length) {
                this.removeAt(this.controls.length - 1, { emitEvent: false });
            }
        } else {
            this.clear();
        }
    }
}
