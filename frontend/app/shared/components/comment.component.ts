/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { CommentDto, UpsertCommentForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-comment',
    styleUrls: ['./comment.component.scss'],
    templateUrl: './comment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommentComponent {
    @Output()
    public delete = new EventEmitter();

    @Output()
    public update = new EventEmitter<string>();

    @Input()
    public comment: CommentDto;

    @Input()
    public userId: string;

    public editForm = new UpsertCommentForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public emitDelete() {
        this.delete.emit();
    }
}