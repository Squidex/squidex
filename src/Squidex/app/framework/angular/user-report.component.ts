/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';

import { ResourceLoaderService } from './../services/resource-loader.service';
import { UserReportConfig } from './../configurations';

@Component({
    selector: 'sqx-user-report',
    template: ''
})
export class UserReportComponent implements OnDestroy, OnInit {
    private loadingTimer: any;

    constructor(config: UserReportConfig,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        window['_urq'] = window['_urq'] || [];
        window['_urq'].push(['initSite', config.siteId]);
    }

    public ngOnDestroy() {
        clearTimeout(this.loadingTimer);
    }

    public ngOnInit() {
        this.loadingTimer =
            setTimeout(() => {
                this.resourceLoader.loadScript('https://cdn.userreport.com/userreport.js');
            }, 4000);
    }
}