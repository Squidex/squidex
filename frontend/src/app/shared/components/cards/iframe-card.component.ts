/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, HostListener, Input, ViewChild } from '@angular/core';
import { ApiUrlConfig, Types } from '@app/framework';
import { AppDto, AuthService, TeamDto } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-iframe-card',
    styleUrls: ['./iframe-card.component.scss'],
    templateUrl: './iframe-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IFrameCardComponent implements AfterViewInit {
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

    constructor(apiUrl: ApiUrlConfig, authService: AuthService) {
        this.context = { apiUrl: apiUrl.buildUrl('api'), user: authService.user };
    }

    public ngAfterViewInit() {
        this.iframe.nativeElement.src = this.options?.src;
    }

    @HostListener('window:message', ['$event'])
    public onWindowMessage(event: MessageEvent) {
        if (event.source === this.iframe.nativeElement.contentWindow) {
            const { type } = event.data;

            if (type === 'started') {
                this.isInitialized = true;

                this.sendInit();
            }
        }
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
