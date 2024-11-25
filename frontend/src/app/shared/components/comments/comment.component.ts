/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AutocompleteComponent, AutocompleteSource, bounceAnimation, ConfirmClickDirective, FromNowPipe, MarkdownDirective, NOOP_DATASOURCE, ScrollActiveDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { CommentItem, CommentsState, DialogService, Keys, StatefulComponent, UpsertCommentForm } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';

interface State {
    mode?: 'Normal' | 'Edit' | 'Reply';
}

@Component({
    standalone: true,
    selector: 'sqx-comment',
    styleUrls: ['./comment.component.scss'],
    templateUrl: './comment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [
        bounceAnimation,
    ],
    imports: [
        AutocompleteComponent,
        ConfirmClickDirective,
        FromNowPipe,
        MarkdownDirective,
        ReactiveFormsModule,
        RouterLink,
        ScrollActiveDirective,
        TooltipDirective,
        TranslatePipe,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
})
export class CommentComponent extends StatefulComponent<State> {
    @Input({ transform: booleanAttribute })
    public canFollow?: boolean | null;

    @Input({ transform: booleanAttribute })
    public canAnswer?: boolean | null;

    @Input({ transform: booleanAttribute })
    public canDelete?: boolean | null;

    @Input({ transform: booleanAttribute })
    public canEdit?: boolean | null;

    @Input({ transform: booleanAttribute })
    public confirmDelete?: boolean | null = true;

    @Input({ required: true })
    public commentItem!: CommentItem;

    @Input({ required: true })
    public comments!: CommentsState;

    @Input()
    public contributors: AutocompleteSource = NOOP_DATASOURCE;

    @Input()
    public userToken = '';

    @Input()
    public currentUrl = '';

    @Input()
    public scrollContainer?: string;

    public replyForm = new UpsertCommentForm();

    public updateForm = new UpsertCommentForm();

    public isDeletable = false;
    public isEditable = false;

    constructor(
        private readonly dialogs: DialogService,
    ) {
        super({});
    }

    public ngOnChanges() {
        const isMyComment = this.commentItem.comment.user === this.userToken;

        this.isDeletable = isMyComment;
        this.isEditable = isMyComment;
    }

    public startEdit() {
        this.updateForm.load({ text: this.commentItem.comment.text });
        this.next({ mode: 'Edit' });
    }

    public startReply() {
        this.next({ mode: 'Reply' });
    }

    public cancelUpdateOrReply() {
        this.next({ mode: 'Normal' });
    }

    public delete() {
        if (!this.isDeletable && !this.canDelete) {
            return;
        }

        this.comments.remove(this.commentItem.index);
    }

    public reply() {
        if (!this.canAnswer) {
            return;
        }

        const { text } = this.replyForm.submit() || {};

        if (text && text.length > 0 && this.commentItem.comment.id) {
            const replyTo = this.commentItem.comment.id!;

            this.comments.create(this.userToken, text, this.currentUrl, { replyTo });
        }

        this.replyForm.submitCompleted();
        this.cancelUpdateOrReply();
    }

    public update() {
        if (!this.isEditable) {
            return;
        }

        const { text } = this.updateForm.submit() || {};

        if (!text || text.length === 0) {
            this.dialogs.confirm('i18n:comments.deleteConfirmTitle', 'i18n:comments.deleteConfirmText')
                .subscribe(confirmed => {
                    if (confirmed) {
                        this.delete();
                    }
                });
        } else {
            this.comments.update(this.commentItem.index, { text });
        }

        this.updateForm.submitCompleted();
        this.cancelUpdateOrReply();
    }

    public onKeyPressReply(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey) {
            this.reply();
            event.preventDefault();
        }
    }

    public onKeyPressUpdate(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey) {
            this.update();
            event.preventDefault();
        }
    }
}
