/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, ElementRef, input, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ApiUrlConfig, AppsState, AuthService, computeEditorUrl, ContentDto, SafeResourceUrlPipe, SchemaDto, ThemeService, Types } from '@app/shared';

@Component({
    selector: 'sqx-content-extension',
    styleUrls: ['./content-extension.component.scss'],
    templateUrl: './content-extension.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        SafeResourceUrlPipe,
    ],
    host: {
        '(window:message)': 'onWindowMessage($event)',
    },
})
export class ContentExtensionComponent {
    private readonly iframe = viewChild<ElementRef<HTMLIFrameElement>>('iframe');
    private isInitialized = false;

    /** The content that is passed to the extension. */
    public readonly contentItem = input.required<ContentDto | null | undefined>();

    /** The schema of the content. */
    public readonly contentSchema = input.required<SchemaDto>();

    /** Lets the iframe scroll instead of resizing it to the height of the extension. */
    public readonly scrollable = input(false, { transform: booleanAttribute });

    /** The URL of the extension. */
    public readonly editorUrl = input<string | null>();

    protected readonly computedUrl = computed(() => {
        return computeEditorUrl(this.editorUrl(), this.appsState.snapshot.selectedSettings);
    });

    private readonly context = computed(() => {
        const schema = this.contentSchema();

        return {
            apiUrl: this.apiUrl.buildUrl('api'),
            appId: this.appsState.snapshot.selectedApp!.id,
            appName: this.appsState.snapshot.selectedApp!.name,
            user: this.authService.user,
            schemaName: schema?.name,
            schemaId: schema?.id,
        };
    });

    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly appsState: AppsState,
        private readonly authService: AuthService,
        private readonly router: Router,
        private readonly themeService: ThemeService,
    ) {
        effect(() => {
            this.sendMessage('themeChanged', { theme: themeService.theme() });
        });

        effect(() => {
            this.sendContent();
        });
    }

    protected onWindowMessage(event: MessageEvent) {
        const iframe = this.iframe()?.nativeElement;
        if (!iframe || event.source !== iframe.contentWindow) {
            return;
        }

        const { type } = event.data;

        if (type === 'started') {
            this.isInitialized = true;

            this.sendTheme();
            this.sendInit();
            this.sendContent();
        } else if (type === 'resize' && !this.scrollable()) {
            const { height } = event.data;

            iframe.height = `${height}px`;
        } else if (type === 'navigate') {
            const { url } = event.data;

            this.router.navigateByUrl(url);
        }
    }

    private sendInit() {
        this.sendMessage('init', { context: this.context() });
    }

    private sendTheme() {
        this.sendMessage('themeChanged', { theme: this.themeService.theme() });
    }

    private sendContent() {
        this.sendMessage('contentChanged', { content: this.contentItem() });
    }

    private sendMessage(type: string, payload: any) {
        const iframe = this.iframe()?.nativeElement;
        if (!iframe) {
            return;
        }

        if (this.isInitialized && iframe.contentWindow && Types.isFunction(iframe.contentWindow.postMessage)) {
            const message = { type, ...payload };

            iframe.contentWindow.postMessage(message, '*');
        }
    }
}
