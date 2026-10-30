/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, HostListener, Input, ViewChild } from '@angular/core';
import { ApiUrlConfig, Subscriptions, ThemeService, Types } from '@app/framework';
import { AppDto, AuthService, isSafeFrameUrl, TeamDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-iframe-card',
    styleUrls: ['./iframe-card.component.scss'],
    templateUrl: './iframe-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IFrameCardComponent implements AfterViewInit {
    private readonly subscriptions = new Subscriptions();
    private readonly context: any;
    private isInitialized = false;

    @Input()
    public options: any;

    @ViewChild('iframe', { static: false })
    public iframe!: ElementRef<HTMLIFrameElement>;

    @Input()
    public set team(value: TeamDto | undefined | null) {
        if (value) {
            this.context.teamId = value.id;
            this.context.teamName = value.name;
        }
    }

    @Input()
    public set app(value: AppDto | undefined | null) {
        if (value) {
            this.context.appId = value.id;
            this.context.appName = value.name;
        }
    }

    constructor(apiUrl: ApiUrlConfig, authService: AuthService,
        private readonly themeService: ThemeService,
    ) {
        this.context = { apiUrl: apiUrl.buildUrl('api'), user: authService.user };

        this.subscriptions.add(
            this.themeService.themeChanges
                .subscribe(() => {
                    this.sendTheme();
                }));
    }

    public ngAfterViewInit() {
        // The source comes from shared app settings and is assigned to the DOM directly, therefore it is not sanitized by Angular.
        const src = this.options?.src;
        if (!isSafeFrameUrl(src)) {
            return;
        }

        this.iframe.nativeElement.src = src;
    }

    @HostListener('window:message', ['$event'])
    public onWindowMessage(event: MessageEvent) {
        if (event.source === this.iframe.nativeElement.contentWindow) {
            const { type } = event.data;

            if (type === 'started') {
                this.isInitialized = true;

                this.sendTheme();
                this.sendInit();
            }
        }
    }

    private sendTheme() {
        this.sendMessage('themeChanged', { theme: this.themeService.theme });
    }

    private sendInit() {
        this.sendMessage('init', { context: this.context });
    }

    private sendMessage(type: string, payload: any) {
        if (!this.iframe) {
            return;
        }

        const iframe = this.iframe.nativeElement;

        if (this.isInitialized && iframe.contentWindow && Types.isFunction(iframe.contentWindow.postMessage)) {
            const message = { type, ...payload };

            iframe.contentWindow.postMessage(message, '*');
        }
    }
}
