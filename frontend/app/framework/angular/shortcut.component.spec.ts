/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgZone } from '@angular/core';
import { ShortcutService } from '@app/framework/internal';
import { ShortcutComponent } from './shortcut.component';

describe('ShortcutComponent', () => {
    const changeDetector: any = {
        detach: () => {
            return 0;
        },
    };

    let shortcutService: ShortcutService;

    beforeEach(() => {
        shortcutService = new ShortcutService();
    });

    it('should instantiate', () => {
        const shortcutComponent = new ShortcutComponent(changeDetector, shortcutService, new NgZone({}));

        expect(shortcutComponent).toBeDefined();
    });

    it('should init without keys', () => {
        const shortcutComponent = new ShortcutComponent(changeDetector, shortcutService, new NgZone({}));

        shortcutComponent.keys = null!;
        shortcutComponent.ngOnInit();

        expect().nothing();
    });

    it('should destroy without keys', () => {
        const shortcutComponent = new ShortcutComponent(changeDetector, shortcutService, new NgZone({}));

        shortcutComponent.keys = null!;
        shortcutComponent.ngOnDestroy();

        expect().nothing();
    });

    it('should raise event if triggered', () => {
        const shortcutComponent = new ShortcutComponent(changeDetector, shortcutService, new NgZone({}));

        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });

        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeTruthy();
    });

    it('should not raise event if triggered but disabled', () => {
        const shortcutComponent = new ShortcutComponent(changeDetector, shortcutService, new NgZone({}));

        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });
        shortcutComponent.disabled = true;

        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });

    it('should not raise event if triggered but destroyed', () => {
        const shortcutComponent = new ShortcutComponent(changeDetector, shortcutService, new NgZone({}));

        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });
        shortcutComponent.ngOnDestroy();

        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });
});
