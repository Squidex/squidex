/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AbstractControl } from '@angular/forms';

export class Validators {
    public static between(minValue: number, maxValue: number) {
        return (control: AbstractControl): { [key: string]: any } => {
            const n: number = control.value;

            if (typeof n !== 'number') {
                return { 'validNumber': false };
            } else if (n < minValue) {
                return { 'minValue': { 'requiredValue': minValue, 'actualValue': n } };
            } else if (n > maxValue) {
                return { 'maxValue': { 'requiredValue': maxValue, 'actualValue': n } };
            }

            return {};
        };
    }
}