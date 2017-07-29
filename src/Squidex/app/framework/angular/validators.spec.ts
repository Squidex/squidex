/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { FormControl, FormGroup, Validators } from '@angular/forms';

import { DateTime } from './../utils/date-time';
import { ValidatorsEx } from './../';

describe('ValidatorsEx.between', () => {
    it('should return null validator if no min value or max value', () => {
        const validator = ValidatorsEx.between(undefined, undefined);

        expect(validator).toBe(Validators.nullValidator);
    });

    it('should return null when value is valid', () => {
        const input = new FormControl(4);

        const error = <any>ValidatorsEx.between(1, 5)(input);

        expect(error).toBeNull();
    });

    it('should return error when not a number', () => {
        const input = new FormControl('text');

        const error = <any>ValidatorsEx.between(1, 5)(input);

        expect(error.validnumber).toBeFalsy();
    });

    it('should return error if less than minimum setting', () => {
        const input = new FormControl(-5);

        const error = <any>ValidatorsEx.between(1, 5)(input);

        expect(error.minvalue).toBeDefined();
    });

    it('should return error if greater than maximum setting', () => {
        const input = new FormControl(300);

        const error = <any>ValidatorsEx.between(1, 5)(input);

        expect(error.maxvalue).toBeDefined();
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

describe('ValidatorsEx.noop', () => {
    it('should return null validator', () => {
        const input = new FormControl(null);

        const error = <any>ValidatorsEx.noop()(input);

        expect(error).toBeNull();
    });
});