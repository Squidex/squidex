/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';

import { ErrorDto } from './utils/error';

export interface FormState {
    submitted: boolean;

    error?: string;
}

export class Form<T extends AbstractControl> {
    private readonly state = new State<FormState>({ submitted: false });

    public submitted =
        this.state.changes.map(s => s.submitted);

    public error =
        this.state.changes.map(s => s.error);

    constructor(
        public readonly form: T
    ) {
    }

    public load(value: any) {
        this.state.next({ submitted: false, error: null });

        this.form.reset(value, { emitEvent: true });
    }

    public submit(): any | null {
        this.state.next({ submitted: true });

        if (this.form.valid) {
            const value = this.form.value;

            this.form.disable();

            return value;
        } else {
            return null;
        }
    }

    public submitCompleted(newValue?: any) {
        this.state.next({ submitted: false, error: null });

        this.form.enable();

        if (newValue) {
            this.form.reset(newValue);
        } else {
            this.form.markAsPristine();
        }
    }

    public submitFailed(error?: string | ErrorDto) {
        this.state.next({ submitted: false, error: this.getError(error) });

        this.form.enable();
    }

    private getError(error?: string | ErrorDto) {
        if (error instanceof ErrorDto) {
            return error.displayMessage;
        } else {
            return error;
        }
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
        if (update instanceof Function) {
            this.state.next(update(this.state.value));
        } else {
            this.state.next(Object.assign({}, this.snapshot, update));
        }
    }
}