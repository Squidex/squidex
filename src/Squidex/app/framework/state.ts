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

import { fullValue} from './angular/forms/forms-helper';

export interface FormState {
    submitted: boolean;

    error?: string | null;
}

export class Form<T extends AbstractControl> {
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

    protected reset(value: any) {
        this.form.reset(value);
    }

    protected setValue(value: any) {
        this.form.reset(value, { emitEvent: true });
    }

    public load(value: any) {
        this.state.next(_ => ({ submitted: false, error: null }));

        this.setValue(value);
    }

    public submit(): any | null {
        this.state.next(_ => ({ submitted: true }));

        if (this.form.valid) {
            const value = fullValue(this.form);

            this.disable();

            return value;
        } else {
            return null;
        }
    }

    public submitCompleted(newValue?: any) {
        this.state.next(_ => ({ submitted: false, error: null }));

        this.enable();

        if (newValue) {
            this.reset(newValue);
        } else {
            this.form.markAsPristine();
        }
    }

    public submitFailed(error?: string | ErrorDto) {
        this.state.next(_ => ({ submitted: false, error: this.getError(error) }));

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

    protected clone(update: ((v: any) => T) | Partial<T>, validOnly = false): T {
        let values: Partial<T>;
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

    public resetState() {
        this.next(this.initialState);
    }

    public next(update: ((v: T) => Readonly<T>) | object) {
        if (Types.isFunction(update)) {
            this.state.next(update(this.state.value));
        } else {
            this.state.next(Object.assign({}, this.snapshot, update));
        }
    }
}