/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
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
    private readonly route = inject(ActivatedRoute);
    private readonly contentStates = inject(ContentsState);

    protected readonly commentsId = this.route.parent!.params.pipe(map(x => x['contentId']));
    protected readonly commentsTab = this.route.queryParams.pipe(map(x => x['commentsTab'] as Tab || 'unresolved'));

    protected readonly content = this.contentStates.selectedContent;
}
