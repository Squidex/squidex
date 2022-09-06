/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class UIOptions {
    constructor(
        public readonly value: any,
    ) {
    }

    public get(path: string) {
        if (!path) {
            return undefined;
        }

        let value = this.value;

        if (value) {
            const parts = path.split('.');

            for (const part of parts) {
                value = value[part];

                if (!value) {
                    break;
                }
            }
        }

        return value;
    }
}

export class ApiUrlConfig {
    public readonly value: string;

    constructor(value: string) {
        if (value.indexOf('/', value.length - 1) < 0) {
            value = `${value}/`;
        }

        this.value = value;
    }

    public buildUrl(path: string) {
        if (path.indexOf('/') === 0) {
            path = path.substring(1);
        }

        return this.value + path;
    }
}
