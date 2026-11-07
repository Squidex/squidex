/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { signal, WritableSignal } from '@angular/core';
import { disabled, FieldTree, form, SchemaPathTree, validateTree } from '@angular/forms/signals';
import { ErrorDto, Types } from '@app/framework/internal';
import { State } from '../../state';
import { FormState } from './model';
import { SERVER_ERROR_KIND, serverErrors } from './signal-form-server-errors';

export abstract class SignalForm<TModel, TOut, TIn = TOut> {
    private readonly state = new State<FormState>({ submitCount: 0, submitting: false });
    private readonly isDisabled = signal(false);
    private readonly serverError = signal<{ error: ErrorDto; model: TModel } | null>(null);

    public readonly model: WritableSignal<TModel>;

    public readonly form: FieldTree<TModel>;

    public readonly submitCount =
        this.state.project(x => x.submitCount);

    public readonly submitted =
        this.state.project(x => x.submitCount > 0);

    public readonly submitting =
        this.state.project(x => x.submitting);

    public readonly error =
        this.state.project(x => x.error);

    constructor(
        private readonly initialModel: TModel,
        schema?: (path: SchemaPathTree<TModel>) => void,
    ) {
        this.model = signal(initialModel);
        this.form = form(this.model, path => {
            disabled(path, () => this.isDisabled());

            validateTree(path, ctx => {
                return serverErrors(ctx.fieldTree, this.model(), this.serverError());
            });

            schema?.(path);
        });
    }

    public setEnabled(isEnabled: boolean) {
        this.isDisabled.set(!isEnabled);
    }

    protected transformLoad(value: Partial<TIn>): TModel {
        const result: Record<string, any> = { ...this.initialModel };

        for (const key of Object.keys(result)) {
            const loaded = (value as Record<string, any>)[key];

            if (loaded !== undefined) {
                result[key] = loaded;
            }
        }

        return result as TModel;
    }

    protected transformSubmit(value: TModel): TOut {
        return value as unknown as TOut;
    }

    public getValue() {
        return this.transformSubmit(this.model());
    }

    public load(value: Partial<TIn> | undefined) {
        this.state.resetState();
        this.setValue(value);
    }

    public submit(): TOut | null {
        markAllAsTouched(this.form, this.model());

        const hasErrors = this.form().errorSummary().some(x => x.kind !== SERVER_ERROR_KIND);
        if (hasErrors) {
            return null;
        }

        const value = this.transformSubmit(this.model());
        if (value) {
            this.updateSubmitState(null, true);
            this.isDisabled.set(true);
        }

        return value;
    }

    public submitCompleted(options?: { newValue?: TIn; noReset?: boolean }) {
        this.updateSubmitState(null, false);
        this.isDisabled.set(false);

        if (options?.noReset) {
            this.form().reset();
        } else {
            this.setValue(options?.newValue);
        }
    }

    public submitFailed(errorOrMessage?: string | ErrorDto, replaceDetails = true) {
        this.updateSubmitState(errorOrMessage, false, replaceDetails);
        this.isDisabled.set(false);
    }

    private setValue(value?: Partial<TIn>) {
        this.model.set(value ? this.transformLoad(value) : this.initialModel);
        this.form().reset();
    }

    private updateSubmitState(errorOrMessage: string | ErrorDto | undefined | null, submitting: boolean, replaceDetails = true) {
        const error = getError(errorOrMessage);

        this.state.next(s => ({
            submitCount: s.submitCount + (submitting ? 1 : 0),
            submitting,
            error,
        }));

        if (replaceDetails) {
            this.serverError.set(error ? { error, model: this.model() } : null);
        }
    }
}

export function markAllAsTouched(field: FieldTree<any> | undefined, value: any) {
    if (!field) {
        return;
    }

    field().markAsTouched();

    if (Types.isArray(value)) {
        value.forEach((item, i) => markAllAsTouched((field as any)[i], item));
    } else if (Types.isObject(value)) {
        for (const key of Object.keys(value)) {
            markAllAsTouched((field as any)[key], value[key]);
        }
    }
}

function getError(error?: string | ErrorDto | null): ErrorDto | undefined | null {
    if (Types.isString(error)) {
        return new ErrorDto(500, error);
    }

    return error;
}
