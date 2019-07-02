/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class UIOptions {
    constructor(
        private readonly value: any
    ) {
    }

    public get(path: string) {
        if (!path) {
            return undefined;
        }

        let value = this.value;

        if (value) {
            const parts = path.split('.');

            for (let part of parts) {
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
            value = value + '/';
        }

        this.value = value;
    }

    public buildUrl(path: string) {
        if (path.indexOf('/') === 0) {
            path = path.substr(1);
        }

        return this.value + path;
    }
}

export class CurrencyConfig {
    constructor(
        public readonly code: string,
        public readonly symbol: string,
        public readonly showAfter: boolean = true
    ) {
    }
}

export class AnalyticsIdConfig {
    constructor(public value: string) {}
}

export class DecimalSeparatorConfig {
    constructor(public readonly value: string) {}
}

export class ProductionModeConfig {
    constructor(public readonly isProductionMode: boolean) {}
}