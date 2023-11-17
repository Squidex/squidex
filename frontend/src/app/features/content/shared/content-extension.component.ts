/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, HostListener, Input, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ApiUrlConfig, AppsState, AuthService, computeEditorUrl, ContentDto, SafeResourceUrlPipe, SchemaDto, TypedSimpleChanges, Types } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-content-extension',
    styleUrls: ['./content-extension.component.scss'],
    templateUrl: './content-extension.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        SafeResourceUrlPipe,
    ],
})
export class ContentExtensionComponent {
    private readonly context: any;
    private isInitialized = false;

    @ViewChild('iframe', { static: false })
    public iframe!: ElementRef<HTMLIFrameElement>;

    @Input({ required: true })
    public contentItem?: ContentDto | null;

    @Input({ required: true })
    public contentSchema!: SchemaDto;

    @Input({ transform: booleanAttribute })
    public scrollable?: boolean = false;

    @Input()
    public set editorUrl(value: string | undefined | null) {
        this.computedUrl = computeEditorUrl(value, this.appsState.snapshot.selectedSettings);
    }

    public computedUrl = '';

    constructor(apiUrl: ApiUrlConfig, authService: AuthService,
        private readonly appsState: AppsState,
        private readonly router: Router,
    ) {
        this.context = {
            apiUrl: apiUrl.buildUrl('api'),
            appId: appsState.snapshot.selectedApp!.id,
            appName: appsState.snapshot.selectedApp!.name,
            user: authService.user,
        };
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.contentSchema) {
            this.context['schemaName'] = this.contentSchema?.name;
            this.context['schemaId'] = this.contentSchema?.id;
        }

        if (changes.contentItem) {
            this.sendContent();
        }
    }

    @HostListener('window:message', ['$event'])
    public onWindowMessage(event: MessageEvent) {
        if (event.source === this.iframe.nativeElement.contentWindow) {
            const { type } = event.data;

            if (type === 'started') {
                this.isInitialized = true;

                this.sendInit();
                this.sendContent();
            } else if (type === 'resize' && !this.scrollable) {
                const { height } = event.data;

                this.iframe.nativeElement.height = `${height}px`;
            } else if (type === 'navigate') {
                const { url } = event.data;

                this.router.navigateByUrl(url);
            }
        }
    }

    private sendInit() {
        this.sendMessage('init', { context: this.context });
    }

    private sendContent() {
        this.sendMessage('contentChanged', { content: this.contentItem });
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
