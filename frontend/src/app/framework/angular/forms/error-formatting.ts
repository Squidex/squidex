/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { LocalizerService, StringHelper, Types } from '@app/framework/internal';

export function formatError(localizer: LocalizerService, field: string, type: string, properties: any, value: any, errors?: any): string | readonly string[] {
    type = type.toLowerCase();

    if (type === 'custom' && Types.isArrayOfString(properties.errors)) {
        const backendError = localizer.get('common.backendError');

        return properties.errors.map((error: string) => StringHelper.appendLast(`${backendError}: ${error}`, '.'));
    }

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

    let message: string | null = properties['message'];

    if (!Types.isString(message) && errors) {
        message = errors[type];
    }

    if (!Types.isString(message)) {
        message = `validation.${type}`;
    }

    const args = { ...properties, field };

    message = localizer.getOrKey(message, args);

    return message;
}
