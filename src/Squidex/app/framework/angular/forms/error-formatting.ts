/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { Types } from './../../utils/types';

const DEFAULT_ERRORS: { [key: string]: string } = {
    between: '{field} must be between \'{min}\' and \'{max}\'.',
    betweenlength: '{field} must have between {minlength} and {maxlength} item(s).',
    betweenlengthstring: '{field} must have between {minlength} and {maxlength} character(s).',
    email: '{field} must be an email address.',
    exactly: '{field} must be exactly \'{expected}\'.',
    exactlylength: '{field} must have exactly {expected} item(s).',
    exactlylengthstring: '{field} must have exactly {expected} character(s).',
    match: '{message}',
    max: '{field} must be less or equal to \'{max}\'.',
    maxlength: '{field} must not have more than {requiredlength} item(s).',
    maxlengthstring: '{field} must not have more than {requiredlength} character(s).',
    min: '{field} must be greater or equal to \'{min}\'.',
    minlength: '{field} must have at least {requiredlength} item(s).',
    minlengthstring: '{field} must have at least {requiredlength} character(s).',
    pattern: '{field} does not match to the pattern.',
    patternmessage: '{message}',
    required: '{field} is required.',
    requiredTrue: '{field} is required.',
    validdatetime: '{field} is not a valid date time.',
    validvalues: '{field} is not a valid value.',
    validarrayvalues: '{field} contains an invalid value: {invalidvalue}.'
};

export function formatError(field: string, type: string, properties: any, value: any, errors?: any)  {
    type = type.toLowerCase();

    if (Types.isString(value)) {
        if (type === 'minlength') {
            type = 'minlengthstring';
        }

        if (type === 'maxlength') {
            type = 'maxlengthstring';
        }

        if (type === 'exactlylength') {
            type = 'exactlylengthstring';
        }

        if (type === 'betweenlength') {
            type = 'betweenlengthstring';
        }
    }

    let message = (errors ? errors[type] : null) || DEFAULT_ERRORS[type];

    if (!message) {
        return null;
    }

    for (let property in properties) {
        if (properties.hasOwnProperty(property)) {
            message = message.replace(`{${property.toLowerCase()}}`, properties[property]);
        }
    }

    message = message.replace('{field}', field);

    return message;
}