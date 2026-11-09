/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalizerService } from '../../services/localizer.service';
import { formatError } from './error-formatting';
import { ValidatorsEx, ValueValidator } from './validators';

describe('formatErrors', () => {
    const localizer = new LocalizerService({
        'common.backendError': 'Backend Error',
        'users.passwordConfirmValidationMessage': 'Passwords must be the same.',
        'validation.between': '{field} must be between \'{min}\' and \'{max}\'.',
        'validation.betweenlength': '{field|upper} must have between {minlength} and {maxlength} item(s).',
        'validation.betweenlengthstring': '{field|upper} must have between {minlength} and {maxlength} character(s).',
        'validation.email': '{field|upper} must be an email address.',
        'validation.exactly': '{field|upper} must be exactly \'{expected}\'.',
        'validation.exactlylength': '{field|upper} must have exactly {expected} item(s).',
        'validation.exactlylengthstring': '{field|upper} must have exactly {expected} character(s).',
        'validation.match': '{message}',
        'validation.max': '{field|upper} must be less or equal to \'{max}\'.',
        'validation.maxlength': '{field|upper} must not have more than {requiredlength} item(s).',
        'validation.maxlengthstring': '{field|upper} must not have more than {requiredlength} character(s).',
        'validation.min': '{field|upper} must be greater or equal to \'{min}\'.',
        'validation.minlength': '{field|upper} must have at least {requiredlength} item(s).',
        'validation.minlengthstring': '{field|upper} must have at least {requiredlength} character(s).',
        'validation.pattern': '{field|upper} does not match to the pattern.',
        'validation.patternmessage': '{message}',
        'validation.required': '{field|upper} is required.',
        'validation.requiredTrue': '{field|upper} is required.',
        'validation.uniquestrings': '{field|upper} must not contain duplicate values.',
        'validation.validarrayvalues': '{field|upper} contains an invalid value: {invalidvalue}.',
        'validation.validdatetime': '{field|upper} is not a valid date time.',
        'validation.validvalues': '{field|upper} is not a valid value.',
    });

    it('should format custom', () => {
        const error = formatError(localizer, 'field', 'custom', {
            errors: [
                'My Message.',
            ],
        }, 123);

        expect(error).toEqual(['Backend Error: My Message.']);
    });

    it('should format custom errors', () => {
        const error = formatError(localizer, 'field', 'custom', {
            errors: [
                'My Message1.',
                'My Message2.',
            ],
        }, 123);

        expect(error).toEqual(['Backend Error: My Message1.', 'Backend Error: My Message2.']);
    });

    it('should format custom errors without dots', () => {
        const error = formatError(localizer, 'field', 'custom', {
            errors: [
                'My Message1',
                'My Message2',
            ],
        }, 123);

        expect(error).toEqual(['Backend Error: My Message1.', 'Backend Error: My Message2.']);
    });

    it('should format min', () => {
        const error = validate(1, ValidatorsEx.min(2));

        expect(error).toEqual('MY_FIELD must be greater or equal to \'2\'.');
    });

    it('should format max', () => {
        const error = validate(3, ValidatorsEx.max(2));

        expect(error).toEqual('MY_FIELD must be less or equal to \'2\'.');
    });

    it('should format required', () => {
        const error = validate(undefined, ValidatorsEx.required());

        expect(error).toEqual('MY_FIELD is required.');
    });

    it('should format requiredTrue', () => {
        const error = validate(undefined, () => ({ kind: 'required' }));

        expect(error).toEqual('MY_FIELD is required.');
    });

    it('should format email', () => {
        const error = validate('invalid', () => ({ kind: 'email' }));

        expect(error).toEqual('MY_FIELD must be an email address.');
    });

    it('should format minLength string', () => {
        const error = validate('x', ValidatorsEx.minLength(2));

        expect(error).toEqual('MY_FIELD must have at least 2 character(s).');
    });

    it('should format maxLength string', () => {
        const error = validate('xxx', ValidatorsEx.maxLength(2));

        expect(error).toEqual('MY_FIELD must not have more than 2 character(s).');
    });

    it('should format minLength array', () => {
        const error = validate([1], ValidatorsEx.minLength(2));

        expect(error).toEqual('MY_FIELD must have at least 2 item(s).');
    });

    it('should format maxLength array', () => {
        const error = validate([1, 1, 1], ValidatorsEx.maxLength(2));

        expect(error).toEqual('MY_FIELD must not have more than 2 item(s).');
    });

    it('should format match', () => {
        const formMessage = formatError(localizer, 'MY_FIELD', 'match', { message: 'i18n:users.passwordConfirmValidationMessage' }, undefined);

        expect(formMessage).toEqual('Passwords must be the same.');
    });

    function validate(value: any, validator: ValueValidator) {
        const formError = validator(value)!;
        const formMessage = formatError(localizer, 'MY_FIELD', formError.kind, formError, value);

        return formMessage;
    }
});
