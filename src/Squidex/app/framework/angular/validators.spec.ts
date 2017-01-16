/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { FormControl, Validators } from '@angular/forms';

import { ValidatorsEx } from './../';

describe('Validators', () => {
    let validateBetween: any;

    beforeEach(() => {
        validateBetween = ValidatorsEx.between(10, 200);
    });

    it('should return null validator if no min value or max value', () => {
        const validator = ValidatorsEx.between(undefined, undefined);

        expect(validator).toBe(Validators.nullValidator);
    });

    it('should return error when not a number', () => {
        const input = new FormControl('text');

        const error = validateBetween(input);

        expect(error.validnumber).toBeFalsy();
    });

    it('should return error if less than minimum setting', () => {
        const input = new FormControl(5);

        const error = validateBetween(input);

        expect(error.minvalue).toBeDefined();
    });

    it('should return error if greater than maximum setting', () => {
        const input = new FormControl(300);

        const error = validateBetween(input);

        expect(error.maxvalue).toBeDefined();
    });

    it('should return empty value when value is valid', () => {
        const input = new FormControl(50);

        const error = validateBetween(input);

        expect(error).toEqual({});
    });

    it('should return empty value if value is in allowed values', () => {
        const input = new FormControl(10);

        const error = ValidatorsEx.validValues([10, 20, 30])(input);

        expect(error).toEqual({});
    });

    it('should return error if value is not in allowed values', () => {
        const input = new FormControl(50);

        const error = <any>ValidatorsEx.validValues([10, 20, 30])(input);

        expect(error.validvalues).toBeDefined();
    });
});
