/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
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
    public parameter1: string;

    @Input()
    public parameter2: string;

    @Input()
    public value1: any;

    @Input()
    public value2: any;

    constructor(
        private readonly titleService: TitleService
    ) {
    }

    public ngOnChanges() {
        const parameters = {};

        if (this.parameter1) {
            if (!this.value1) {
                return;
            }

            parameters[this.parameter1] = this.value1;
        }

        if (this.parameter2) {
            if (!this.value2) {
                return;
            }

            parameters[this.parameter2] = this.value2;
        }

        this.titleService.setTitle(this.message, parameters);
    }
}