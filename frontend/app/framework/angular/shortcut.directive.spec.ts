/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ElementRef } from '@angular/core';
import { ShortcutService } from '@app/framework/internal';
import { ShortcutDirective } from './shortcut.directive';

describe('ShortcutDirective', () => {
    let shortcutService: ShortcutService;
    let shortcutElement: object;
    let shortcutDirective: ShortcutDirective;

    beforeEach(() => {
        shortcutElement = {};
        shortcutService = new ShortcutService();
        shortcutDirective = new ShortcutDirective(new ElementRef(shortcutElement), shortcutService);
    });

    it('should init without keys', () => {
        shortcutDirective.shortcut = null!;
        shortcutDirective.ngOnInit();

        expect().nothing();
    });

    it('should destroy without keys', () => {
        shortcutDirective.shortcut = null!;
        shortcutDirective.ngOnDestroy();

        expect().nothing();
    });

    it('should raise event if triggered', () => {
        let isTriggered = false;

        shortcutElement['click'] = () => {
            isTriggered = true;
        };

        shortcutDirective.shortcut = 'ctrl+a';
        shortcutDirective.shortcutAction = 'click';
        shortcutDirective.ngOnInit();

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeTruthy();
    });

    it('should raise event if function does not exist', () => {
        let isTriggered = false;

        shortcutElement['click'] = () => {
            isTriggered = true;
        };

        shortcutDirective.shortcut = 'ctrl+a';
        shortcutDirective.shortcutAction = 'focus';
        shortcutDirective.ngOnInit();

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });

    it('should not raise event if triggered but disabled', () => {
        let isTriggered = false;

        shortcutElement['disabled'] = true;
        shortcutElement['click'] = () => {
            isTriggered = true;
        };

        shortcutDirective.shortcut = 'ctrl+a';
        shortcutDirective.shortcutAction = 'click';
        shortcutDirective.ngOnInit();

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });

    it('should not raise event if triggered but destroyed', () => {
        let isTriggered = false;

        shortcutElement['click'] = () => {
            isTriggered = true;
        };

        shortcutDirective.shortcut = 'ctrl+a';
        shortcutDirective.shortcutAction = 'click';
        shortcutDirective.ngOnInit();
        shortcutDirective.ngOnDestroy();

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });
});
