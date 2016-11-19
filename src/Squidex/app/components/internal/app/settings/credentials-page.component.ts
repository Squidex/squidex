/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { AppsStoreService, TitleService } from 'shared';

@Ng2.Component({
    selector: 'sqx-credentials-page',
    styles,
    template
})
export class CredentialsPageComponent implements Ng2.OnInit {
    private appSubscription: any | null = null;

    constructor(
        private readonly titles: TitleService,
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.titles.setTitle('{appName} | Settings | Credentials', { appName: app.name });
                }
            });
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }
}

