/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import { ResourceLoaderService } from './../services/resource-loader.service';
import { UserReportConfig } from './../configurations';

@Component({
    selector: 'sqx-user-report',
    template: ''
})
export class UserReportComponent implements OnInit {
    constructor(config: UserReportConfig,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        window['_urq'] = window['_urq'] || [];
        window['_urq'].push(['initSite', config.siteId]);
    }

    public ngOnInit() {
        setTimeout(() => {
            this.resourceLoader.loadScript('https://cdn.userreport.com/userreport.js');
        }, 4000);
    }
}