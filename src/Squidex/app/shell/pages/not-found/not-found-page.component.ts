/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import { TitleService } from 'shared';

@Component({
    selector: 'sqx-not-found-page',
    styleUrls: ['./not-found-page.component.scss'],
    templateUrl: './not-found-page.component.html'
})
export class NotFoundPageComponent implements OnInit {
    constructor(
        private readonly title: TitleService
    ) {
    }

    public ngOnInit() {
        this.title.setTitle('Not found');
    }
}