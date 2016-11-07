/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { TitleService } from 'shared';

@Ng2.Component({
    selector: 'sqx-dashboard-page',
    template
})
export class DashboardComponent implements Ng2.OnInit {
    constructor(
        private readonly titles: TitleService,
        private readonly route: Ng2Router.ActivatedRoute
    ) {
    }

    public ngOnInit() {
        const appName = this.route.snapshot.params['appName'];

        this.titles.setTitle('{appName} | Dashboard', { appName: appName });
    }
}

