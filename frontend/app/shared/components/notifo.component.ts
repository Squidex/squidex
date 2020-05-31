/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, OnChanges, OnDestroy, Output, SimpleChanges, ViewChild } from '@angular/core';
import { Pager, ResourceLoaderService, UIOptions } from '@app/framework';
import { AuthService } from '@app/shared/internal';

@Component({
    selector: 'sqx-notifo',
    styleUrls: ['./notifo.component.scss'],
    templateUrl: './notifo.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotifoComponent implements AfterViewInit, OnChanges, OnDestroy {
    private readonly notifoApiUrl: string;
    private readonly notifoApiKey: string;

    @Output()
    public pagerChange = new EventEmitter<Pager>();

    @Input()
    public topic: string;

    @ViewChild('element', { static: false })
    public element: ElementRef<Element>;

    constructor(resourceLoader: ResourceLoaderService, uiOptions: UIOptions,
        private readonly authService: AuthService
    ) {
        this.notifoApiKey = uiOptions.get('more.notifoKey');
        this.notifoApiUrl = uiOptions.get('more.notifoApi');

        if (this.notifoApiKey) {
            if (this.notifoApiUrl.indexOf('localhost:5002') >= 0) {
                resourceLoader.loadScript(`https://localhost:3002/notifo-sdk.js`);
            } else {
                resourceLoader.loadScript(`${this.notifoApiUrl}/build/notifo-sdk.js`);
            }
        }
    }

    public ngAfterViewInit() {
        if (this.notifoApiKey) {
            let notifo = window['notifo'];

            if (!notifo) {
                notifo = [];

                if (this.notifoApiUrl.indexOf('localhost:5002') >= 0) {
                    notifo.push(['set', 'style', 'https://localhost:3002/notifo-sdk.css']);
                }

                notifo.push(['set', 'api-url', this.notifoApiUrl]);
                notifo.push(['set', 'api-key', this.notifoApiKey]);
                notifo.push(['set', 'user-email', this.authService.user?.email]);
                notifo.push(['set', 'user-name', this.authService.user?.email]);
                notifo.push(['set', 'user-id', this.authService.user?.id]);
                notifo.push(['subscribe']);

                window['notifo'] = notifo;
            }

            const element = this.element?.nativeElement;

            if (!this.topic) {
                notifo.push(['show-notifications', element, { position: 'bottom-right' }]);
            } else {
                notifo.push(['show-topic', element, this.topic, { style: 'bell' }]);
            }
        }
    }

    public ngOnChanges(changes: SimpleChanges) {
        const notifo = window['notifo'];

        const element = this.element?.nativeElement;

        if (notifo && changes['topic'] && element) {
            notifo.push(['hide-topic', element]);
            notifo.push(['show-topic', element, this.topic, { style: 'heart' }]);
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