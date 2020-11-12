/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ValidatorFn } from '@angular/forms';
import { ErrorDto, Types } from '@app/framework/internal';
import { getControlPath, getRawValue } from './forms-helper';

export class ErrorValidator {
    private values: { [path: string]: { value: any } } = {};
    private error: ErrorDto | undefined | null;

    public validator: ValidatorFn = control => {
        if (!this.error) {
            return null;
        }

        const path = getControlPath(control, true);

        if (!path) {
            return null;
        }

        const value = getRawValue(control);

        const current = this.values[path];

        if (current && !Types.equals(value, current.value, true)) {
            this.values[path] = { value };
            return null;
        }

        const errors: string[] = [];

        for (const details of this.error.details) {
            for (const property of details.properties) {
                if (property.startsWith(path)) {
                    const subProperty = property.substr(path.length);

                    const first = subProperty[0];

                    if (!first) {
                        errors.push(details.message);
                        break;
                    } else if (first === '[') {
                        errors.push(`${subProperty}: ${details.message}`);
                        break;
                    } else if (first === '.') {
                        errors.push(`${subProperty.substr(1)}: ${details.message}`);
                        break;
                    }
                }
            }
        }

        this.values[path] = { value };

        if (errors.length > 0) {
            return {
                custom: {
                    errors
                }
            };
        }

        return null;
    }

    public setError(error: ErrorDto | undefined | null) {
        this.values = {};
        this.error = error;
    }
}