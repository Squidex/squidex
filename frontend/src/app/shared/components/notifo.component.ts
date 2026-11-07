/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, ChangeDetectionStrategy, Component, DestroyRef, effect, ElementRef, inject, input, Renderer2, viewChild } from '@angular/core';
import { ResourceLoaderService, UIOptions } from '@app/framework';
import { AuthService } from '@app/shared/internal';
import { TourHintDirective } from './tour-hint.directive';

@Component({
    selector: 'sqx-notifo',
    styleUrls: ['./notifo.component.scss'],
    templateUrl: './notifo.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TourHintDirective,
    ],
})
export class NotifoComponent {
    private readonly authService = inject(AuthService);
    private readonly uiOptions = inject(UIOptions);
    private readonly renderer = inject(Renderer2);
    private readonly notifoApiUrl: string = this.uiOptions.value.notifoApi;
    private readonly notifoApiKey = this.authService.user?.notifoToken;
    private readonly element = viewChild<ElementRef<Element>>('element');
    private topicShown = '';
    private isShown = false;

    /** The topic to subscribe to. Shows all notifications when not defined. */
    public readonly topic = input('');

    /** The position of the notifications popup. */
    public readonly position = input<'bottom-left' | 'bottom-right'>('bottom-left');

    constructor() {
        const resourceLoader = inject(ResourceLoaderService);
        const destroyRef = inject(DestroyRef);

        if (!this.notifoApiKey || !this.notifoApiUrl) {
            return;
        }

        if (this.notifoApiUrl.indexOf('localhost:5002') >= 0) {
            resourceLoader.loadScript('https://localhost:3002/notifo-sdk.js');
        } else {
            resourceLoader.loadScript(`${this.notifoApiUrl}/build/notifo-sdk.js`);
        }

        afterNextRender(() => {
            this.show();
        });

        effect(() => {
            const topic = this.topic();
            if (!this.isShown || topic === this.topicShown) {
                return;
            }

            const element = this.element()?.nativeElement;
            if (!element) {
                return;
            }

            const notifo = getNotifo();
            if (!notifo) {
                return;
            }

            notifo.push(['hide-topic', element]);
            notifo.push(['show-topic', element, topic, { style: 'bell' }]);

            this.topicShown = topic;
        });

        destroyRef.onDestroy(() => {
            const element = this.element()?.nativeElement;
            if (!element || !this.topicShown) {
                return;
            }

            getNotifo()?.push(['hide-topic', element]);
        });
    }

    private show() {
        let notifo = getNotifo();

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

        const element = this.element()?.nativeElement;
        const topic = this.topic();
        const position = this.position();

        if (!topic) {
            notifo.push(['show-notifications', element, { position, style: 'notifo' }]);
        } else {
            notifo.push(['show-topic', element, topic, { position, style: 'bell' }]);
        }

        this.topicShown = topic;
        this.isShown = true;

        if (element) {
            this.renderer.addClass(element, 'notifo-container');
        }
    }
}

function getNotifo(): any[] | undefined {
    return (window as any)['notifo'];
}
