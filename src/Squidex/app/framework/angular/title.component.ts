/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnChanges } from '@angular/core';

import { TitleService } from './../services/title.service';

@Component({
    selector: 'sqx-title',
    template: ''
})
export class TitleComponent implements OnChanges {
    @Input()
    public message: any;

    @Input()
    public parameter: string;

    @Input()
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