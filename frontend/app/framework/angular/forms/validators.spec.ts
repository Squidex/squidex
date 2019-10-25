/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, FormGroup, Validators } from '@angular/forms';

import { DateTime } from '@app/framework/internal';

import { ValidatorsEx } from './validators';

describe('ValidatorsEx.between', () => {
    it('should return null validator if no min value or max value', () => {
        const validator = ValidatorsEx.between(undefined, undefined);

        expect(validator).toBe(Validators.nullValidator);
    });

    it('should return null when value is equal to min and max', () => {
        const input = new FormControl(3);

        const error = <any>ValidatorsEx.between(3, 3)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is valid', () => {
        const input = new FormControl(4);

        const error = <any>ValidatorsEx.between(1, 5)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is null', () => {
        const input = new FormControl(null);

        const error = <any>ValidatorsEx.between(1, 5)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is undefined', () => {
        const input = new FormControl(undefined);

        const error = <any>ValidatorsEx.between(1, 5)(input);

        expect(error).toBeNull();
    });

    it('should return error if less than min', () => {
        const input = new FormControl(0);

        const error = <any>ValidatorsEx.between(1, undefined)(input);

        expect(error.min).toBeDefined();
    });

    it('should return error if greater than max', () => {
        const input = new FormControl(6);

        const error = <any>ValidatorsEx.between(undefined, 5)(input);

        expect(error.max).toBeDefined();
    });

    it('should return error if not in range', () => {
        const input = new FormControl(1);

        const error = <any>ValidatorsEx.between(2, 4)(input);

        expect(error.between).toBeDefined();
    });

    it('should return error if not equal to min and max', () => {
        const input = new FormControl(2);

        const error = <any>ValidatorsEx.between(3, 3)(input);

        expect(error.exactly).toBeDefined();
    });
});

describe('ValidatorsEx.betweenLength', () => {
    it('should return null validator if no min value or max value', () => {
        const validator = ValidatorsEx.betweenLength(undefined, undefined);

        expect(validator).toBe(Validators.nullValidator);
    });

    it('should return null when value is equal to min and max', () => {
        const input = new FormControl('xxx');

        const error = <any>ValidatorsEx.betweenLength(3, 3)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is valid', () => {
        const input = new FormControl('xxxx');

        const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is null', () => {
        const input = new FormControl(null);

        const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is undefined', () => {
        const input = new FormControl(undefined);

        const error = <any>ValidatorsEx.betweenLength(1, 5)(input);

        expect(error).toBeNull();
    });

    it('should return error if less than min', () => {
        const input = new FormControl('x');

        const error = <any>ValidatorsEx.betweenLength(2, undefined)(input);

        expect(error.minlength).toBeDefined();
    });

    it('should return error if greater than max', () => {
        const input = new FormControl('xxxxxx');

        const error = <any>ValidatorsEx.betweenLength(undefined, 5)(input);

        expect(error.maxlength).toBeDefined();
    });

    it('should return error if not in range', () => {
        const input = new FormControl('x');

        const error = <any>ValidatorsEx.betweenLength(2, 4)(input);

        expect(error.betweenlength).toBeDefined();
    });

    it('should return error if not equal to min and max', () => {
        const input = new FormControl('xx');

        const error = <any>ValidatorsEx.betweenLength(3, 3)(input);

        expect(error.exactlylength).toBeDefined();
    });
});

describe('ValidatorsEx.validDateTime', () => {
    it('should return null validator if valid is not defined', () => {
        const input = new FormControl(null);

        const error = <any>ValidatorsEx.validDateTime()(input);

        expect(error).toBeNull();
    });

    it('should return null if date time is valid', () => {
        const input = new FormControl(DateTime.now().toISOString());

        const error = ValidatorsEx.validDateTime()(input);

        expect(error).toBeNull();
    });

    it('should return error if value is invalid date', () => {
        const input = new FormControl('invalid');

        const error = <any>ValidatorsEx.validDateTime()(input);

        expect(error.validdatetime).toBeDefined();
    });
});

describe('ValidatorsEx.validValues', () => {
    it('should return null validator if values not defined', () => {
        const validator = ValidatorsEx.validValues(null!);

        expect(validator).toBe(Validators.nullValidator);
    });

    it('should return null if value is in allowed values', () => {
        const input = new FormControl(10);

        const error = ValidatorsEx.validValues([10, 20, 30])(input);

        expect(error).toBeNull();
    });

    it('should return error if value is not in allowed values', () => {
        const input = new FormControl(50);

        const error = <any>ValidatorsEx.validValues([10, 20, 30])(input);

        expect(error.validvalues).toBeDefined();
    });
});

