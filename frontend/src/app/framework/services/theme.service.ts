/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DOCUMENT } from '@angular/common';
import { computed, inject, Injectable, OnDestroy, signal } from '@angular/core';
import { LocalStoreService } from './local-store.service';

export type ThemeMode = 'light' | 'dark' | 'system';

export type Theme = 'light' | 'dark';

const THEME_MODES: ReadonlyArray<ThemeMode> = ['light', 'dark', 'system'];

const THEME_KEY = 'squidex.theme';

@Injectable({
    providedIn: 'root',
})
export class ThemeService implements OnDestroy {
    private readonly document = inject(DOCUMENT);
    private readonly localStore = inject(LocalStoreService);
    private readonly systemQuery = this.document.defaultView?.matchMedia?.('(prefers-color-scheme: dark)');
    private readonly systemListener = () => this.update();
    private readonly state = signal<{ mode: ThemeMode; theme: Theme }>({ mode: 'system', theme: 'light' });

    public readonly mode = computed(() => {
        return this.state().mode;
    });

    public readonly theme = computed(() => {
        return this.state().theme;
    });

    constructor() {
        this.systemQuery?.addEventListener?.('change', this.systemListener);

        this.update(parseMode(this.localStore.get(THEME_KEY)));
    }

    public ngOnDestroy() {
        this.systemQuery?.removeEventListener?.('change', this.systemListener);
    }

    public getColor(name: string) {
        // Colors are defined in _colors.scss and exposed as CSS variables for the current theme.
        const style = this.document.defaultView?.getComputedStyle?.(this.document.documentElement);

        return style?.getPropertyValue(`--sqx-${name}`).trim() || '';
    }

    public setMode(mode: ThemeMode) {
        this.localStore.set(THEME_KEY, mode);

        this.update(mode);
    }

    private update(mode = this.state().mode) {
        const theme = mode === 'system' ? (this.systemQuery?.matches ? 'dark' : 'light') : mode;

        // Bootstrap and our own CSS variables are switched by this attribute.
        this.document.documentElement.setAttribute('data-bs-theme', theme);

        this.state.set({ mode, theme });
    }
}

function parseMode(value: string | null): ThemeMode {
    return THEME_MODES.includes(value as ThemeMode) ? value as ThemeMode : 'system';
}
