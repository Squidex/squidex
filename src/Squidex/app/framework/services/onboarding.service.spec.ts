/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { OnboardingService, OnboardingServiceFactory } from './../';

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
    const localStore = new LocalStoreMock();

    it('should instantiate from factory', () => {
        const onboardingService = OnboardingServiceFactory(<any>localStore);

        expect(onboardingService).toBeDefined();
    });

    it('should instantiate', () => {
        const onboardingService = new OnboardingService(<any>localStore);

        expect(onboardingService).toBeDefined();
    });

    it('should return true when value not in store', () => {
        localStore.set('squidex.onboarding.disable.feature1', '0');

        const onboardingService = new OnboardingService(<any>localStore);

        onboardingService.disable('feature2');

        expect(onboardingService.shouldShow('feature1')).toBeTruthy();
    });

    it('should return false when value in store', () => {
        localStore.set('squidex.onboarding.disable.feature1', '1');

        const onboardingService = new OnboardingService(<any>localStore);

        expect(onboardingService.shouldShow('feature1')).toBeFalsy();
    });

    it('should return false when disabled', () => {
        const onboardingService = new OnboardingService(<any>localStore);

        onboardingService.disable('feature1');

        expect(onboardingService.shouldShow('feature1')).toBeFalsy();
    });

    it('should return false when all disabled', () => {
        const onboardingService = new OnboardingService(<any>localStore);

        onboardingService.disableAll();

        expect(onboardingService.shouldShow('feature1')).toBeFalsy();
    });
});