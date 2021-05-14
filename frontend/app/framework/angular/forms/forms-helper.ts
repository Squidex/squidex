/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, FormArray, FormGroup, ValidatorFn } from '@angular/forms';
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

export function updateAll(form: AbstractControl) {
    form.updateValueAndValidity({ onlySelf: true, emitEvent: true });

    for (const child of formControls(form)) {
        updateAll(child);
    }
}

export function addValidator(form: AbstractControl, validator: ValidatorFn) {
    if (form.validator) {
        form.setValidators([form.validator, validator]);
    } else {
        form.setValidators(validator);
    }

    for (const child of formControls(form)) {
        addValidator(child, validator);
    }
}

export function getControlPath(control: AbstractControl | undefined | null, apiCompatible = false): string {
    if (!control || !control.parent) {
        return '';
    }

    let name = '';

    if (control.parent instanceof FormGroup) {
        for (const key in control.parent.controls) {
            if (control.parent.controls[key] === control) {
                name = key;
            }
        }
    } else if (control.parent) {
        for (let i = 0; i < control.parent.controls.length; i++) {
            if (control.parent.controls[i] === control) {
                if (apiCompatible) {
                    name = `[${i + 1}]`;
                } else {
                    name = i.toString();
                }
                break;
            }
        }
    }

    if (!name) {
        return '';
    }

    const parentName = getControlPath(control.parent, apiCompatible);

    if (parentName) {
        if (name.startsWith('[')) {
            return `${parentName}${name}`;
        } else {
            return `${parentName}.${name}`;
        }
    }

    return name;
}

export function disabled$(form: AbstractControl): Observable<boolean> {
    return form.statusChanges.pipe(map(() => form.disabled), startWith(form.disabled), distinctUntilChanged());
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

export function hasNonCustomError(form: AbstractControl) {
    if (form.errors) {
        for (const key in form.errors) {
            if (key !== 'custom') {
                return true;
            }
        }
    }

    if (Types.is(form, FormGroup)) {
        for (const key in form.controls) {
            if (hasNonCustomError(form.controls[key])) {
                return true;
            }
        }
    } else if (Types.is(form, FormArray)) {
        for (const control of form.controls) {
            if (hasNonCustomError(control)) {
                return true;
            }
        }
    }

    return false;
}
