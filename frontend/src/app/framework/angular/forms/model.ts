/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, ValidatorFn } from '@angular/forms';
import { ErrorDto, Types } from '@app/framework/internal';
import { State } from '../../state';
import { ErrorValidator } from './error-validator';
import { addValidator, hasNonCustomError, updateAll } from './forms-helper';

export interface FormState {
    // The number of submits.
    submitCount: number;

    // True, when the submitting is in progress.
    submitting: boolean;

    // The current remote error.
    error?: ErrorDto | null;
}

export class Form<T extends AbstractControl, TOut, TIn = TOut> {
    private readonly state = new State<FormState>({ submitCount: 0, submitting: false });
    private readonly errorValidator = new ErrorValidator();

    public submitCount =
        this.state.project(x => x.submitCount);

    public submitted =
        this.state.project(x => x.submitCount > 0);

    public submitting =
        this.state.project(x => x.submitting);

    public error =
        this.state.project(x => x.error);

    public get remoteValidator(): ValidatorFn {
        return this.errorValidator.validator;
    }

    constructor(
        public readonly form: T,
    ) {
        addValidator(form, this.errorValidator.validator);
    }

    public setEnabled(isEnabled: boolean) {
        if (isEnabled) {
            this.enable();
        } else {
            this.disable();
        }
    }

    protected enable() {
        this.form.enable();
    }

    protected disable() {
        this.form.disable();
    }

    protected setValue(value?: Partial<TIn>) {
        if (value) {
            this.form.reset(this.transformLoad(value));
        } else {
            this.form.reset();
        }
    }

    protected transformLoad(value: Partial<TIn>): any {
        return value;
    }

    protected transformSubmit(value: any): TOut {
        return value;
    }

    public getValue() {
        return this.transformSubmit(this.form.value);
    }

    public load(value: Partial<TIn> | undefined) {
        this.state.resetState();

        this.setValue(value);
    }

    public submit(): TOut | null {
        this.form.markAllAsTouched();

        if (!hasNonCustomError(this.form)) {
            const value = this.transformSubmit(this.form.value);

            if (value) {
                this.updateSubmitState(null, true);
                this.disable();
            }

            return value;
        } else {
            return null;
        }
    }

    public submitCompleted(options?: { newValue?: TIn; noReset?: boolean }) {
        this.updateSubmitState(null, false);

        this.enable();

        if (options && options.noReset) {
            this.form.markAsPristine();
        } else {
            this.setValue(options?.newValue);
        }
    }

    public submitFailed(errorOrMessage?: string | ErrorDto, replaceDetails = true) {
        this.updateSubmitState(errorOrMessage, false, replaceDetails);

        this.enable();
    }

    private updateSubmitState(errorOrMessage: string | ErrorDto | undefined | null, submitting: boolean, replaceDetails = true) {
        const error = getError(errorOrMessage);

        this.state.next(s => ({
            submitCount: s.submitCount + (submitting ? 1 : 0),
            submitting,
            error,
        }));

        if (replaceDetails) {
            this.errorValidator.setError(error);

            updateAll(this.form);
        }
    }
}

function getError(error?: string | ErrorDto | null): ErrorDto | undefined | null {
    if (Types.isString(error)) {
        return new ErrorDto(500, error);
    }

    return error;
}
