/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, Renderer2, SimpleChanges, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ApiUrlConfig, ResourceOwner, Types } from '@app/framework/internal';
import { AppsState, AuthService, ContentDto, SchemaDto } from '@app/shared';

@Component({
    selector: 'sqx-content-extension',
    styleUrls: ['./content-extension.component.scss'],
    templateUrl: './content-extension.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentExtensionComponent extends ResourceOwner implements AfterViewInit, OnChanges {
    private readonly context: any;
    private isInitialized = false;

    @Input()
    public url: string;

    @Input()
    public content: ContentDto;

    @Input()
    public contentSchema: SchemaDto;

    @ViewChild('iframe', { static: false })
    public iframe: ElementRef<HTMLIFrameElement>;

    constructor(apiUrl: ApiUrlConfig, authService: AuthService, appsState: AppsState,
        private readonly renderer: Renderer2,
        private readonly router: Router
    ) {
        super();

        this.context = {
            apiUrl: apiUrl.buildUrl('api'),
            appId: appsState.snapshot.selectedApp!.id,
            appName: appsState.snapshot.selectedApp!.name,
            user: authService.user
        };
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['url'] && this.iframe?.nativeElement) {
            this.iframe.nativeElement.src = this.url || '';
        }

        if (changes['contentSchema']) {
            this.context['schemaName'] = this.contentSchema?.name;
            this.context['schemaId'] = this.contentSchema?.id;
        }

        if (changes['content']) {
            this.sendContent();
        }
    }

    public ngAfterViewInit() {
        this.iframe.nativeElement.src = this.url || '';

        this.own(
            this.renderer.listen('window', 'message', (event: MessageEvent) => {
                if (event.source === this.iframe.nativeElement.contentWindow) {
                    const { type } = event.data;

                    if (type === 'started') {
                        this.isInitialized = true;

                        this.sendInit();
                        this.sendContent();
                    } else if (type === 'resize') {
                        const { height } = event.data;

                        this.iframe.nativeElement.height = `${height}px`;
                    } else if (type === 'navigate') {
                        const { url } = event.data;

                        this.router.navigateByUrl(url);
                    }
                }
            }));
    }

    private sendInit() {
        this.sendMessage('init', { context: this.context });
    }

    private sendContent() {
        this.sendMessage('contentChanged', { content: this.content });
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