/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, ElementRef, inject, input, viewChild } from '@angular/core';
import { ApiUrlConfig, ThemeService, Types } from '@app/framework';
import { AppDto, AuthService, TeamDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-iframe-card',
    styleUrls: ['./iframe-card.component.scss'],
    templateUrl: './iframe-card.component.html',
    host: {
        '(window:message)': 'onWindowMessage($event)',
    },
})
export class IFrameCardComponent {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly authService = inject(AuthService);
    private readonly themeService = inject(ThemeService);
    private isInitialized = false;

    /** The card configuration, with the name and the URL of the page to embed. */
    public readonly options = input<any>();

    /** The team of the dashboard, passed to the embedded page. */
    public readonly team = input<TeamDto | null>();

    /** The app of the dashboard, passed to the embedded page. */
    public readonly app = input<AppDto | null>();

    protected readonly iframe = viewChild<ElementRef<HTMLIFrameElement>>('iframe');

    protected readonly context = computed(() => {
        const team = this.team();
        const app = this.app();

        return {
            apiUrl: this.apiUrl.buildUrl('api'),
            user: this.authService.user,
            teamId: team?.id,
            teamName: team?.name,
            appId: app?.id,
            appName: app?.name,
        };
    });

    constructor() {
        effect(() => {
            const iframe = this.iframe();
            if (iframe) {
                iframe.nativeElement.src = this.options()?.src;
            }
        });

        effect(() => {
            this.sendMessage('themeChanged', { theme: this.themeService.theme() });
        });
    }

    protected onWindowMessage(event: MessageEvent) {
        if (event.source !== this.iframe()?.nativeElement.contentWindow) {
            return;
        }

        if (event.data?.type === 'started') {
            this.isInitialized = true;

            this.sendTheme();
            this.sendInit();
        }
    }

    private sendTheme() {
        this.sendMessage('themeChanged', { theme: this.themeService.theme() });
    }

    private sendInit() {
        this.sendMessage('init', { context: this.context() });
    }

    private sendMessage(type: string, payload: any) {
        if (!this.isInitialized) {
            return;
        }

        const target = this.iframe()?.nativeElement.contentWindow;
        if (!target || !Types.isFunction(target.postMessage)) {
            return;
        }

        target.postMessage({ type, ...payload }, '*');
    }
}
