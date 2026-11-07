/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ShortcutService } from '@app/framework/internal';
import { ShortcutDirective } from './shortcut.directive';

@Component({
    template: '<button [disabled]="disabled()" (click)="isTriggered = true" [shortcut]="shortcut()" [shortcutAction]="shortcutAction()"></button>',
    imports: [
        ShortcutDirective,
    ],
})
class TestHostComponent {
    public readonly shortcut = input<string | null>(null);
    public readonly shortcutAction = input<'focus' | 'click' | 'none'>('click');
    public readonly disabled = input(false);
    public isTriggered = false;
}

describe('ShortcutDirective', () => {
    let shortcutService: ShortcutService;
    let shortcutFixture: ComponentFixture<TestHostComponent>;

    beforeEach(() => {
        shortcutService = new ShortcutService();

        TestBed.configureTestingModule({
            providers: [
                { provide: ShortcutService, useValue: shortcutService },
            ],
        });

        shortcutFixture = TestBed.createComponent(TestHostComponent);
    });

    it('should init and destroy without keys', () => {
        setInputs({ shortcut: null });

        shortcutFixture.destroy();
    });

    it('should raise event if triggered', () => {
        setInputs({ shortcut: 'ctrl+a', shortcutAction: 'click' });

        shortcutService.raise('ctrl+a');

        expect(shortcutFixture.componentInstance.isTriggered).toBeTruthy();
    });

    it('should not click if action is focus', () => {
        setInputs({ shortcut: 'ctrl+a', shortcutAction: 'focus' });

        shortcutService.raise('ctrl+a');

        expect(shortcutFixture.componentInstance.isTriggered).toBeFalsy();
    });

    it('should not raise event if action is none', () => {
        setInputs({ shortcut: 'ctrl+a', shortcutAction: 'none' });

        shortcutService.raise('ctrl+a');

        expect(shortcutFixture.componentInstance.isTriggered).toBeFalsy();
    });

    it('should not raise event if triggered but disabled', () => {
        setInputs({ shortcut: 'ctrl+a', shortcutAction: 'click', disabled: true });

        shortcutService.raise('ctrl+a');

        expect(shortcutFixture.componentInstance.isTriggered).toBeFalsy();
    });

    it('should not raise event if triggered but destroyed', () => {
        setInputs({ shortcut: 'ctrl+a', shortcutAction: 'click' });
        shortcutFixture.destroy();

        shortcutService.raise('ctrl+a');

        expect(shortcutFixture.componentInstance.isTriggered).toBeFalsy();
    });

    it('should listen to new keys if keys changed', () => {
        setInputs({ shortcut: 'ctrl+a' });
        setInputs({ shortcut: 'ctrl+b' });

        shortcutService.raise('ctrl+a');

        expect(shortcutFixture.componentInstance.isTriggered).toBeFalsy();

        shortcutService.raise('ctrl+b');

        expect(shortcutFixture.componentInstance.isTriggered).toBeTruthy();
    });

    function setInputs(inputs: { shortcut: string | null; shortcutAction?: string; disabled?: boolean }) {
        for (const [key, value] of Object.entries(inputs)) {
            shortcutFixture.componentRef.setInput(key, value);
        }

        shortcutFixture.detectChanges();
    }
});
