/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ShortcutService, ShortcutServiceFactory } from './../';

describe('ShortcutService', () => {
    it('should instantiate from factory', () => {
        const shortcutService = ShortcutServiceFactory();

        expect(shortcutService).toBeDefined();
    });

    it('should instantiate', () => {
        const shortcutService = new ShortcutService();

        expect(shortcutService).toBeDefined();
    });

    it('should raise event when triggered', () => {
        const shortcutService = new ShortcutService();

        let isTriggered = false;

        shortcutService.on('ctrl+a', () => { isTriggered = true; });
        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeTruthy();
    });

    it('should not raise event when triggered but unsubscribed', () => {
        const shortcutService = new ShortcutService();

        let isTriggered = false;

        shortcutService.on('ctrl+a', () => { isTriggered = true; });
        shortcutService.off('ctrl+a');
        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });
});
