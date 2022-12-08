/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormArray, UntypedFormControl, UntypedFormGroup } from '@angular/forms';
import { ErrorDto } from '@app/framework/internal';
import { ErrorValidator } from './error-validator';

describe('ErrorValidator', () => {
    const validator = new ErrorValidator();

    const control = new UntypedFormGroup({
        nested1: new UntypedFormArray([
            new UntypedFormGroup({
                nested2: new UntypedFormControl(),
            }),
        ]),
    });

    beforeEach(() => {
        control.reset([]);
    });

    it('should return no message if error is null', () => {
        validator.setError(null);

        const error = validator.validator(control);

        expect(error).toBeNull();
    });

    it('should return no message if error does not match', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1Property: My Error.',
        ]));

        const error = validator.validator(control.get('nested1')!);

        expect(error).toBeNull();
    });

    it('should return matching error', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'other, nested1: My Error.',
        ]));

        const error = validator.validator(control.get('nested1')!);

        expect(error).toEqual({
            custom: {
                errors: ['My Error.'],
            },
        });
    });

    it('should return matching error twice if value does not change', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1: My Error.',
        ]));

        const error1 = validator.validator(control.get('nested1')!);
        const error2 = validator.validator(control.get('nested1')!);

        expect(error1).toEqual({
            custom: {
                errors: ['My Error.'],
            },
        });

        expect(error2).toEqual({
            custom: {
                errors: ['My Error.'],
            },
        });
    });

    it('should not return matching error again if value has changed', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1[1].nested2: My Error.',
        ]));

        const nested = control.get('nested1.0.nested2');

        nested?.setValue('a');
        const error1 = validator.validator(nested!);

        nested?.setValue('b');
        const error2 = validator.validator(nested!);

        expect(error1).toEqual({
            custom: {
                errors: ['My Error.'],
            },
        });

        expect(error2).toBeNull();
    });

    it('should not return matching error again if value has changed to initial', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1[1].nested2: My Error.',
        ]));

        const nested = control.get('nested1.0.nested2');

        nested?.setValue('a');
        const error1 = validator.validator(nested!);

        nested?.setValue('b');
        const error2 = validator.validator(nested!);

        nested?.setValue('a');
        const error3 = validator.validator(nested!);

        expect(error1).toEqual({
            custom: {
                errors: ['My Error.'],
            },
        });

        expect(error2).toBeNull();
        expect(error3).toBeNull();
    });

    it('should return matching errors', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1: My Error1.',
            'nested1: My Error2.',
        ]));

        const error = validator.validator(control.get('nested1')!);

        expect(error).toEqual({
            custom: {
                errors: ['My Error1.', 'My Error2.'],
            },
        });
    });

    it('should return deeply matching error', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1[1].nested2: My Error.',
        ]));

        const error = validator.validator(control.get('nested1.0.nested2')!);

        expect(error).toEqual({
            custom: {
                errors: ['My Error.'],
            },
        });
    });

    it('should return partial matching error', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1[1].nested2: My Error.',
        ]));

        const error = validator.validator(control.get('nested1.0')!);

        expect(error).toEqual({
            custom: {
                errors: ['nested2: My Error.'],
            },
        });
    });

    it('should return partial matching index error', () => {
        validator.setError(new ErrorDto(500, 'Error', null, [
            'nested1[1].nested2: My Error.',
        ]));

        const error = validator.validator(control.get('nested1')!);

        expect(error).toEqual({
            custom: {
                errors: ['[1].nested2: My Error.'],
            },
        });
    });
});
