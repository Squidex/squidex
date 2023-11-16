/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MentionConfig, MentionModule } from 'angular-mentions';
import { ConfirmClickDirective, FocusOnInitDirective, FromNowPipe, MarkdownPipe, SafeHtmlPipe, TooltipDirective, TranslatePipe } from '@app/framework';
import { Comment, ContributorDto, DialogService, Keys, SharedArray, StatefulComponent } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';

interface State {
    isEditing: boolean;
}

@Component({
    standalone: true,
    selector: 'sqx-comment',
    styleUrls: ['./comment.component.scss'],
    templateUrl: './comment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        FocusOnInitDirective,
        FormsModule,
        FromNowPipe,
        MarkdownPipe,
        MentionModule,
        NgIf,
        RouterLink,
        SafeHtmlPipe,
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
    public canDelete?: boolean | null;

    @Input({ transform: booleanAttribute })
    public canEdit?: boolean | null;

    @Input({ transform: booleanAttribute })
    public confirmDelete?: boolean | null = true;

    @Input({ required: true })
    public comment!: Comment;

    @Input({ required: true })
    public commentIndex!: number;

    @Input({ required: true })
    public comments!: SharedArray<Comment>;

    @Input()
    public userToken = '';

    @Input()
    public mentionUsers?: ReadonlyArray<ContributorDto>;

    public mentionConfig: MentionConfig = { dropUp: true, labelKey: 'contributorEmail' };

    public isDeletable = false;
    public isEditable = false;

    public editingText = '';

    constructor(
        private readonly dialogs: DialogService,
    ) {
        super({ isEditing: false });
    }

    public ngOnChanges() {
        const isMyComment = this.comment.user === this.userToken;

        this.isDeletable = isMyComment;
        this.isEditable = isMyComment;
    }

    public startEdit() {
        this.editingText = this.comment.text;

        this.next({ isEditing: true });
    }

    public cancelEdit() {
        this.next({ isEditing: false });
    }

    public delete() {
        if (!this.isDeletable && !this.canDelete) {
            return;
        }

        this.comments.remove(this.commentIndex);
    }

    public updateWhenEnter(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey && !event.defaultPrevented) {
            event.preventDefault();
            event.stopImmediatePropagation();
            event.stopPropagation();

            this.update();
        }
    }

    public update() {
        if (!this.isEditable) {
            return;
        }

        const text = this.editingText;

        if (!text || text.length === 0) {
            this.dialogs.confirm('i18n:comments.deleteConfirmTitle', 'i18n:comments.deleteConfirmText')
                .subscribe(confirmed => {
                    if (confirmed) {
                        this.delete();
                    }
                });
        } else {
            this.comments.set(this.commentIndex, { ...this.comment, text });
            this.cancelEdit();
        }
    }
}
