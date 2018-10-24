/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { allParams } from '@app/shared';

@Component({
    selector: 'sqx-comments-page',
    styleUrls: ['./comments-page.component.scss'],
    templateUrl: './comments-page.component.html'
})
export class CommentsPageComponent implements OnInit {
    public commentsId: string;

    constructor(
        private readonly route: ActivatedRoute
    ) {
    }

    public ngOnInit() {
        this.commentsId = allParams(this.route)['contentId'];
    }
}

