/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { FormControl } from '@angular/forms';

import { Validators } from './../';

describe('Validators', () => {
    let validateBetween: any;

    beforeEach(() => {
        validateBetween = Validators.between(10, 200);
    });

    it('should return error when not a number', () => {
        const input = new FormControl('text');

        const error = validateBetween(input);

        expect(error.validNumber).toBeFalsy();
    });

    it('should return error if less than minimum setting', () => {
        const input = new FormControl(5);

        const error = validateBetween(input);

        expect(error.minValue).toBeDefined();
    });

    it('should return error if greater than maximum setting', () => {
        const input = new FormControl(300);

        const error = validateBetween(input);

        expect(error.maxValue).toBeDefined();
    });

    it('should return empty value when value is valid', () => {
        const input = new FormControl(50);

        const error = validateBetween(input);

        expect(error).toBeDefined();
    });
});
