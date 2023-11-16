/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, inject, Input, OnDestroy, Renderer2, ViewChild } from '@angular/core';
import { ResourceLoaderService, TypedSimpleChanges, UIOptions } from '@app/framework';
import { AuthService } from '@app/shared/internal';
import { TourHintDirective } from './tour-hint.directive';

@Component({
    standalone: true,
    selector: 'sqx-notifo',
    styleUrls: ['./notifo.component.scss'],
    templateUrl: './notifo.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TourHintDirective,
    ],
})
export class NotifoComponent implements AfterViewInit, OnDestroy {
    private readonly notifoApiUrl: string = inject(UIOptions).value.notifoApi;
    private readonly notifoApiKey: string | undefined;

    @Input()
    public topic = '';

    @Input()
    public position?: 'bottom-left' | 'bottom-right' = 'bottom-left';

    @ViewChild('element', { static: false })
    public element!: ElementRef<Element>;

    public get isConfigured() {
        return !!this.notifoApiKey && !!this.notifoApiUrl;
    }

    public get showOnboarding() {
        return !!this.notifoApiUrl && !!this.topic;
    }

    constructor(resourceLoader: ResourceLoaderService, authService: AuthService,
        private readonly renderer: Renderer2,
    ) {
        this.notifoApiKey = authService.user?.notifoToken;

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
            let notifo = (window as any)['notifo'];

            if (!notifo) {
                notifo = [];

                const options: any = {
                    apiUrl: this.notifoApiUrl,
                    userKey: null,
                    userToken: this.notifoApiKey,
                    linkTarget: '_blank',
                };

                if (this.notifoApiUrl.includes('localhost:5002')) {
                    options.styleUrl = 'https://localhost:3002/notifo-sdk.css';
                }

                notifo.push(['init', options]);
                notifo.push(['subscribe']);

                (window as any)['notifo'] = notifo;
            }

            const element = this.element?.nativeElement;

            if (!this.topic) {
                notifo.push(['show-notifications', element, { position: this.position, style: 'notifo' }]);
            } else {
                notifo.push(['show-topic', element, this.topic, { position: this.position, style: 'bell' }]);
            }

            if (element) {
                this.renderer.addClass(element, 'notifo-container');
            }
        }
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        const notifo = (window as any)['notifo'];

        const element = this.element?.nativeElement;

        if (notifo && changes.topic && element) {
            notifo.push(['hide-topic', element]);
            notifo.push(['show-topic', element, this.topic, { style: 'bell' }]);
        }
    }

    public ngOnDestroy() {
        const notifo = (window as any)['notifo'];

        const element = this.element?.nativeElement;

        if (notifo && this.topic && element) {
            notifo.push(['hide-topic', element]);
        }
    }
}
