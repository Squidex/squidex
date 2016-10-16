/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { UserReportConfig } from './../configurations';

@Ng2.Component({
    selector: 'gp-user-report', 
    template: ''
})
export class UserReportComponent implements Ng2.OnInit {
    constructor(config: UserReportConfig, 
        private readonly renderer: Ng2.Renderer
    ) {
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