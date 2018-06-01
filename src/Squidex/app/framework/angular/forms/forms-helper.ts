/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

 import { AbstractControl, FormArray, FormGroup } from '@angular/forms';

import { Types } from '@app/framework/internal';

export const formControls = (form: AbstractControl): AbstractControl[] => {
    if (Types.is(form, FormGroup)) {
        return Object.values(form.controls);
    } else if (Types.is(form, FormArray)) {
        return form.controls;
    } else {
        return [];
    }
};

export const fullValue = (form: AbstractControl): any => {
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
};