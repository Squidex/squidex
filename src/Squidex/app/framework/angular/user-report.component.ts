/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';

import {
    ResourceLoaderService,
    ResourceOwner,
    UserReportConfig
} from '@app/framework/internal';

@Component({
    selector: 'sqx-user-report',
    template: ''
})
export class UserReportComponent extends ResourceOwner implements OnInit {
    constructor(changeDetector: ChangeDetectorRef,
        private readonly config: UserReportConfig,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        super();

        changeDetector.detach();
    }

    public ngOnInit() {
        window['_urq'] = window['_urq'] || [];
        window['_urq'].push(['initSite', this.config.siteId]);

        this.own(
            timer(4000).subscribe(() => {
                this.resourceLoader.loadScript('https://cdn.userreport.com/userreport.js');
            }));
    }
}