/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FieldTree, ValidationError } from '@angular/forms/signals';
import { ErrorDto, Types } from '@app/framework/internal';

const SERVER_ERROR_KIND = 'custom';

export interface ServerValidationError extends ValidationError.WithOptionalField {
    readonly errors: ReadonlyArray<string>;
}

export function serverErrors(root: FieldTree<unknown>, model: any, error: ErrorDto): ServerValidationError[] {
    const details = error.details;
    if (!details || details.length === 0) {
        return [];
    }

    const result: ServerValidationError[] = [];

    const visit = (field: FieldTree<unknown> | undefined, value: any, path: string) => {
        if (!field) {
            return;
        }

        if (path) {
            const errors = getErrors(details, path);
            if (errors.length > 0) {
                result.push({ kind: SERVER_ERROR_KIND, errors, fieldTree: field });
            }
        }

        if (Types.isArray(value)) {
            value.forEach((item, i) => visit((field as any)[i], item, `${path}[${i + 1}]`));
        } else if (Types.isObject(value)) {
            for (const key of Object.keys(value)) {
                visit((field as any)[key], value[key], path ? `${path}.${key}` : key);
            }
        }
    };

    visit(root, model, '');
    return result;
}

function getErrors(details: ErrorDto['details'], path: string) {
    const errors: string[] = [];

    for (const detail of details) {
        for (const property of detail.properties) {
            if (!property.startsWith(path)) {
                continue;
            }

            const subProperty = property.substring(path.length);
            const first = subProperty[0];

            if (!first) {
                errors.push(detail.message);
                break;
            } else if (first === '[') {
                errors.push(`${subProperty}: ${detail.message}`);
                break;
            } else if (first === '.') {
                errors.push(`${subProperty.substring(1)}: ${detail.message}`);
                break;
            }
        }
    }

    return errors;
}
