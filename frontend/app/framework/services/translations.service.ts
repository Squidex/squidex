/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

export const TranslationsServiceFactory = () => {
    return new TranslationsService();
};

@Injectable()
export class TranslationsService {
    public get(key: string, args?: any) {
        return key;
    }
}