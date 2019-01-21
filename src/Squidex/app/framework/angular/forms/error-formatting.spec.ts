/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';

import { ValidatorsEx } from './validators';

import { formatError } from './error-formatting';

describe('formatErrors', () => {
    it('should format min', () => {
        const error = validate(1, Validators.min(2));

        expect(error).toEqual('MY_FIELD must be greater or equal to \'2\'.');
    });

    it('should format max', () => {
        const error = validate(3, Validators.max(2));

        expect(error).toEqual('MY_FIELD must be less or equal to \'2\'.');
    });

    it('should format required', () => {
        const error = validate(undefined, Validators.required);

        expect(error).toEqual('MY_FIELD is required.');
    });

    it('should format requiredTrue', () => {
        const error = validate(undefined, Validators.requiredTrue);

        expect(error).toEqual('MY_FIELD is required.');
    });

    it('should format email', () => {
        const error = validate('invalid', Validators.email);

        expect(error).toEqual('MY_FIELD must be an email address.');
    });

    it('should format minLength string', () => {
        const error = validate('x', Validators.minLength(2));

        expect(error).toEqual('MY_FIELD must have at least 2 character(s).');
    });

    it('should format maxLength string', () => {
        const error = validate('xxx', Validators.maxLength(2));

        expect(error).toEqual('MY_FIELD must not have more than 2 character(s).');
    });

    it('should format minLength array', () => {
        const error = validate([1], Validators.minLength(2));

        expect(error).toEqual('MY_FIELD must have at least 2 item(s).');
    });

    it('should format maxLength array', () => {
        const error = validate([1, 1, 1], Validators.maxLength(2));

        expect(error).toEqual('MY_FIELD must not have more than 2 item(s).');
    });

    it('should format match', () => {
        const error = validate('123', Validators.pattern('[A-Z]'));

        expect(error).toEqual('MY_FIELD does not match to the pattern.');
    });

    it('should format match with message', () => {
        const error = validate('123', ValidatorsEx.pattern('[A-Z]', 'Custom Message'));

        expect(error).toEqual('Custom Message');
    });

    it('should format between exactly', () => {
        const error = validate(2, ValidatorsEx.between(3, 3));

        expect(error).toEqual('MY_FIELD must be exactly \'3\'.');
    });

    it('should format between range', () => {
        const error = validate(2, ValidatorsEx.between(3, 5));

        expect(error).toEqual('MY_FIELD must be between \'3\' and \'5\'.');
    });

    it('should format betweenLength string exactly', () => {
        const error = validate('xx', ValidatorsEx.betweenLength(3, 3));

        expect(error).toEqual('MY_FIELD must have exactly 3 character(s).');
    });

    it('should format betweenLength string range', () => {
        const error = validate('xx', ValidatorsEx.betweenLength(3, 5));

        expect(error).toEqual('MY_FIELD must have between 3 and 5 character(s).');
    });

    it('should format betweenLength array exactly', () => {
        const error = validate([1], ValidatorsEx.betweenLength(3, 3));

        expect(error).toEqual('MY_FIELD must have exactly 3 item(s).');
    });

    it('should format betweenLength array range', () => {
        const error = validate([1, 1], ValidatorsEx.betweenLength(3, 5));

        expect(error).toEqual('MY_FIELD must have between 3 and 5 item(s).');
    });

    it('should format validDateTime', () => {
        const error = validate('invalid', ValidatorsEx.validDateTime());

        expect(error).toEqual('MY_FIELD is not a valid date time.');
    });

    it('should format validValues', () => {
        const error = validate(5, ValidatorsEx.validValues([1, 2, 3]));

        expect(error).toEqual('MY_FIELD is not a valid value.');
    });

    it('should format validArrayValues', () => {
        const error = validate([2, 4], ValidatorsEx.validArrayValues([1, 2, 3]));

        expect(error).toEqual('MY_FIELD contains an invalid value: 4.');
    });

    it('should format match', () => {
        const formControl1 = new FormControl(1);
        const formControl2 = new FormControl(2);

        const formGroup = new FormGroup({
            field1: formControl1,
            field2: formControl2
        });

        const formError = ValidatorsEx.match('field2', 'Passwords must match.')!(formControl1)!;
        const formMessage = formatError('MY_FIELD', Object.keys(formError)[0], Object.values(formError)[0], undefined);

        expect(formMessage).toEqual('Passwords must match.');

        formGroup.reset();
    });

    function validate(value: any, validator: ValidatorFn) {
        const formControl = new FormControl(value);

        const formError = validator(formControl)!;
        const formMessage = formatError('MY_FIELD', Object.keys(formError)[0], Object.values(formError)[0], value);

        return formMessage;
    }
});