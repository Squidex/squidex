/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { getControlPath, hasNoValue$, hasValue$, touchedChange$, value$ } from './forms-helper';

describe('FormHelpers', () => {
    describe('value$', () => {
        it('should provide change values', () => {
            const form = new UntypedFormControl('1', Validators.required);

            const values: any[] = [];

            value$(form).subscribe(x => {
                values.push(x);
            });

            form.setValue('2');
            form.setValue('3');

            expect(values).toEqual(['1', '2', '3']);
        });

        it('should also trigger on disable', () => {
            const form = new UntypedFormControl('1', Validators.required);

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

    it('should provide touched changes', () => {
        const form = new UntypedFormControl('1', Validators.required);

        const values: any[] = [];

        touchedChange$(form).subscribe(x => {
            values.push(x);
        });

        form.markAsTouched();
        form.markAsUntouched();
        form.markAsTouched();

        expect(values).toEqual([false, true, false, true]);
    });

    it('should provide value when defined', () => {
        const form = new UntypedFormControl('1', Validators.required);

        const values: any[] = [];

        hasValue$(form).subscribe(x => {
            values.push(x);
        });

        form.setValue(undefined);
        form.setValue('1');
        form.setValue(null);

        expect(values).toEqual([true, false, true, false]);
    });

    it('should provide value when defined', () => {
        const form = new UntypedFormControl('1', Validators.required);

        const values: any[] = [];

        hasNoValue$(form).subscribe(x => {
            values.push(x);
        });

        form.setValue(undefined);
        form.setValue('1');
        form.setValue(null);

        expect(values).toEqual([false, true, false, true]);
    });

    describe('getControlPath', () => {
        it('should calculate path for standalone control', () => {
            const control = new UntypedFormControl();

            const path = getControlPath(control);

            expect(path).toEqual('');
        });

        it('should calculate path for nested control', () => {
            const control = new UntypedFormGroup({
                nested: new UntypedFormControl(),
            });

            const path = getControlPath(control.get('nested'));

            expect(path).toEqual('nested');
        });

        it('should calculate path for deeply nested control', () => {
            const control = new UntypedFormGroup({
                nested1: new UntypedFormGroup({
                    nested2: new UntypedFormControl(),
                }),
            });

            const path = getControlPath(control.get('nested1.nested2'));

            expect(path).toEqual('nested1.nested2');
        });

        it('should calculate path for deeply nested array control', () => {
            const control = new UntypedFormGroup({
                nested1: new UntypedFormArray([
                    new UntypedFormGroup({
                        nested2: new UntypedFormControl(),
                    }),
                ]),
            });

            const path = getControlPath(control.get('nested1.0.nested2'));

            expect(path).toEqual('nested1.0.nested2');
        });

        it('should calculate api compatible path for deeply nested array control', () => {
            const control = new UntypedFormGroup({
                nested1: new UntypedFormArray([
                    new UntypedFormGroup({
                        nested2: new UntypedFormControl(),
                    }),
                ]),
            });

            const path = getControlPath(control.get('nested1.0.nested2'), true);

            expect(path).toEqual('nested1[1].nested2');
        });
    });
});
