/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DOCUMENT } from '@angular/common';
import { TestBed } from '@angular/core/testing';
import { LocalStoreService } from './local-store.service';
import { Theme, ThemeService } from './theme.service';

describe('ThemeService', () => {
    let listeners: (() => void)[];
    let systemDark: boolean;
    let localStore: LocalStoreService;
    let documentElement: HTMLElement;

    beforeEach(() => {
        listeners = [];
        systemDark = false;

        documentElement = document.createElement('html');

        const fakeDocument = {
            documentElement,
            defaultView: {
                getComputedStyle: () => ({
                    getPropertyValue: (name: string) => name === '--sqx-progress' ? ' #3d7dd5 ' : '',
                }),
                matchMedia: () => ({
                    get matches() {
                        return systemDark;
                    },
                    addEventListener: (_: string, listener: () => void) => listeners.push(listener),
                    removeEventListener: () => {},
                }),
            },
        };

        localStore = new LocalStoreService();
        localStore.configureStore(createStore());

        TestBed.configureTestingModule({
            providers: [
                { provide: DOCUMENT, useValue: fakeDocument },
                { provide: LocalStoreService, useValue: localStore },
            ],
        });
    });

    it('should use light theme from system by default', () => {
        const themeService = TestBed.inject(ThemeService);

        expect(themeService.mode).toEqual('system');
        expect(themeService.theme).toEqual('light');
        expect(documentElement.getAttribute('data-bs-theme')).toEqual('light');
    });

    it('should use dark theme from system', () => {
        systemDark = true;

        const themeService = TestBed.inject(ThemeService);

        expect(themeService.mode).toEqual('system');
        expect(themeService.theme).toEqual('dark');
        expect(documentElement.getAttribute('data-bs-theme')).toEqual('dark');
    });

    it('should restore mode from local store', () => {
        localStore.set('squidex.theme', 'dark');

        const themeService = TestBed.inject(ThemeService);

        expect(themeService.mode).toEqual('dark');
        expect(themeService.theme).toEqual('dark');
    });

    it('should ignore invalid mode from local store', () => {
        localStore.set('squidex.theme', 'invalid');

        const themeService = TestBed.inject(ThemeService);

        expect(themeService.mode).toEqual('system');
    });

    it('should store and apply mode', () => {
        const themeService = TestBed.inject(ThemeService);

        themeService.setMode('dark');

        expect(localStore.get('squidex.theme')).toEqual('dark');
        expect(themeService.theme).toEqual('dark');
        expect(documentElement.getAttribute('data-bs-theme')).toEqual('dark');
    });

    it('should get color from CSS variable', () => {
        const themeService = TestBed.inject(ThemeService);

        expect(themeService.getColor('progress')).toEqual('#3d7dd5');
        expect(themeService.getColor('unknown')).toEqual('');
    });

    it('should follow system changes if mode is system', () => {
        const themeService = TestBed.inject(ThemeService);

        const themes: Theme[] = [];

        themeService.themeChanges.subscribe(theme => {
            themes.push(theme);
        });

        systemDark = true;
        listeners.forEach(x => x());

        expect(themes).toEqual(['light', 'dark']);
    });

    it('should not follow system changes if mode is explicit', () => {
        const themeService = TestBed.inject(ThemeService);

        themeService.setMode('light');

        systemDark = true;
        listeners.forEach(x => x());

        expect(themeService.theme).toEqual('light');
    });
});

function createStore() {
    const values: Record<string, string> = {};

    return {
        getItem: (key: string) => values[key] ?? null,
        setItem: (key: string, value: string) => {
            values[key] = value;
        },
    };
}
