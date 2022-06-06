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

        let triggered = 0;

        shortcutService.listen('ctrl+a', () => { triggered++; });
        shortcutService.raise('ctrl+a');

        expect(triggered).toEqual(1);
    });

    it('should not raise event if triggered but unsubscribed', () => {
        const shortcutService = new ShortcutService();

        let triggered = 0;

        shortcutService.listen('ctrl+a', () => { triggered++; })();
        shortcutService.raise('ctrl+a');

        expect(triggered).toEqual(0);
    });

    it('should transform shortcut', () => {
        const shortcutService = new ShortcutService();

        let triggered = 0;

        shortcutService.listen('CTRL + A', () => { triggered++; });
        shortcutService.raise('ctrl+a');

        expect(triggered).toEqual(1);
    });

    it('should register multiple shortcuts', () => {
        const shortcutService = new ShortcutService();

        let triggered = 0;

        shortcutService.listen('ctrl+a,ctrl+b', () => { triggered++; });
        shortcutService.raise('ctrl+a');
        shortcutService.raise('ctrl+b');

        expect(triggered).toEqual(2);
    });
});
