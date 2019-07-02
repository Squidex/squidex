


/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

import { LocalStoreService } from './local-store.service';

import { UIOptions } from './../configurations';

export const OnboardingServiceFactory = (uiOptions: UIOptions, localStore: LocalStoreService) => {
    return new OnboardingService(uiOptions, localStore);
};

@Injectable()
export class OnboardingService {
    private readonly disabled: boolean;

    constructor(uiOptions: UIOptions,
        private readonly localStore: LocalStoreService
    ) {
        this.disabled = uiOptions.get('hideOnboarding');
    }

    public disableAll() {
        this.disable('all');
    }

    public disable(key: string) {
        this.localStore.set(this.getConfigKey(key), '1');
    }

    public shouldShow(key: string) {
        return !this.disabled && this.shouldShowKey(key) && this.shouldShowKey('all');
    }

    private shouldShowKey(key: string) {
        return this.localStore.get(this.getConfigKey(key)) !== '1';
    }

    private getConfigKey(key: string): string {
        return `squidex.onboarding.disable.${key}`;
    }
}