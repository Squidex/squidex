/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, UntypedFormGroup } from '@angular/forms';
import { ExtendedFormGroup, UndefinableFormGroup } from './extended-form-group';

describe('UndefinableFormGroup', () => {
    it('should provide value even if controls are disabled', () => {
        const control = new ExtendedFormGroup({
            test1: new UntypedFormControl('1'),
            test2: new UntypedFormControl('2'),
        });

        expect(control.value).toEqual({ test1: '1', test2: '2' });

        assertValue(control, { test1: '1', test2: '2' }, () => {
            control.controls['test1'].disable();
        });
    });
});

describe('ExtendedFormGroup', () => {
    const tests = [{
        name: 'undefined (on)',
        undefinable: true,
        valueExpected: undefined,
        valueActual: undefined,
    }, {
        name: 'defined (on)',
        undefinable: true,
        valueExpected: { field: 1 },
        valueActual: { field: 1 },
    }, {
        name: 'defined (off)',
        undefinable: false,
        valueExpected: { field: 1 },
        valueActual: { field: 1 },
    }];

    it('should provide value even if controls are disabled', () => {
        const control = new ExtendedFormGroup({
            test1: new UntypedFormControl('1'),
            test2: new UntypedFormControl('2'),
        });

        expect(control.value).toEqual({ test1: '1', test2: '2' });

        assertValue(control, { test1: '1', test2: '2' }, () => {
            control.controls['test1'].disable();
        });
    });

    tests.forEach(x => {
        it(`should set value as <${x.name}>`, () => {
            const control = buildControl(x.undefinable);

            assertValue(control, x.valueExpected, () => {
                control.setValue(x.valueActual as any);
            });
        });
    });

    tests.forEach(x => {
        it(`should patch value as <${x.name}>`, () => {
            const control = buildControl(x.undefinable);

            assertValue(control, x.valueExpected, () => {
                control.patchValue(x.valueActual as any);
            });
        });
    });

    tests.forEach(x => {
        it(`should reset value as <${x.name}>`, () => {
            const control = buildControl(x.undefinable);

            assertValue(control, x.valueExpected, () => {
                control.reset(x.valueActual);
            });
        });
    });

    function buildControl(undefinable: boolean) {
        return undefinable ?
            new UndefinableFormGroup({
                field: new UntypedFormControl(),
            }) :
            new ExtendedFormGroup({
                field: new UntypedFormControl(),
            });
    }
});

function assertValue(control: UntypedFormGroup, expected: any, action: () => void) {
    let currentValue: any;

    control.valueChanges.subscribe(value => {
        currentValue = value;
    });

    action();

    expect(currentValue).toEqual(expected);
    expect(control.getRawValue()).toEqual(expected);
    expect(control.value).toEqual(expected);
}