describe('ValidatorsEx.validArrayValues', () => {
    it('should return null validator if values not defined', () => {
        const validator = ValidatorsEx.validArrayValues(null!);

        expect(validator).toBe(Validators.nullValidator);
    });

    it('should return null if value is in allowed values', () => {
        const input = new FormControl([10, 20]);

        const error = ValidatorsEx.validArrayValues([10, 20, 30])(input);

        expect(error).toBeNull();
    });

    it('should return error if value is not in allowed values', () => {
        const input = new FormControl([50]);

        const error = <any>ValidatorsEx.validArrayValues([10, 20, 30])(input);

        expect(error.validarrayvalues).toBeDefined();
    });
});

describe('ValidatorsEx.match', () => {
    it('should revalidate if other control changes', () => {
        const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

        const form = new FormGroup({
            password: new FormControl('1'),
            passwordConfirm: new FormControl('2', validator)
        });

        form.controls['passwordConfirm'].setValue('1');

        expect(form.valid).toBeTruthy();

        form.controls['password'].setValue('2');

        expect(form.controls['password'].valid).toBeTruthy();
        expect(form.controls['passwordConfirm'].valid).toBeFalsy();
    });

    it('should return error if not the same value', () => {
        const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

        const form = new FormGroup({
            password: new FormControl('1'),
            passwordConfirm: new FormControl('2', validator)
        });

        expect(validator(form.controls['passwordConfirm'])).toEqual({ match: { message: 'Passwords are not the same.' }});
    });

    it('should return empty object if values are the same', () => {
        const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

        const form = new FormGroup({
            password: new FormControl('1'),
            passwordConfirm: new FormControl('1', validator)
        });

        expect(validator(form.controls['passwordConfirm'])).toBeNull();
    });

    it('should throw error if other object is not found', () => {
        const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

        const form = new FormGroup({
            passwordConfirm: new FormControl('2', validator)
        });

        expect(() => validator(form.controls['passwordConfirm'])).toThrow();
    });

    it('should return empty object if control has no parent', () => {
        const validator = ValidatorsEx.match('password', 'Passwords are not the same.');

        const control = new FormControl('2', validator);

        expect(validator(control)).toBeNull();
    });
});

describe('ValidatorsEx.pattern', () => {
    it('should return null validator if pattern not defined', () => {
        const validator = ValidatorsEx.pattern(undefined!, undefined);

        expect(validator).toBe(Validators.nullValidator);
    });

    it('should return null when value is valid pattern', () => {
        const input = new FormControl('1234');

        const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is null string', () => {
        const input = new FormControl(null);

        const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

        expect(error).toBeNull();
    });

    it('should return null when value is empty string', () => {
        const input = new FormControl('');

        const error = ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);

        expect(error).toBeNull();
    });

    it('should return error with message if value does not match pattern string', () => {
        const input = new FormControl('abc');

        const error = <any>ValidatorsEx.pattern('[0-9]{1,4}', 'My-Message')(input);
        const expected: any = {
            patternmessage: {
                requiredPattern: '^[0-9]{1,4}$', actualValue: 'abc', message: 'My-Message'
            }
        };

        expect(error).toEqual(expected);
    });

    it('should return error with message if value does not match pattern', () => {
        const input = new FormControl('abc');

        const error = <any>ValidatorsEx.pattern(/^[0-9]{1,4}$/, 'My-Message')(input);
        const expected: any = {
            patternmessage: {
                requiredPattern: '/^[0-9]{1,4}$/', actualValue: 'abc', message: 'My-Message'
            }
        };

        expect(error).toEqual(expected);
    });

    it('should return error without message if value does not match pattern string', () => {
        const input = new FormControl('abc');

        const error = <any>ValidatorsEx.pattern('[0-9]{1,4}')(input);
        const expected: any = {
            pattern: {
                requiredPattern: '^[0-9]{1,4}$', actualValue: 'abc'
            }
        };

        expect(error).toEqual(expected);
    });

    it('should return error without message if value does not match pattern', () => {
        const input = new FormControl('abc');

        const error = <any>ValidatorsEx.pattern(/^[0-9]{1,4}$/)(input);
        const expected: any = {
            pattern: {
                requiredPattern: '/^[0-9]{1,4}$/', actualValue: 'abc'
            }
        };

        expect(error).toEqual(expected);
    });
});

describe('ValidatorsEx.uniqueStrings', () => {
    it('should return null when value is null', () => {
        const input = new FormControl(null);

        const error = ValidatorsEx.uniqueStrings()(input);

        expect(error).toBeNull();
    });

    it('should return null when value is not a string array', () => {
        const input = new FormControl([1, 2, 3]);

        const error = ValidatorsEx.uniqueStrings()(input);

        expect(error).toBeNull();
    });

    it('should return null when values are unique', () => {
        const input = new FormControl(['1', '2', '3']);

        const error = ValidatorsEx.uniqueStrings()(input);

        expect(error).toBeNull();
    });

    it('should return error when values are not unique', () => {
        const input = new FormControl(['1', '2', '2', '3']);

        const error = ValidatorsEx.uniqueStrings()(input);

        expect(error).toEqual({ uniquestrings: false });
    });
});