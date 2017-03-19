/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class Version {
    public get value() {
        return this.currentValue;
    }

    constructor(
        private currentValue: string = ''
    ) {
    }

    public update(newValue: string) {
        this.currentValue = newValue;
    }
}