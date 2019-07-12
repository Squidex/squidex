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

export function formControls(form: AbstractControl): AbstractControl[] {
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
    return form.valueChanges.pipe(startWith(form.value));
}

export function hasValue$(form: AbstractControl): Observable<boolean> {
    return value$(form).pipe(map(v => !!v));
}

export function hasNoValue$(form: AbstractControl): Observable<boolean> {
    return value$(form).pipe(map(v => !v));
}

export function fullValue(form: AbstractControl): any {
    if (Types.is(form, FormGroup)) {
        const groupValue = {};

        for (let key in form.controls) {
            if (form.controls.hasOwnProperty(key)) {
                groupValue[key] = fullValue(form.controls[key]);
            }
        }

        return groupValue;
    } else if (Types.is(form, FormArray)) {
        const arrayValue = [];

        for (let child of form.controls) {
            arrayValue.push(fullValue(child));
        }

        return arrayValue;
    } else {
        return form.value;
    }
}