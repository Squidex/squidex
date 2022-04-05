/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UIOptions } from './../configurations';
import { OnboardingService } from './onboarding.service';

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

    it('should instantiate', () => {
        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        expect(onboardingService).toBeDefined();
    });

    it('should show if value not in store', () => {
        localStore.set('squidex.onboarding.disable.feature-a1', '0');

        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        expect(onboardingService.shouldShow('feature-a2')).toBeTruthy();
    });

    it('should not show if value in store', () => {
        localStore.set('squidex.onboarding.disable.feature-b1', '1');

        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        expect(onboardingService.shouldShow('feature-b1')).toBeFalsy();
    });

    it('should not show if disabled', () => {
        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        onboardingService.disable('feature-c1');

        expect(onboardingService.shouldShow('feature-c1')).toBeFalsy();
    });

    it('should not show if all disabled', () => {
        const onboardingService = new OnboardingService(new UIOptions({}), <any>localStore);

        onboardingService.disableAll();

        expect(onboardingService.shouldShow('feature-d1')).toBeFalsy();
    });

    it('should not show if disabled by setting', () => {
        const onboardingService = new OnboardingService(new UIOptions({ hideOnboarding: true }), <any>localStore);

        expect(onboardingService.shouldShow('feature-d1')).toBeFalsy();
    });
});
