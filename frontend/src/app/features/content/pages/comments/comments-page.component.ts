/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map } from 'rxjs/operators';
import { CommentsComponent, ContentsState, LayoutComponent, TranslatePipe } from '@app/shared';

type Tab = 'unresolved' | 'all';

@Component({
    selector: 'sqx-comments-page',
    styleUrls: ['./comments-page.component.scss'],
    templateUrl: './comments-page.component.html',
    imports: [
        AsyncPipe,
        CommentsComponent,
        LayoutComponent,
        RouterLink,
        TranslatePipe,
    ],
})
export class CommentsPageComponent {
    public commentsId = this.route.parent!.params.pipe(map(x => x['contentId']));
    public commentsTab = this.route.queryParams.pipe(map(x => x['commentsTab'] as Tab || 'unresolved'));

    public readonly content = this.contentStates.selectedContent;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly contentStates: ContentsState,
    ) {
    }
}
