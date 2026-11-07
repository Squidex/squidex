/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { AutocompleteComponent, AutocompleteSource, ConfirmClickDirective, FromNowPipe, MarkdownDirective, MessageBus, NOOP_DATASOURCE, ScrollActiveDirective, StopClickDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { CommentItem, CommentSelected, CommentsState, DialogService, Keys, UpsertCommentForm } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';

@Component({
    selector: 'sqx-comment',
    styleUrls: ['./comment.component.scss'],
    templateUrl: './comment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AutocompleteComponent,
        ConfirmClickDirective,
        FormField,
        FormsModule,
        FromNowPipe,
        MarkdownDirective,
        RouterLink,
        ScrollActiveDirective,
        TooltipDirective,
        TranslatePipe,
        UserNameRefPipe,
        UserPictureRefPipe,
        StopClickDirective,
    ],
})
export class CommentComponent {
    private readonly dialogs = inject(DialogService);
    private readonly messageBus = inject(MessageBus);

    /** Shows a link to the page where the comment has been created. */
    public readonly canFollow = input(false, { transform: booleanAttribute });

    /** Allows the user to reply to the comment. */
    public readonly canAnswer = input(false, { transform: booleanAttribute });

    /** Allows the user to delete the comment, even if it is not the own comment. */
    public readonly canDelete = input(false, { transform: booleanAttribute });

    /** Allows the user to edit the own comment. */
    public readonly canEdit = input(false, { transform: booleanAttribute });

    /** Allows the user to resolve the comment. */
    public readonly canResolve = input(false, { transform: booleanAttribute });

    /** Asks for a confirmation before the comment is deleted. */
    public readonly confirmDelete = input(true, { transform: booleanAttribute });

    /** Highlights the comment and scrolls it into view. */
    public readonly isSelected = input(false, { transform: booleanAttribute });

    /** The comment with its replies. */
    public readonly commentItem = input.required<CommentItem>();

    /** The state to create, update and delete comments. */
    public readonly comments = input.required<CommentsState>();

    /** The source for the autocompletion of contributors when editing or replying. */
    public readonly contributors = input<AutocompleteSource>(NOOP_DATASOURCE);

    /** The token of the current user. */
    public readonly userToken = input('');

    /** The URL that is stored with replies. */
    public readonly currentUrl = input('');

    /** The selector of the container to scroll when the comment is selected. */
    public readonly scrollContainer = input<string>();

    protected readonly replyForm = new UpsertCommentForm();

    protected readonly updateForm = new UpsertCommentForm();

    protected readonly mode = signal<'Normal' | 'Edit' | 'Reply'>('Normal');

    protected readonly isMyComment = computed(() => {
        return this.commentItem().comment.user === this.userToken();
    });

    protected readonly isOnlyDelete = computed(() => {
        // A single icon without context is hard to understand, therefore we show a label in this case.
        return !this.canAnswer() && !(this.isMyComment() && this.canEdit()) && !this.canResolve();
    });

    protected select() {
        const editorId = this.commentItem().comment.editorId;
        if (!editorId) {
            return;
        }

        this.messageBus.emit(new CommentSelected(editorId));
    }

    protected startEdit() {
        this.updateForm.load({ text: this.commentItem().comment.text });
        this.mode.set('Edit');
    }

    protected startReply() {
        this.mode.set('Reply');
    }

    protected cancelUpdateOrReply() {
        this.mode.set('Normal');
    }

    protected resolve(isResolved: boolean) {
        if (!this.canResolve()) {
            return;
        }

        this.comments().update(this.commentItem().index, { isResolved });
    }

    protected delete() {
        if (!this.isMyComment() && !this.canDelete()) {
            return;
        }

        this.comments().remove(this.commentItem().index);
    }

    protected reply() {
        if (!this.canAnswer()) {
            return;
        }

        this.replyForm.submit(async ({ text }) => {
            const replyTo = this.commentItem().comment.id;
            if (text && text.length > 0 && replyTo) {
                this.comments().create(this.userToken(), text, this.currentUrl(), { replyTo });
            }
        }, { resetAfterSubmit: true });

        this.cancelUpdateOrReply();
    }

    protected update() {
        if (!this.isMyComment()) {
            return;
        }

        this.updateForm.submit(async ({ text }) => {
            if (!text || text.length === 0) {
                this.dialogs.confirm('i18n:comments.deleteConfirmTitle', 'i18n:comments.deleteConfirmText')
                    .subscribe(confirmed => {
                        if (confirmed) {
                            this.delete();
                        }
                    });
            } else {
                this.comments().update(this.commentItem().index, { text });
            }
        }, { resetAfterSubmit: true });

        this.cancelUpdateOrReply();
    }

    protected onKeyPressReply(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey) {
            this.reply();
            event.preventDefault();
        }
    }

    protected onKeyPressUpdate(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey) {
            this.update();
            event.preventDefault();
        }
    }
}
