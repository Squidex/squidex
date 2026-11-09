/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ClipboardService } from './clipboard.service';

describe('ClipboardService', () => {
    it('should instantiate', () => {
        const clipboardService = new ClipboardService();

        expect(clipboardService).toBeDefined();
    });

    it('should return empty string if clipboard is empty', () => {
        const clipboardService = new ClipboardService();

        expect(clipboardService.text()).toBe('');
    });

    it('should get value from clipboard', () => {
        const clipboardService = new ClipboardService();

        clipboardService.setText('MyContent');

        expect(clipboardService.text()).toBe('MyContent');
    });
});
