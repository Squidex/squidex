/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { getControlPath, value$ } from './forms-helper';

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

    describe('getControlPath', () => {
        it('should calculate path for standalone control', () => {
            const control = new FormControl();

            const path = getControlPath(control);

            expect(path).toEqual('');
        });

        it('should calculate path for nested control', () => {
            const control = new FormGroup({
                nested: new FormControl(),
            });

            const path = getControlPath(control.get('nested'));

            expect(path).toEqual('nested');
        });

        it('should calculate path for deeply nested control', () => {
            const control = new FormGroup({
                nested1: new FormGroup({
                    nested2: new FormControl(),
                }),
            });

            const path = getControlPath(control.get('nested1.nested2'));

            expect(path).toEqual('nested1.nested2');
        });

        it('should calculate path for deeply nested array control', () => {
            const control = new FormGroup({
                nested1: new FormArray([
                    new FormGroup({
                        nested2: new FormControl(),
                    }),
                ]),
            });

            const path = getControlPath(control.get('nested1.0.nested2'));

            expect(path).toEqual('nested1.0.nested2');
        });

        it('should calculate api compatible path for deeply nested array control', () => {
            const control = new FormGroup({
                nested1: new FormArray([
                    new FormGroup({
                        nested2: new FormControl(),
                    }),
                ]),
            });

            const path = getControlPath(control.get('nested1.0.nested2'), true);

            expect(path).toEqual('nested1[1].nested2');
        });
    });
});
