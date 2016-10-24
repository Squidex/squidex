/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { ShortcutService } from './../';
import { ShortcutComponent } from './shortcut.component';

describe('ShortcutComponent', () => {
    let shortcutService: ShortcutService;

    beforeEach(() => {
        shortcutService = new ShortcutService();
    });

    it('should instantiate', () => {
        const shortcutComponent = new ShortcutComponent(shortcutService, new Ng2.NgZone({}));

        expect(shortcutComponent).toBeDefined();
    });

    it('should init without keys', () => {
        const shortcutComponent = new ShortcutComponent(shortcutService, new Ng2.NgZone({}));

        shortcutComponent.keys = null!;
        shortcutComponent.ngOnInit();
    });

    it('should destroy without keys', () => {
        const shortcutComponent = new ShortcutComponent(shortcutService, new Ng2.NgZone({}));

        shortcutComponent.keys = null!;
        shortcutComponent.ngOnDestroy();
    });

    it('should raise event when triggered', () => {
        const shortcutComponent = new ShortcutComponent(shortcutService, new Ng2.NgZone({}));

        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });

        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeTruthy();
    });

    it('should not raise event when triggered but disabled', () => {
        const shortcutComponent = new ShortcutComponent(shortcutService, new Ng2.NgZone({}));

        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });
        shortcutComponent.disabled = true;

        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });

    it('should not raise event when triggered but destroyed', () => {
        const shortcutComponent = new ShortcutComponent(shortcutService, new Ng2.NgZone({}));

        let isTriggered = false;

        shortcutComponent.keys = 'ctrl+a';
        shortcutComponent.ngOnInit();
        shortcutComponent.trigger.subscribe(() => { isTriggered = true; });
        shortcutComponent.ngOnDestroy();

        shortcutService.trigger('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });
});
