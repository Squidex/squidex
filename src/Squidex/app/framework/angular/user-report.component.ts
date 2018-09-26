/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';

import { UserReportConfig } from './../configurations';
import { ResourceLoaderService } from './../services/resource-loader.service';

@Component({
    selector: 'sqx-user-report',
    template: ''
})
export class UserReportComponent implements OnDestroy, OnInit {
    private loadingTimer: any;

    constructor(config: UserReportConfig, changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        changeDetector.detach();

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