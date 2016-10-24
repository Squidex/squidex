/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2Forms from '@angular/forms';

export class Validators {
    public static between(minValue: number, maxValue: number) {
        return (control: Ng2Forms.AbstractControl): { [key: string]: any } => {
            let n: number = control.value;

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