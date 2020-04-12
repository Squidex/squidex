/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:only-arrow-functions

import { Injectable } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';

import { AnalyticsIdConfig } from './../configurations';
import { UIOptions } from './../configurations';
import { Types } from './../utils/types';
import { ResourceLoaderService } from './resource-loader.service';

export const AnalyticsServiceFactory = (uiOptions: UIOptions, router: Router, resourceLoader: ResourceLoaderService) => {
    return new AnalyticsService(uiOptions, router, resourceLoader);
};

@Injectable()
export class AnalyticsService {
    private readonly gtag: any;
    private readonly analyticsId: AnalyticsIdConfig;

    constructor(private readonly uiOptions: UIOptions,
        private readonly router?: Router,
        private readonly resourceLoader?: ResourceLoaderService
    ) {
        window['dataLayer'] = window['dataLayer'] || [];
        this.analyticsId = new AnalyticsIdConfig(this.uiOptions.get('google.analyticsId'));

        this.gtag = function () {
            window['dataLayer'].push(arguments);
        };
         this.configureGtag();
    }

    public trackEvent(category: string, action: string, label?: string, value?: number) {
        this.gtag('event', 'user-action', {
            event_category: category,
            event_action: action,
            event_label: label,
            value: value
        });
    }

    private configureGtag() {
        if (this.analyticsId && this.router && this.resourceLoader && window.location.hostname !== 'localhost' ) {

            this.gtag('config', this.analyticsId.value, { anonymize_ip: true });

            this.router.events.pipe(
                    filter(e => Types.is(e, NavigationEnd)))
                .subscribe(() => {
                    this.gtag('config', this.analyticsId.value, { page_path: window.location.pathname, anonymize_ip: true });
                });

            this.loadGoogletagmanagerScript();
        }
    }

    private loadGoogletagmanagerScript() {
        if (document.cookie.indexOf('ga-disable') < 0 && this.resourceLoader) {
            this.resourceLoader.loadScript(`https://www.googletagmanager.com/gtag/js?id=${this.analyticsId.value}`);
        }
    }
}