/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, Input, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { Router } from '@angular/router';
import { MentionConfig } from 'angular-mentions';
import { Observable } from 'rxjs';
import { AppsState, AuthService, CollaborationService, Comment, ContributorsState, SharedArray, UpsertCommentForm } from '@app/shared/internal';
import { CommentComponent } from './comment.component';

@Component({
    selector: 'sqx-comments',
    styleUrls: ['./comments.component.scss'],
    templateUrl: './comments.component.html',
    providers: [
        CollaborationService,
    ],
})
export class CommentsComponent {
    @ViewChild('commentsList', { static: false })
    public commentsList!: ElementRef<HTMLDivElement>;

    @ViewChildren(CommentComponent)
    public children!: QueryList<CommentComponent>;

    @Input()
    public commentsId = '';

    public commentsUrl!: string;
    public commentsArray!: Observable<SharedArray<Comment>>;
    public commentForm = new UpsertCommentForm();

    public mentionUsers = this.contributorsState.contributors;
    public mentionConfig: MentionConfig = { dropUp: true, labelKey: 'contributorEmail' };

    public userToken = '';

    constructor(authService: AuthService,
        private readonly appsState: AppsState,
        private readonly collaboration: CollaborationService,
        private readonly contributorsState: ContributorsState,
        private readonly router: Router,
    ) {
        this.userToken = authService.user!.token;
    }

    public ngOnChanges() {
        const basePath = `apps/${this.appsState.appName}/comments2/${this.commentsId}`;

        this.collaboration.connect(basePath);
        this.commentsArray = this.collaboration.getArray<Comment>('stream');
    }

    public scrollDown() {
        if (this.commentsList && this.commentsList.nativeElement) {
            let isEditing = false;

            this.children.forEach(x => {
                isEditing = isEditing || x.snapshot.isEditing;
            });

            if (!isEditing) {
                const height = this.commentsList.nativeElement.scrollHeight;

                this.commentsList.nativeElement.scrollTop = height;
            }
        }
    }

    public delete(comments: SharedArray<Comment>, index: number) {
        comments.remove(index);
    }

    public replace(comments: SharedArray<Comment>, comment: Comment, text: string, index: number) {
        comments.set(index, { ...comment, text });
    }

    public comment(comments: SharedArray<Comment>) {
        const value = this.commentForm.submit();

        if (value?.text && value.text.length > 0) {
            comments.add({ text: value.text, url: this.router.url, time: new Date().toISOString(), user: this.userToken });
        }

        this.commentForm.submitCompleted();
    }

    public trackByComment(index: number) {
        return index;
    }
}
