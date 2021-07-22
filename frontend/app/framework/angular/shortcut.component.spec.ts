/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ShortcutService } from '@app/framework/internal';
import { ShortcutComponent } from './shortcut.component';

describe('ShortcutComponent', () => {
    const changeDetector: any = {
        detach: () => {
            return 0;
        },
    };

    let shortcutService: ShortcutService;
    let shortcutComponent: ShortcutComponent;

    beforeEach(() => {
        shortcutService = new ShortcutService();
        shortcutComponent = new ShortcutComponent(changeDetector, shortcutService);
    });

    it('should init without keys', () => {
        shortcutComponent.keys = null!;
        shortcutComponent.ngOnInit();

        expect().nothing();
    });

    it('should destroy without keys', () => {
        shortcutComponent.keys = null!;
        shortcutComponent.ngOnDestroy();

        expect().nothing();
    });

    it('should raise event if triggered', () => {
        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeTruthy();
    });

    it('should not raise event if triggered but disabled', () => {
        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });
        shortcutComponent.disabled = true;

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });

    it('should not raise event if triggered but destroyed', () => {
        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });
        shortcutComponent.ngOnDestroy();

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });
});
