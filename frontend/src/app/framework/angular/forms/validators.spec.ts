/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { DateTime } from '@app/framework/internal';
import { ValidatorsEx } from './validators';

describe('ValidatorsEx', () => {
    describe('between', () => {
        it('should return null validator if no min value or max value', () => {
            const validator = ValidatorsEx.between(undefined, undefined);

            expect(validator).toBe(Validators.nullValidator);
        });

        it('should return null if value is equal to min and max', () => {
            const input = new UntypedFormControl(3);

            const error = <any>ValidatorsEx.between(3, 3)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is valid', () => {
            const input = new UntypedFormControl(4);

            const error = <any>ValidatorsEx.between(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is null', () => {
            const input = new UntypedFormControl(null);

            const error = <any>ValidatorsEx.between(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is undefined', () => {
            const input = new UntypedFormControl(undefined);

            const error = <any>ValidatorsEx.between(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return error if less than min', () => {
            const input = new UntypedFormControl(0);

            const error = <any>ValidatorsEx.between(1, undefined)(input);

            expect(error.min).toBeDefined();
        });

        it('should return error if greater than max', () => {
            const input = new UntypedFormControl(6);

            const error = <any>ValidatorsEx.between(undefined, 5)(input);

            expect(error.max).toBeDefined();
        });

        it('should return error if not in range', () => {
            const input = new UntypedFormControl(1);

            const error = <any>ValidatorsEx.between(2, 4)(input);

            expect(error.between).toBeDefined();
        });

        it('should return error if not equal to min and max', () => {
            const input = new UntypedFormControl(2);

            const error = <any>ValidatorsEx.between(3, 3)(input);

            expect(error.exactly).toBeDefined();
        });
    });

    describe('betweenLength', () => {
        it('should return null validator if no min value or max value', () => {
            const validator = ValidatorsEx.betweenLength(undefined, undefined);

            expect(validator).toBe(Validators.nullValidator);
        });

        it('should return null if value is equal to min and max', () => {
            const input = new UntypedFormControl('xxx');

            const error = <any>ValidatorsEx.betweenLength(3, 3)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is valid', () => {
            const input = new UntypedFormControl('xxxx');

            const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is null', () => {
            const input = new UntypedFormControl(null);

            const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is undefined', () => {
            const input = new UntypedFormControl(undefined);

            const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

            expect(error).toBeNull();
        });

        it('should return error if less than min', () => {
            const input = new UntypedFormControl('x');

            const error = <any>ValidatorsEx.betweenLength(2, undefined)(input);

            expect(error.minlength).toBeDefined();
        });

        it('should return error if greater than max', () => {
            const input = new UntypedFormControl('xxxxxx');

            const error = <any>ValidatorsEx.betweenLength(undefined, 5)(input);

            expect(error.maxlength).toBeDefined();
        });

        it('should return error if not in range', () => {
            const input = new UntypedFormControl('x');

            const error = <any>ValidatorsEx.betweenLength(2, 4)(input);

            expect(error.betweenlength).toBeDefined();
        });

        it('should return error if not equal to min and max', () => {
            const input = new UntypedFormControl('xx');

            const error = <any>ValidatorsEx.betweenLength(3, 3)(input);

            expect(error.exactlylength).toBeDefined();
        });
    });

    describe('validDateTime', () => {
        it('should return null validator if valid is not defined', () => {
            const input = new UntypedFormControl(null);

            const error = <any>ValidatorsEx.validDateTime()(input);

            expect(error).toBeNull();
        });

        it('should return null if date time is valid', () => {
            const input = new UntypedFormControl(DateTime.now().toISOString());

            const error = ValidatorsEx.validDateTime()(input);

            expect(error).toBeNull();
        });

        it('should return error if value is invalid date', () => {
            const input = new UntypedFormControl('invalid');

            const error = <any>ValidatorsEx.validDateTime()(input);

            expect(error.validdatetime).toBeDefined();
        });
    });

    describe('validValues', () => {
        it('should return null validator if values not defined', () => {
            const validator = ValidatorsEx.validValues(null!);

            expect(validator).toBe(Validators.nullValidator);
        });

        it('should return null if value is in allowed values', () => {
            const input = new UntypedFormControl(10);

            const error = ValidatorsEx.validValues([10, 20, 30])(input);

            expect(error).toBeNull();
        });

        it('should return error if value is not in allowed values', () => {
            const input = new UntypedFormControl(50);

            const error = <any>ValidatorsEx.validValues([10, 20, 30])(input);

            expect(error.validvalues).toBeDefined();
        });
    });

    describe('validArrayValues', () => {
        it('should return null validator if values not defined', () => {
            const validator = ValidatorsEx.validArrayValues(null!);

            expect(validator).toBe(Validators.nullValidator);
        });

        it('should return null if value is in allowed values', () => {
            const input = new UntypedFormControl([10, 20]);

            const error = ValidatorsEx.validArrayValues([10, 20, 30])(input);

            expect(error).toBeNull();
        });

        it('should return error if value is not in allowed values', () => {
            const input = new UntypedFormControl([50]);

            const error = <any>ValidatorsEx.validArrayValues([10, 20, 30])(input);

            expect(error.validarrayvalues).toBeDefined();
        });
    });

    describe('match', () => {
        it('should revalidate if other control changes', () => {
            const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

            const form = new UntypedFormGroup({
                password: new UntypedFormControl('1'),
                passwordConfirm: new UntypedFormControl('2', validator),
            });

            form.controls['passwordConfirm'].setValue('1');

            expect(form.valid).toBeTruthy();

            form.controls['password'].setValue('2');

            expect(form.controls['password'].valid).toBeTruthy();
            expect(form.controls['passwordConfirm'].valid).toBeFalsy();
        });

        it('should return error if not the same value', () => {
            const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

            const form = new UntypedFormGroup({
                password: new UntypedFormControl('1'),
                passwordConfirm: new UntypedFormControl('2', validator),
            });

            expect(validator(form.controls['passwordConfirm'])).toEqual({ match: { message: 'Passwords are not the same.' } });
        });

        it('should return empty object if values are the same', () => {
            const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

            const form = new UntypedFormGroup({
                password: new UntypedFormControl('1'),
                passwordConfirm: new UntypedFormControl('1', validator),
            });

            expect(validator(form.controls['passwordConfirm'])).toBeNull();
        });

        it('should throw error if other object is not found', () => {
            const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

            const form = new UntypedFormGroup({
                passwordConfirm: new UntypedFormControl('2', validator),
            });

            expect(() => validator(form.controls['passwordConfirm'])).toThrow();
        });

        it('should return empty object if control has no parent', () => {
            const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

            const control = new UntypedFormControl('2', validator);

            expect(validator(control)).toBeNull();
        });
    });

    describe('pattern', () => {
        it('should return null validator if pattern not defined', () => {
            const validator = ValidatorsEx.pattern(undefined!, undefined);

            expect(validator).toBe(Validators.nullValidator);
        });

        it('should return null if value is valid pattern', () => {
            const input = new UntypedFormControl('1234');

            const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is null string', () => {
            const input = new UntypedFormControl(null);

            const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

            expect(error).toBeNull();
        });

        it('should return null if value is empty string', () => {
            const input = new UntypedFormControl('');

            const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

            expect(error).toBeNull();
        });

        it('should return error with message if value does not match pattern string', () => {
            const input = new UntypedFormControl('abc');

            const error = <any>ValidatorsEx.pattern('[0-9]{1,4}', 'My-Message')(input);
            const expected: any = {
                patternmessage: {
                    requiredPattern: '^[0-9]{1,4}$', actualValue: 'abc', message: 'My-Message',
                },
            };

            expect(error).toEqual(expected);
        });

        it('should return error with message if value does not match pattern', () => {
            const input = new UntypedFormControl('abc');

            const error = <any>ValidatorsEx.pattern(/^[0-9]{1,4}$/, 'My-Message')(input);
            const expected: any = {
                patternmessage: {
                    requiredPattern: '/^[0-9]{1,4}$/', actualValue: 'abc', message: 'My-Message',
                },
            };

            expect(error).toEqual(expected);
        });

        it('should return error without message if value does not match pattern string', () => {
            const input = new UntypedFormControl('abc');

            const error = <any>ValidatorsEx.pattern('[0-9]{1,4}')(input);
            const expected: any = {
                pattern: {
                    requiredPattern: '^[0-9]{1,4}$', actualValue: 'abc',
                },
            };

            expect(error).toEqual(expected);
        });

        it('should return error without message if value does not match pattern', () => {
            const input = new UntypedFormControl('abc');

            const error = <any>ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);
            const expected: any = {
                pattern: {
                    requiredPattern: '/^[0-9]{1,4}$/', actualValue: 'abc',
                },
            };

            expect(error).toEqual(expected);
        });
    });

    describe('uniqueStrings', () => {
        it('should return null if value is null', () => {
            const input = new UntypedFormControl(null);

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toBeNull();
        });

        it('should return null if value is not a string array', () => {
            const input = new UntypedFormControl([1, 2, 3]);

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toBeNull();
        });

        it('should return null if values are unique', () => {
            const input = new UntypedFormControl(['1', '2', '3']);

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toBeNull();
        });

        it('should return error if values are not unique', () => {
            const input = new UntypedFormControl(['1', '2', '2', '3']);

            const error = ValidatorsEx.uniqueStrings()(input);

            expect(error).toEqual({ uniquestrings: false });
        });
    });

    describe('uniqueObjectValues', () => {
        it('should return null if value is null', () => {
            const input = new UntypedFormControl(null);

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if value is not an object array', () => {
            const input = new UntypedFormControl([1, 2, 3]);

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if values array has one item', () => {
            const input = new UntypedFormControl([{}]);

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if values array has no duplicate', () => {
            const input = new UntypedFormControl([{ myString: '1' }, { myString: '2' }]);

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return null if values array has unchecked duplicate', () => {
            const input = new UntypedFormControl([{ other: '1' }, { other: '1' }]);

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toBeNull();
        });

        it('should return error if values array has duplicate', () => {
            const input = new UntypedFormControl([{ myString: '1' }, { myString: '1' }]);

            const error = ValidatorsEx.uniqueObjectValues(['myString'])(input);

            expect(error).toEqual({ uniqueobjectvalues: { fields: 'myString' } });
        });

        it('should return error if values array has multiple duplicates', () => {
            const input = new UntypedFormControl([{ myString: '1', myNumber: 2 }, { myString: '1', myNumber: 2 }]);

            const error = ValidatorsEx.uniqueObjectValues(['myString', 'myNumber'])(input);

            expect(error).toEqual({ uniqueobjectvalues: { fields: 'myString, myNumber' } });
        });
    });
});
