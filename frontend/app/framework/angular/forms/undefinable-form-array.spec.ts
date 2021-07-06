/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormControl } from '@angular/forms';
import { UndefinableFormArray } from './undefinable-form-array';

describe('UndefinableFormArray', () => {
    const tests = [{
        name: 'undefined',
        value: undefined,
    }, {
        name: 'defined',
        value: ['1'],
    }];

    tests.forEach(x => {
        it(`should set value as <${x.name}>`, () => {
            const control =
                new UndefinableFormArray([
                    new FormControl(''),
                ]);

            assertValue(control, x.value, () => {
                control.setValue(x.value);
            });
        });
    });

    tests.forEach(x => {
        it(`should patch value as <${x.name}>`, () => {
            const control =
                new UndefinableFormArray([
                    new FormControl(''),
                ]);

            assertValue(control, x.value, () => {
                control.patchValue(x.value);
            });
        });
    });

    tests.forEach(x => {
        it(`should reset value as <${x.name}>`, () => {
            const control =
                new UndefinableFormArray([
                    new FormControl(''),
                ]);

            assertValue(control, x.value, () => {
                control.reset(x.value);
            });
        });
    });

    it('should reset value back after push', () => {
        const control = new UndefinableFormArray([]);

        assertValue(control, ['1'], () => {
            control.setValue(undefined);
            control.push(new FormControl('1'));
        });
    });

    it('should reset value back after insert', () => {
        const control = new UndefinableFormArray([]);

        assertValue(control, ['1'], () => {
            control.setValue(undefined);
            control.insert(0, new FormControl('1'));
        });
    });

    function assertValue(control: FormArray, expected: any, action: () => void) {
        let currentValue: any;

        control.valueChanges.subscribe(value => {
            currentValue = value;
        });

        action();

        expect(currentValue).toEqual(expected);
        expect(control.getRawValue()).toEqual(expected);
    }
});
