/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Injector, signal, WritableSignal } from '@angular/core';
import { disabled, FieldTree, form, SchemaPathTree, submit, TreeValidationResult } from '@angular/forms/signals';
import { isObservable, lastValueFrom, Observable } from 'rxjs';
import { ErrorDto, Types } from '@app/framework/internal';
import { nullSafeSignal } from './null-safe-signal';
import { serverErrors } from './signal-form-server-errors';

export interface SubmitOptions {
    resetAfterSubmit?: boolean;
}

export type SubmitAction<TValue, TResult> = (value: TValue) => Observable<TResult> | Promise<TResult>;

export abstract class SignalForm<TModel, TOut, TIn = TOut> {
    private readonly isDisabled = signal(false);
    private readonly submitCounter = signal(0);
    private readonly submitError = signal<ErrorDto | null>(null);

    public readonly model: WritableSignal<TModel>;

    public readonly form: FieldTree<TModel>;

    public readonly submitCount = this.submitCounter.asReadonly();

    public readonly submitted = computed(() => {
        return this.submitCounter() > 0;
    });

    public readonly submitting = computed(() => {
        return this.root().submitting();
    });

    public readonly error = this.submitError.asReadonly();

    constructor(
        private readonly initialModel: TModel,
        schema?: (path: SchemaPathTree<TModel>) => void,
        injector?: Injector,
    ) {
        this.model = nullSafeSignal(initialModel);
        this.form = form(this.model, path => {
            disabled(path as SchemaPathTree<unknown>, () => this.isDisabled() || this.submitting());

            schema?.(path as SchemaPathTree<TModel>);
        }, { injector }) as FieldTree<TModel>;
    }

    public setEnabled(isEnabled: boolean) {
        this.isDisabled.set(!isEnabled);
    }

    protected transformLoad(value: Partial<TIn>, base: TModel): TModel {
        const result: Record<string, any> = { ...base as Record<string, any> };

        for (const key of Object.keys(result)) {
            const loaded = (value as Record<string, any>)[key];

            if (loaded !== undefined) {
                result[key] = loaded;
            }
        }

        return result as TModel;
    }

    protected transformSubmit(value: TModel): TOut | null {
        return value as unknown as TOut;
    }

    public getValue() {
        return this.transformSubmit(this.model());
    }

    public load(value: Partial<TIn> | undefined) {
        this.submitCounter.set(0);
        this.submitError.set(null);
        this.setValue(value, this.initialModel);
    }

    public setError(error: string | ErrorDto) {
        this.submitError.set(getError(error));
    }

    public reset() {
        this.load(undefined);
    }

    public submit<TResult>(action: SubmitAction<TOut, TResult>, options?: SubmitOptions) {
        return this.run(action, () => {
            if (options?.resetAfterSubmit) {
                this.setValue(undefined, this.initialModel);
            } else {
                this.root().reset();
            }
        });
    }

    public submitAndUpdate(action: SubmitAction<TOut, Partial<TIn> | undefined>, options?: SubmitOptions) {
        return this.run(action, result => {
            this.setValue(result, options?.resetAfterSubmit ? this.initialModel : this.model());
        });
    }

    protected onSubmitted() {

    }

    private async run<TResult>(action: SubmitAction<TOut, TResult>, onSuccess: (result: TResult) => void) {
        let isSuccess = false;

        this.submitCounter.update(x => x + 1);
        this.submitError.set(null);

        await submit(this.form, async () => {
            const value = this.transformSubmit(this.model());
            if (!value) {
                return undefined;
            }

            try {
                const result = await toPromise(action(value));

                onSuccess(result);
                this.onSubmitted();
                isSuccess = true;
                return undefined;
            } catch (ex) {
                return this.handleError(ex);
            }
        });

        return isSuccess;
    }

    private handleError(ex: unknown): TreeValidationResult {
        const error = getError(ex);
        if (!error) {
            throw ex;
        }

        this.submitError.set(error);

        return serverErrors(this.form as FieldTree<unknown>, this.model(), error);
    }

    private setValue(value: Partial<TIn> | undefined, base: TModel) {
        this.model.set(value ? this.transformLoad(value, base) : base);
        this.root().reset();
    }

    private root() {
        return (this.form as FieldTree<unknown>)();
    }
}

function toPromise<T>(result: Observable<T> | Promise<T>): Promise<T> {
    if (isObservable(result)) {
        return lastValueFrom(result, { defaultValue: undefined as T });
    }

    return result;
}

function getError(error: unknown): ErrorDto | null {
    if (Types.is(error, ErrorDto)) {
        return error;
    }

    if (Types.isString(error)) {
        return new ErrorDto(500, error);
    }

    return null;
}
