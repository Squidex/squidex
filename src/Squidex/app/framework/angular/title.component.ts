/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { TitleService } from './../services/title.service';

@Ng2.Component({
    selector: 'sqx-title',
    template: ''
})
export class TitleComponent implements Ng2.OnChanges {
    @Ng2.Input()
    public message: any;

    @Ng2.Input()
    public parameter: string;

    @Ng2.Input()
    public value: any;

    constructor(
        private readonly titleService: TitleService
    ) {
    }

    public ngOnChanges() {
        const parameters = {};

        if (this.parameter) {
            if (!this.value) {
                return;
            }

            parameters[this.parameter] = this.value;
        }

        this.titleService.setTitle(this.message, parameters);
    }
}