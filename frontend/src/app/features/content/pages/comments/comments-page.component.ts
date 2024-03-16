/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-comments-page',
    styleUrls: ['./comments-page.component.scss'],
    templateUrl: './comments-page.component.html',
})
export class CommentsPageComponent {
    public commentsId = this.route.parent!.params.pipe(map(x => x['contentId']));

    constructor(
        private readonly route: ActivatedRoute,
    ) {
    }
}
