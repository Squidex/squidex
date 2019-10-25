/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl } from '@angular/forms';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { distinctUntilChanged, map, shareReplay } from 'rxjs/operators';

import { getRawValue } from './angular/forms/forms-helper';

import { ErrorDto } from './utils/error';
import { ResourceLinks } from './utils/hateos';
import { Types } from './utils/types';

export interface FormState {
    submitted: boolean;

    error?: ErrorDto | null;
}

export class Form<T extends AbstractControl, V> {
    private readonly state = new State<FormState>({ submitted: false });

    public submitted =
        this.state.project(s => s.submitted);

    public error =
        this.state.project(s => s.error);

    constructor(
        public readonly form: T
    ) {
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
        this.state.next({ submitted: false, error: null });

        this.setValue(value);
    }

    public submit(): V | null {
        this.state.next({ submitted: true, error: null });

        if (this.form.valid) {
            const value = this.transformSubmit(getRawValue(this.form));

            if (value) {
                this.disable();
            }

            return value;
        } else {
            return null;
        }
    }

    public submitCompleted(options?: { newValue?: V, noReset?: boolean }) {
        this.state.next({ submitted: false, error: null });

        this.enable();

        if (options && options.noReset) {
            this.form.markAsPristine();
        } else {
            this.setValue(options ? options.newValue : undefined);
        }
    }

    public submitFailed(error?: string | ErrorDto) {
        if (Types.isString(error)) {
            error = new ErrorDto(500, error);
        }

        this.state.next({ submitted: false, error });

        this.enable();
    }
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

        for (const key in values) {
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

export class ResultSet<T> {
    public readonly _links: ResourceLinks;

    constructor(
        public readonly total: number,
        public readonly items: ReadonlyArray<T>,
        links?: ResourceLinks
    ) {
        this._links = links || {};
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

    public project<M>(project: (value: T) => M, compare?: (x: M, y: M) => boolean) {
        return this.changes.pipe(
            map(x => project(x)), distinctUntilChanged(compare), shareReplay(1));
    }

    public projectFrom<M, N>(source: Observable<M>, project: (value: M) => N, compare?: (x: N, y: N) => boolean) {
        return source.pipe(
            map(x => project(x)), distinctUntilChanged(compare), shareReplay(1));
    }

    public projectFrom2<M, N, O>(lhs: Observable<M>, rhs: Observable<N>, project: (l: M, r: N) => O, compare?: (x: O, y: O) => boolean) {
        return combineLatest(lhs, rhs, (x, y) => project(x, y)).pipe(
            distinctUntilChanged(compare), shareReplay(1));
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