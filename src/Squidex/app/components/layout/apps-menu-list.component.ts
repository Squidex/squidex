/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { AppDto } from 'shared';

@Ng2.Component({
    selector: 'sqx-apps-menu-list',
    styles,
    template
})
export class AppsMenuListComponent {

    @Ng2.Input()
    public apps: AppDto[];
}