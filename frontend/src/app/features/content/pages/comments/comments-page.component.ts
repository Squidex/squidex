/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommentsComponent, ContentsState, LayoutComponent, queryParam, routeParam, TranslatePipe } from '@app/shared';

type Tab = 'unresolved' | 'all';

@Component({
    selector: 'sqx-comments-page',
    styleUrls: ['./comments-page.component.scss'],
    templateUrl: './comments-page.component.html',
    imports: [
        CommentsComponent,
        LayoutComponent,
        RouterLink,
        TranslatePipe,
    ],
})
export class CommentsPageComponent {
    private readonly contentStates = inject(ContentsState);
    private readonly route = inject(ActivatedRoute);

    protected readonly commentsId = routeParam('contentId', this.route.parent!);

    protected readonly commentsTab = queryParam<Tab>('commentsTab', 'unresolved');

    protected readonly content = this.contentStates.selectedContent;
}
