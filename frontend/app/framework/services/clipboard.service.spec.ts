/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ClipboardService } from './clipboard.service';

describe('ShortcutService', () => {
    it('should instantiate', () => {
        const clipboardService = new ClipboardService();

        expect(clipboardService).toBeDefined();
    });

    it('should return empty string if clipboard is empty', () => {
        const clipboardService = new ClipboardService();

        expect(clipboardService.selectText()).toBe('');
    });

    it('should get value from clipboard', () => {
        const clipboardService = new ClipboardService();

        clipboardService.setText('MyContent');

        expect(clipboardService.selectText()).toBe('MyContent');
    });

    it('should raise subject if setting text', () => {
        const clipboardService = new ClipboardService();

        let text = '';

        clipboardService.textChanges.subscribe(t => {
            text = t;
        });

        clipboardService.setText('MyContent');

        expect(text).toBe('MyContent');
    });
});
