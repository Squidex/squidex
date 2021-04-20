/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ShortcutService } from './shortcut.service';

describe('ShortcutService', () => {
    it('should instantiate', () => {
        const shortcutService = new ShortcutService();

        expect(shortcutService).toBeDefined();
    });

    it('should raise event if triggered', () => {
        const shortcutService = new ShortcutService();

        let isTriggered = false;

        shortcutService.on('ctrl+a', () => { isTriggered = true; });
        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeTruthy();
    });

    it('should not raise event if triggered but unsubscribed', () => {
        const shortcutService = new ShortcutService();

        let isTriggered = false;

        shortcutService.on('ctrl+a', () => { isTriggered = true; });
        shortcutService.off('ctrl+a');
        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });
});
