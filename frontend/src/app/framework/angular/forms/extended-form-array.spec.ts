/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormArray, UntypedFormControl } from '@angular/forms';
import { ExtendedFormArray, UndefinableFormArray } from './extended-form-array';

describe('ExtendedFormArray', () => {
    it('should provide value even if controls are disabled', () => {
        const control = new ExtendedFormArray([
            new UntypedFormControl('1'),
            new UntypedFormControl('2'),
        ]);

        expect(control.value).toEqual(['1', '2']);

        assertValue(control, ['1', '2'], () => {
            control.controls[0].disable();
        });
    });
});

describe('UndefinableFormArray', () => {
    const tests = [{
        name: 'undefined (on)',
        undefinable: true,
        valueExpected: undefined,
        valueActual: undefined,
    }, {
        name: 'defined (on)',
        undefinable: true,
        valueExpected: [1],
        valueActual: [1],
    }, {
        name: 'defined (off)',
        undefinable: false,
        valueExpected: [1],
        valueActual: [1],
    }];

    it('should provide value even if controls are disabled', () => {
        const control = new UndefinableFormArray([
            new UntypedFormControl('1'),
            new UntypedFormControl('2'),
        ]);

        expect(control.value).toEqual(['1', '2']);

        assertValue(control, ['1', '2'], () => {
            control.controls[0].disable();
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
                control.reset(x.valueActual as any);
            });
        });
    });

    it('should reset value back after push', () => {
        const control = new UndefinableFormArray([]);

        assertValue(control, ['1'], () => {
            control.setValue(undefined);
            control.push(new UntypedFormControl('1'));
        });
    });

    it('should reset value back after insert', () => {
        const control = new UndefinableFormArray([]);

        assertValue(control, ['1'], () => {
            control.setValue(undefined);
            control.insert(0, new UntypedFormControl('1'));
        });
    });

    function buildControl(undefinable: boolean) {
        return undefinable ?
            new UndefinableFormArray([
                new UntypedFormControl(''),
            ]) :
            new ExtendedFormArray([
                new UntypedFormControl(''),
            ]);
    }
});

function assertValue(control: UntypedFormArray, expected: any, action: () => void) {
    let currentValue: any;

    control.valueChanges.subscribe(value => {
        currentValue = value;
    });

    action();

    expect(currentValue).toEqual(expected);
    expect(control.getRawValue()).toEqual(expected);
    expect(control.value).toEqual(expected);
}
