/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ErrorDto, Types } from '@app/framework/internal';
import { fullValue} from './angular/forms/forms-helper';

export interface FormState {
    submitted: boolean;

    error?: string;
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
        this.state.next({ submitted: false, error: null });

        this.setValue(value);
    }

    public submit(): any | null {
        this.state.next({ submitted: true });

        if (this.form.valid) {
            const value = fullValue(this.form);

            this.disable();

            return value;
        } else {
            return null;
        }
    }

    public submitCompleted(newValue?: any) {
        this.state.next({ submitted: false, error: null });

        this.enable();

        if (newValue) {
            this.reset(newValue);
        } else {
            this.form.markAsPristine();
        }
    }

    public submitFailed(error?: string | ErrorDto) {
        this.state.next({ submitted: false, error: this.getError(error) });

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

export class Model {
    protected clone(update: ((v: any) => object) | object): any {
        let values: object;
        if (Types.isFunction(update)) {
            values = update(<any>this);
        } else {
            values = update;
        }

        const clone = Object.assign(Object.create(Object.getPrototypeOf(this)), this, values);

        if (Types.isFunction(clone.onCloned)) {
            clone.onCloned();
        }

        return clone;
    }
}

export class State<T extends {}> {
    private readonly state: BehaviorSubject<T>;
    private readonly initialState: T;

    public get changes(): Observable<T> {
        return this.state;
    }

    public get snapshot() {
        return this.state.value;
    }

    constructor(state: T) {
        this.initialState = state;

        this.state = new BehaviorSubject(state);
    }

    public resetState() {
        this.next(this.initialState);
    }

    public next(update: ((v: T) => T) | object) {
        if (Types.isFunction(update)) {
            this.state.next(update(this.state.value));
        } else {
            this.state.next(Object.assign({}, this.snapshot, update));
        }
    }
}