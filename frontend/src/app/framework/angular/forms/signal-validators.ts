/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FieldTree, PathKind, pattern, SchemaPath, validate, ValidationError } from '@angular/forms/signals';
import { DateTime, LocalizerService, Types } from '@app/framework/internal';
import { formatError } from './error-formatting';

export namespace SignalValidators {
    export function patternWithMessage<TKind extends PathKind>(path: SchemaPath<string, any, TKind>, regex: string, message?: string) {
        pattern(path, new RegExp(`^(?:${regex})$`), message ? { message } : undefined);
    }

    export function between<TKind extends PathKind>(path: SchemaPath<any, any, TKind>, min: number, max: number) {
        validate(path, ({ value }) => {
            const raw = value();
            if (raw === null || raw === undefined || raw === '') {
                return undefined;
            }

            if (Number.isNaN(raw)) {
                return undefined;
            }

            const actual = parseFloat(raw);
            if (min === max) {
                if (Number.isNaN(actual) || actual !== min) {
                    return { kind: 'exactly', expected: min, actual } as ValidationError;
                }

                return undefined;
            }

            if (Number.isNaN(actual) || actual < min || actual > max) {
                return { kind: 'between', min, max, actual } as ValidationError;
            }

            return undefined;
        });
    }

    export function validDateTime<TKind extends PathKind>(path: SchemaPath<string | null | undefined, any, TKind>) {
        validate(path, ({ value }) => {
            const raw = value();
            if (!raw) {
                return undefined;
            }

            try {
                DateTime.parseISO(raw);
                return undefined;
            } catch {
                return { kind: 'validdatetime' };
            }
        });
    }

    export function match<TValue, TKind extends PathKind>(path: SchemaPath<TValue, any, TKind>, other: SchemaPath<TValue, any, any>, message: string) {
        validate(path, ({ value, valueOf }) => {
            if (value() !== valueOf(other)) {
                return { kind: 'match', message };
            }

            return undefined;
        });
    }
}

export function formatSignalErrors(localizer: LocalizerService, fieldName: string, field: FieldTree<unknown>) {
    const state = field();
    const value = state.value();
    const messages: string[] = [];

    for (const error of state.errors()) {
        const [type, properties] = getLegacyError(error);
        const message = formatError(localizer, fieldName, type, properties, value);

        if (Types.isString(message)) {
            messages.push(message);
        } else if (Types.isArray(message)) {
            messages.push(...message);
        }
    }

    return messages;
}

function getLegacyError(error: ValidationError & Record<string, any>): [string, any] {
    switch (error.kind) {
        case 'maxLength':
            return ['maxlength', { requiredLength: error['maxLength'], message: error.message }];
        case 'minLength':
            return ['minlength', { requiredLength: error['minLength'], message: error.message }];
        default:
            return [error.kind, error];
    }
}

export function getFieldKey(field: FieldTree<unknown>): string {
    try {
        return `${field().keyInParent()}`;
    } catch {
        return '';
    }
}
