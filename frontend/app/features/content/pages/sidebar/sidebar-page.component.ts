/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Renderer2, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ApiUrlConfig, ResourceOwner, Types } from '@app/framework/internal';
import { AppsState, AuthService, ContentsState, SchemasState } from '@app/shared';
import { combineLatest } from 'rxjs';

@Component({
    selector: 'sqx-sidebar-page',
    styleUrls: ['./sidebar-page.component.scss'],
    templateUrl: './sidebar-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarPageComponent extends ResourceOwner implements AfterViewInit {
    private readonly context: any;
    private content: any;
    private isInitialized = false;

    @ViewChild('iframe', { static: false })
    public iframe: ElementRef<HTMLIFrameElement>;

    constructor(apiUrl: ApiUrlConfig, authService: AuthService, appsState: AppsState,
        private readonly contentsState: ContentsState,
        private readonly schemasState: SchemasState,
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

    public ngAfterViewInit() {
        this.own(
            combineLatest([
                this.schemasState.selectedSchema,
                this.contentsState.selectedContent
            ]).subscribe(([schema, content]) => {
                const url =
                    content ?
                    schema.properties.contentSidebarUrl :
                    schema.properties.contentsSidebarUrl;

                this.context['schemaName'] = schema.name;
                this.context['schemaId'] = schema.id;

                this.iframe.nativeElement.src = url || '';
            }));

        this.own(
            this.contentsState.selectedContent
                .subscribe(content => {
                    this.content = content;

                    this.sendContent();
                }));

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

                        this.iframe.nativeElement.height = height + 'px';
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