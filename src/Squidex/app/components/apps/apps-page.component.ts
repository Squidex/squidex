/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { fadeAnimation, ModalView } from './../../framework';

@Ng2.Component({
    selector: 'sqx-apps-page',
    styles,
    template,
    animations: [
        fadeAnimation()
    ]
})
export class AppsPageComponent {
    public modalDialog = new ModalView();
}