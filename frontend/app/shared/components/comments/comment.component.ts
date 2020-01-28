/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { CommentDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-comment',
    styleUrls: ['./comment.component.scss'],
    templateUrl: './comment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommentComponent {
    @Output()
    public delete = new EventEmitter();

    @Input()
    public canDelete = false;

    @Input()
    public canFollow = false;

    @Input()
    public confirmDelete = true;

    @Input()
    public comment: CommentDto;

    @Input()
    public userToken: string;
}