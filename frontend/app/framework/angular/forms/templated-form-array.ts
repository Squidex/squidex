/*
* Squidex Headless CMS
*
* @license
* Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
*/

import { AbstractControl, AbstractControlOptions, AsyncValidatorFn, ValidatorFn } from '@angular/forms';
import { Types } from '@app/framework/internal';
import { UndefinableFormArray } from './undefinable-form-array';

export interface FormArrayTemplate {
    createControl(value?: any): AbstractControl;

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

    public add(value?: any) {
        const control = this.template.createControl(value);

        this.push(control);

        return control;
    }

    private prepare(value?: any[]) {
        if (Types.isArray(value)) {
            if (value.length === 0) {
                if (this.controls.length > 0) {
                    this.clear({ emitEvent: false });

                    if (this.template.clearControls) {
                        this.template.clearControls();
                    }
                }
            } else {
                while (this.controls.length < value.length) {
                    this.add();
                }

                while (this.controls.length > value.length) {
                    const index = this.controls.length - 1;

                    if (this.template.removeControl) {
                        this.template.removeControl(index, this.controls[index]);
                    }

                    this.removeAt(index, { emitEvent: false });
                }
            }
        } else if (this.template.clearControls) {
            this.clear();

            this.template.clearControls();
        }
    }
}
