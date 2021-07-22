/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, OnDestroy, Renderer2, SimpleChanges, ViewChild } from '@angular/core';
import { ResourceLoaderService, UIOptions } from '@app/framework';
import { AuthService } from '@app/shared/internal';

@Component({
    selector: 'sqx-notifo',
    styleUrls: ['./notifo.component.scss'],
    templateUrl: './notifo.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotifoComponent implements AfterViewInit, OnChanges, OnDestroy {
    private readonly notifoApiUrl: string;
    private readonly notifoApiKey: string | undefined;

    @Input()
    public topic: string;

    @ViewChild('element', { static: false })
    public element: ElementRef<Element>;

    public get isConfigured() {
        return !!this.notifoApiKey && !!this.notifoApiUrl;
    }

    public get showOnboarding() {
        return !!this.notifoApiUrl && !!this.topic;
    }

    constructor(resourceLoader: ResourceLoaderService, uiOptions: UIOptions, authService: AuthService,
        private readonly renderer: Renderer2,
    ) {
        this.notifoApiKey = authService.user?.notifoToken;
        this.notifoApiUrl = uiOptions.get('more.notifoApi');

        if (this.isConfigured) {
            if (this.notifoApiUrl.indexOf('localhost:5002') >= 0) {
                resourceLoader.loadScript('https://localhost:3002/notifo-sdk.js');
            } else {
                resourceLoader.loadScript(`${this.notifoApiUrl}/build/notifo-sdk.js`);
            }
        }
    }

    public ngAfterViewInit() {
        if (this.isConfigured) {
            let notifo = window['notifo'];

            if (!notifo) {
                notifo = [];

                const options: any = { apiUrl: this.notifoApiUrl, userToken: this.notifoApiKey };

                if (this.notifoApiUrl.indexOf('localhost:5002') >= 0) {
                    options.styleUrl = 'https://localhost:3002/notifo-sdk.css';
                }

                notifo.push(['init', options]);
                notifo.push(['subscribe']);

                window['notifo'] = notifo;
            }

            const element = this.element?.nativeElement;

            if (!this.topic) {
                notifo.push(['show-notifications', element, { position: 'bottom-right' }]);
            } else {
                notifo.push(['show-topic', element, this.topic, { style: 'bell', position: 'bottom-right' }]);
            }

            if (element) {
                this.renderer.addClass(element, 'notifo-container');
            }
        }
    }

    public ngOnChanges(changes: SimpleChanges) {
        const notifo = window['notifo'];

        const element = this.element?.nativeElement;

        if (notifo && changes['topic'] && element) {
            notifo.push(['hide-topic', element]);
            notifo.push(['show-topic', element, this.topic, { style: 'bell' }]);
        }
    }

    public ngOnDestroy() {
        const notifo = window['notifo'];

        const element = this.element?.nativeElement;

        if (notifo && this.topic && element) {
            notifo.push(['hide-topic', element]);
        }
    }
}
