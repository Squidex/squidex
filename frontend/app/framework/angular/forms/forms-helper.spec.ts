/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, Validators } from '@angular/forms';
import { value$ } from './forms-helper';

describe('FormHelpers', () => {
    describe('value$', () => {
        it('should provide change values', () => {
            const form = new FormControl('1', Validators.required);

            const values: any[] = [];

            value$(form).subscribe(x => {
                values.push(x);
            });

            form.setValue('2');
            form.setValue('3');

            expect(values).toEqual(['1', '2', '3']);
        });

        it('should also trigger on disable', () => {
            const form = new FormControl('1', Validators.required);

            const values: any[] = [];

            value$(form).subscribe(x => {
                values.push(x);
            });

            form.setValue('2');
            form.enable();
            form.setValue('3');
            form.disable();
            form.setValue('4');

            expect(values).toEqual(['1', '2', '3', '4']);
        });
    });
});