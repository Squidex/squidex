/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ShortcutService } from '@app/framework/internal';
import { ShortcutComponent } from './shortcut.component';

describe('ShortcutComponent', () => {
    let shortcutService: ShortcutService;
    let shortcutFixture: ComponentFixture<ShortcutComponent>;
    let isTriggered = false;

    beforeEach(() => {
        shortcutService = new ShortcutService();

        TestBed.configureTestingModule({
            providers: [
                { provide: ShortcutService, useValue: shortcutService },
            ],
        });

        shortcutFixture = TestBed.createComponent(ShortcutComponent);
        shortcutFixture.componentInstance.trigger.subscribe(() => { isTriggered = true; });

        isTriggered = false;
    });

    it('should init and destroy without keys', () => {
        shortcutFixture.detectChanges();
        shortcutFixture.destroy();
    });

    it('should raise event if triggered', () => {
        setInputs({ keys: 'ctrl+a' });

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeTruthy();
    });

    it('should not raise event if triggered but disabled', () => {
        setInputs({ keys: 'ctrl+a', disabled: true });

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });

    it('should not raise event if triggered but destroyed', () => {
        setInputs({ keys: 'ctrl+a' });
        shortcutFixture.destroy();

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();
    });

    it('should listen to new keys if keys changed', () => {
        setInputs({ keys: 'ctrl+a' });
        setInputs({ keys: 'ctrl+b' });

        shortcutService.raise('ctrl+a');

        expect(isTriggered).toBeFalsy();

        shortcutService.raise('ctrl+b');

        expect(isTriggered).toBeTruthy();
    });

    function setInputs(inputs: { keys: string; disabled?: boolean }) {
        for (const [key, value] of Object.entries(inputs)) {
            shortcutFixture.componentRef.setInput(key, value);
        }

        shortcutFixture.detectChanges();
    }
});
