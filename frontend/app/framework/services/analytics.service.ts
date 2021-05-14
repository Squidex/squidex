/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { UIOptions } from './../configurations';
import { Types } from './../utils/types';
import { ResourceLoaderService } from './resource-loader.service';

@Injectable()
export class AnalyticsService {
    private readonly gtag: any;
    private readonly analyticsId: string;

    constructor(
        private readonly uiOptions?: UIOptions,
        private readonly router?: Router,
        private readonly resourceLoader?: ResourceLoaderService,
    ) {
        window['dataLayer'] = window['dataLayer'] || [];

        // eslint-disable-next-line func-names
        this.gtag = function () {
            // eslint-disable-next-line prefer-rest-params
            window['dataLayer'].push(arguments);
        };

        if (this.uiOptions) {
            this.analyticsId = this.uiOptions.get('google.analyticsId');
        }

        this.configureGtag();
    }

    public trackEvent(category: string, action: string, label?: string, value?: number) {
        this.gtag('event', 'user-action', {
            event_category: category,
            event_action: action,
            event_label: label,
            value,
        });
    }

    private configureGtag() {
        if (this.analyticsId && this.router && this.resourceLoader && window.location.hostname !== 'localhost') {
            this.gtag('config', this.analyticsId, { anonymize_ip: true });

            this.router.events.pipe(
                filter(event => Types.is(event, NavigationEnd)))
                .subscribe(() => {
                    this.gtag('config', this.analyticsId, { page_path: window.location.pathname, anonymize_ip: true });
                });

            this.loadScript();
        }
    }

    private loadScript() {
        if (document.cookie.indexOf('ga-disable') < 0 && this.resourceLoader) {
            this.resourceLoader.loadScript(`https://www.googletagmanager.com/gtag/js?id=${this.analyticsId}`);
        }
    }
}
