/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { OnboardingService, OnboardingServiceFactory } from './onboarding.service';

import { UIOptions } from './../configurations';

class LocalStoreMock {
    private store = {};

    public get(key: string) {
        return this.store[key];
    }

    public set(key: string, value: string) {
        this.store[key] = value;
    }
}

describe('OnboardingService', () => {
    let localStore: LocalStoreMock;

    beforeEach(() => {
        localStore = new LocalStoreMock();
    });

    it('should instantiate from factory', () => {
        const onboardingService = OnboardingServiceFactory(new UIOptions({}), <any>localStore);

        expect(onboardingService).toBeDefined();
    });

    it('should instantiate', () => {
        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        expect(onboardingService).toBeDefined();
    });

    it('should show when value not in store', () => {
        localStore.set('squidex.onboarding.disable.feature-a1', '0');

        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        expect(onboardingService.shouldShow('feature-a2')).toBeTruthy();
    });

    it('should not show when value in store', () => {
        localStore.set('squidex.onboarding.disable.feature-b1', '1');

        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        expect(onboardingService.shouldShow('feature-b1')).toBeFalsy();
    });

    it('should not show when disabled', () => {
        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        onboardingService.disable('feature-c1');

        expect(onboardingService.shouldShow('feature-c1')).toBeFalsy();
    });

    it('should not show when all disabled', () => {
        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        onboardingService.disableAll();

        expect(onboardingService.shouldShow('feature-d1')).toBeFalsy();
    });

    it('should not show when disabled by setting', () => {
        const onboardingService = new OnboardingService(new UIOptions({ hideOnboarding: true }), <any>localStore);

        expect(onboardingService.shouldShow('feature-d1')).toBeFalsy();
    });
});