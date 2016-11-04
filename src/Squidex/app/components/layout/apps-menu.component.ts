/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AppsStoreService } from 'shared';

import { fadeAnimation, ModalView } from 'framework';

const FALLBACK_NAME = 'Apps Overview';

@Ng2.Component({
    selector: 'sqx-apps-menu',
    styles,
    template,
    animations: [
        fadeAnimation()
    ]
})
export class AppsMenuComponent {
    public modalMenu = new ModalView();
    public modalDialog = new ModalView();

    public apps =
        this.appsStore.apps.map(a => a || []);

    public app = 
        this.router.events.switchMap(() => {
        return this.router.routerState.root.params.map(p => {
            return p['my_named'] || FALLBACK_NAME;
        } );
    });

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Ng2Router.Router,
        private readonly route: Ng2Router.ActivatedRoute
    ) {
    }

    public createApp() {
        this.modalMenu.hide();
        this.modalDialog.show();
    }
}