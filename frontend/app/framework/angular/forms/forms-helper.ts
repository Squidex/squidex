/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, FormArray, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, startWith } from 'rxjs/operators';
import { Types } from './../../utils/types';

export interface FocusComponent {
    focus(): void;
}

export function formControls(form: AbstractControl): ReadonlyArray<AbstractControl> {
    if (Types.is(form, FormGroup)) {
        return Object.values(form.controls);
    } else if (Types.is(form, FormArray)) {
        return form.controls;
    } else {
        return [];
    }
}

export function invalid$(form: AbstractControl): Observable<boolean> {
    return form.statusChanges.pipe(map(() => form.invalid), startWith(form.invalid), distinctUntilChanged());
}

export function value$<T = any>(form: AbstractControl): Observable<T> {
    return form.valueChanges.pipe(startWith(form.value), distinctUntilChanged());
}

export function valueAll$<T = any>(form: AbstractControl): Observable<T> {
    return form.valueChanges.pipe(map(() => getRawValue(form)), startWith(getRawValue(form)), distinctUntilChanged());
}

export function hasValue$(form: AbstractControl): Observable<boolean> {
    return value$(form).pipe(map(v => !!v));
}

export function hasNoValue$(form: AbstractControl): Observable<boolean> {
    return value$(form).pipe(map(v => !v));
}

export function getRawValue(form: AbstractControl): any {
    if (Types.is(form, FormGroup)) {
        return form.getRawValue();
    } else if (Types.is(form, FormArray)) {
        return form.getRawValue();
    } else {
        return form.value;
    }
}