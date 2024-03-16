/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ValidatorFn } from '@angular/forms';
import { ErrorDto } from '@app/framework/internal';
import { getControlPath } from './forms-helper';

export class ErrorValidator {
    private errorsCache: { [path: string]: { value: any } } = {};
    private error: ErrorDto | undefined | null;

    public validator: ValidatorFn = control => {
        if (!this.error) {
            return null;
        }

        const path = getControlPath(control, true);

        if (!path) {
            return null;
        }

        const value = control.value;

        const current = this.errorsCache[path];

        if (current && current.value !== value) {
            this.errorsCache[path] = { value };
            return null;
        }

        const errors: string[] = [];

        if (this.error.details) {
            for (const details of this.error.details) {
                for (const property of details.properties) {
                    if (property.startsWith(path)) {
                        const subProperty = property.substring(path.length);

                        const first = subProperty[0];

                        if (!first) {
                            errors.push(details.message);
                            break;
                        } else if (first === '[') {
                            errors.push(`${subProperty}: ${details.message}`);
                            break;
                        } else if (first === '.') {
                            errors.push(`${subProperty.substring(1)}: ${details.message}`);
                            break;
                        }
                    }
                }
            }
        }

        if (errors.length > 0) {
            this.errorsCache[path] = { value };

            return {
                custom: {
                    errors,
                },
            };
        }

        return null;
    };

    public setError(error: ErrorDto | undefined | null) {
        this.errorsCache = {};
        this.error = error;
    }
}
