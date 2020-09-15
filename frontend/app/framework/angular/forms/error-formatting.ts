/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { Types } from '@app/framework/internal';
import { LocalizerService } from '@app/shared';

export function formatError(localizer: LocalizerService, field: string, type: string, properties: any, value: any, errors?: any)  {
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