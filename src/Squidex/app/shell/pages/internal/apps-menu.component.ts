/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import {
    AppDto,
    AppsStoreService,
    ModalView
} from 'shared';

const FALLBACK_NAME = 'Apps Overview';

@Ng2.Component({
    selector: 'sqx-apps-menu',
    styles,
    template
})
export class AppsMenuComponent implements Ng2.OnInit, Ng2.OnDestroy {
    private appsSubscription: any | null = null;
    private appSubscription: any | null = null;

    public modalMenu = new ModalView(false, true);
    public modalDialog = new ModalView();

    public apps: AppDto[] = [];

    public appName = FALLBACK_NAME;

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Ng2Router.Router,
        private readonly route: Ng2Router.ActivatedRoute
    ) {
    }

    public ngOnInit() {
        this.appsSubscription =
            this.appsStore.apps.subscribe(apps => {
                this.apps = apps || [];
            });

        this.appSubscription =
            this.appsStore.selectedApp.subscribe(selectedApp => this.appName = selectedApp ? selectedApp.name : FALLBACK_NAME);
    }

    public ngOnDestroy() {
        this.appsSubscription.unsubscribe();
        this.appSubscription.unsubscribe();
    }

    public onAppCreationCancelled() {
        this.modalDialog.hide();
    }

    public onAppCreationCompleted(app: AppDto) {
        this.modalDialog.hide();
    }

    public createApp() {
        this.modalMenu.hide();
        this.modalDialog.show();
    }
}