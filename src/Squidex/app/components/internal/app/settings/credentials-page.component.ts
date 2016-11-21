/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { 
    AppsStoreService,
    AppClientKeyDto,
    AppClientKeysService,    
    TitleService 
} from 'shared';

@Ng2.Component({
    selector: 'sqx-credentials-page',
    styles,
    template
})
export class CredentialsPageComponent implements Ng2.OnInit {
    private appSubscription: any | null = null;
    private appName: string | null = null;

    public appClientKeys: AppClientKeyDto[] = [];

    constructor(
        private readonly titles: TitleService,
        private readonly appsStore: AppsStoreService,
        private readonly appClientKeysService: AppClientKeysService
    ) {
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.appName = app.name;

                    this.titles.setTitle('{appName} | Settings | Credentials', { appName: app.name });

                    this.appClientKeysService.getClientKeys(app.name).subscribe(clientKeys => {
                        this.appClientKeys = clientKeys;
                    });
                }
            });
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public createClientKey() {
        this.appClientKeysService.postClientKey(this.appName).subscribe(clientKey => {
            this.appClientKeys.push(clientKey);
        })
    }
}

