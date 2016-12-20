/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import { UserReportConfig } from './../configurations';

@Component({
    selector: 'sqx-user-report',
    template: ''
})
export class UserReportComponent implements OnInit {
    constructor(config: UserReportConfig) {
        window['_urq'] = window['_urq'] || [];
        window['_urq'].push(['initSite', config.siteId]);
    }

    public ngOnInit() {
        setTimeout(() => {
            const url = 'https://cdn.userreport.com/userreport.js';

            const script = document.createElement('script');
            script.src = url;
            script.async = true;

            const node = document.getElementsByTagName('script')[0];

            node.parentNode.insertBefore(script, node);
        }, 4000);
    }
}