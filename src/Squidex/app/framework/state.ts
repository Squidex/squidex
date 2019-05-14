/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ErrorDto } from './utils/error';

import { Types } from './utils/types';

import { fullValue } from './angular/forms/forms-helper';

export interface FormState {
    submitted: boolean;

    error?: string | null;
}

export class Form<T extends AbstractControl, V> {
    private readonly state = new State<FormState>({ submitted: false });

    public submitted =
        this.state.changes.pipe(map(s => s.submitted));

    public error =
        this.state.changes.pipe(map(s => s.error));

    constructor(
        public readonly form: T
    ) {
    }

    protected disable() {
        this.form.disable();
    }

    protected enable() {
        this.form.enable();
    }

    protected setValue(value?: V) {
        if (value) {
            this.form.reset(this.transformLoad(value));
        } else {
            this.form.reset();
        }
    }

    protected transformLoad(value: V): any {
        return value;
    }

    protected transformSubmit(value: any): V {
        return value;
    }

    public load(value: V | undefined) {
        this.state.next(() => ({ submitted: false, error: null }));

        this.setValue(value);
    }

    public submit(): V | null {
        this.state.next(() => ({ submitted: true }));

        if (this.form.valid) {
            const value = this.transformSubmit(fullValue(this.form));

            this.disable();

            return value;
        } else {
            return null;
        }
    }

    public submitCompleted(options?: { newValue?: V, noReset?: boolean }) {
        this.state.next(() => ({ submitted: false, error: null }));

        this.enable();

        if (options && options.noReset) {
            this.form.markAsPristine();
        } else {
            this.setValue(options ? options.newValue : undefined);
        }
    }

    public submitFailed(error?: string | ErrorDto) {
        this.state.next(() => ({ submitted: false, error: this.getError(error) }));

        this.enable();
    }

    private getError(error?: string | ErrorDto) {
        if (Types.is(error, ErrorDto)) {
            return error.displayMessage;
        } else {
            return error;
        }
    }
}

export function createModel<T>(c: { new(): T; }, values: Partial<T>): T {
    return Object.assign(new c(), values);
}

export class Model<T> {
    public with(value: Partial<T>, validOnly = false): T {
        return this.clone(value, validOnly);
    }

    protected clone<V>(update: ((v: any) => V) | Partial<V>, validOnly = false): V {
        let values: Partial<V>;
        if (Types.isFunction(update)) {
            values = update(<any>this);
        } else {
            values = update;
        }

        const clone = Object.assign(Object.create(Object.getPrototypeOf(this)), this);

        for (let key in values) {
            if (values.hasOwnProperty(key)) {
                let value = values[key];

                if (value || !validOnly) {
                    clone[key] = value;
                }
            }
        }

        if (Types.isFunction(clone.onCloned)) {
            clone.onCloned();
        }

        return clone;
    }
}

export class ResultSet<T> extends Model<ResultSet<T>> {
    constructor(
        public readonly total: number,
        public readonly items: T[]
    ) {
        super();
    }
}

export class State<T extends {}> {
    private readonly state: BehaviorSubject<Readonly<T>>;
    private readonly initialState: Readonly<T>;

    public get changes(): Observable<Readonly<T>> {
        return this.state;
    }

    public get snapshot(): Readonly<T> {
        return this.state.value;
    }

    constructor(state: Readonly<T>) {
        this.initialState = state;

        this.state = new BehaviorSubject(state);
    }

    public resetState(update?: ((v: T) => Readonly<T>) | object) {
        this.state.next(this.initialState);

        if (update) {
            this.next(update);
        }
    }

    public next(update: ((v: T) => Readonly<T>) | object) {
        if (Types.isFunction(update)) {
            this.state.next(update(this.state.value));
        } else {
            this.state.next(Object.assign({}, this.snapshot, update));
        }
    }
}