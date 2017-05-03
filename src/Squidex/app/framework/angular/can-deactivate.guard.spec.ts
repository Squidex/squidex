/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { CanDeactivateGuard } from './can-deactivate.guard';

describe('CanDeactivateGuard', () => {
    it('should call component', () => {
        let called = false;

        const component = {
            canDeactivate: () => {
                called = true;

                return true;
            }
        };

        const result = new CanDeactivateGuard().canDeactivate(component);

        expect(result).toBeTruthy();
        expect(called).toBeTruthy();
    });
});
