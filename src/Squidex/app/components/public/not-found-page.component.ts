/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { TitleService } from './../../framework';

@Ng2.Component({
    selector: 'not-found',
    template
})
export class NotFoundPageComponent implements Ng2.OnInit {
    constructor(
        private readonly title: TitleService
    ) {
    }

    public ngOnInit() {
        this.title.setTitle('Not found');
    }
}