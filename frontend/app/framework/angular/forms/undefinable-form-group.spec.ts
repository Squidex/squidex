/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, FormGroup } from '@angular/forms';
import { UndefinableFormGroup } from './undefinable-form-group';

describe('UndefinableFormGroup', () => {
    const tests = [{
        name: 'undefined',
        value: undefined,
    }, {
        name: 'defined',
        value: { field: ['1'] },
    }];

    it('should provide value even if controls required', () => {
        const control = new UndefinableFormGroup({
            test1: new FormControl('1'),
            test2: new FormControl('2'),
        });

        expect(control.value).toEqual({ test1: '1', test2: '2' });

        control.controls['test1'].disable();

        expect(control.value).toEqual({ test1: '1', test2: '2' });
    });

    tests.forEach(x => {
        it(`should set value as <${x.name}>`, () => {
            const control =
                new UndefinableFormGroup({
                    field: new FormControl(),
                });

            assertValue(control, x.value, () => {
                control.setValue(x.value);
            });
        });
    });

    tests.forEach(x => {
        it(`should patch value as <${x.name}>`, () => {
            const control =
                new UndefinableFormGroup({
                    field: new FormControl(),
                });

            assertValue(control, x.value, () => {
                control.patchValue(x.value);
            });
        });
    });

    tests.forEach(x => {
        it(`should reset value as <${x.name}>`, () => {
            const control =
                new UndefinableFormGroup({
                    field: new FormControl(),
                });

            assertValue(control, x.value, () => {
                control.reset(x.value);
            });
        });
    });

    function assertValue(control: FormGroup, expected: any, action: () => void) {
        let currentValue: any;

        control.valueChanges.subscribe(value => {
            currentValue = value;
        });

        action();

        expect(currentValue).toEqual(expected);
        expect(control.getRawValue()).toEqual(expected);
    }
});
