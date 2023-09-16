/*
* Squidex Headless CMS
*
* @license
* Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
*/

import { AbstractControlOptions, AsyncValidatorFn, UntypedFormGroup, ValidatorFn } from '@angular/forms';
import { Types } from '@app/framework/internal';
import { UndefinableFormGroup } from './extended-form-group';

export interface FormGroupTemplate {
    setControls(form: UntypedFormGroup, value: any): void;

    clearControls?(): void;
}

export class TemplatedFormGroup extends UndefinableFormGroup {
    constructor(public readonly template: FormGroupTemplate,
        validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null, asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null,
    ) {
        super({}, validatorOrOpts, asyncValidator);
    }

    public setValue(value?: {}, options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.build(value);

        super.setValue(value, options);
    }

    public patchValue(value?: {}, options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.build(value);

        super.patchValue(value, options);
    }

    public reset(value?: {}, options?: { onlySelf?: boolean; emitEvent?: boolean }) {
        this.build(value);

        super.reset(value, options);
    }

    public build(value?: {}) {
        if (Types.isObject(value)) {
            this.template?.setControls(this, value);
        } else if (this.template?.clearControls) {
            this.template?.clearControls();
        }
    }
}
