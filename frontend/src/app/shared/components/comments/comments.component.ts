/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, ElementRef, Input, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MentionConfig, MentionModule } from 'angular-mentions';
import { Observable } from 'rxjs';
import { ResizedDirective, TranslatePipe } from '@app/framework';
import { AuthService, CollaborationService, Comment, ContributorsState, SharedArray, UpsertCommentForm } from '@app/shared/internal';
import { CommentComponent } from './comment.component';

@Component({
    selector: 'sqx-comments',
    styleUrls: ['./comments.component.scss'],
    templateUrl: './comments.component.html',
    standalone: true,
    imports: [
        NgIf,
        ResizedDirective,
        NgFor,
        CommentComponent,
        FormsModule,
        ReactiveFormsModule,
        MentionModule,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class CommentsComponent {
    @ViewChild('scrollContainer', { static: false })
    public scrollContainer!: ElementRef<HTMLDivElement>;

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
        private readonly collaboration: CollaborationService,
        private readonly contributorsState: ContributorsState,
        private readonly router: Router,
    ) {
        this.userToken = authService.user!.token;
    }

    public ngOnChanges() {
        this.commentsArray = this.collaboration.getArray<Comment>('stream');
    }

    public scrollDown() {
        if (this.scrollContainer && this.scrollContainer.nativeElement) {
            let isEditing = false;

            this.children.forEach(x => {
                isEditing = isEditing || x.snapshot.isEditing;
            });

            if (!isEditing) {
                const height = this.scrollContainer.nativeElement.scrollHeight;

                this.scrollContainer.nativeElement.scrollTop = height;
            }
        }
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
