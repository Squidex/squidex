


/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';

import { LocalStoreService } from './local-store.service';

export const OnboardingServiceFactory = (localStore: LocalStoreService) => {
    return new OnboardingService(localStore);
};

@Injectable()
export class OnboardingService {
    constructor(
        private readonly localStore: LocalStoreService
    ) {
    }

    public disableAll() {
        this.disable('all');
    }

    public disable(key: string) {
        this.localStore.set(`squidex.onboarding.disable.${key}`, '1');
    }

    public shouldShow(key: string) {
        return this.shouldShowKey(key) && this.shouldShowKey('all');
    }

    private shouldShowKey(key: string) {
        return this.localStore.get(`squidex.onboarding.disable.${key}`) !== '1';
    }
}