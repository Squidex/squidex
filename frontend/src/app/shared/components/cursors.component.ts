/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { map } from 'rxjs';
import { StringColorPipe } from '@app/framework';
import { CollaborationService, Profile } from '@app/shared/internal';

type CursorState = { user: Profile; cursor: { x: number; y: number } };

@Component({
    standalone: true,
    selector: 'sqx-cursors',
    styleUrls: ['./cursors.component.scss'],
    templateUrl: './cursors.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        StringColorPipe,
    ],
})
export class CursorsComponent  {
    public otherCursor =
        this.collaboration.userChanges
            .pipe(map(x => x.filter(s => !!s['cursor']) as CursorState[]));

    constructor(
        public readonly collaboration: CollaborationService,
    ) {
    }

    public sizeInPx(value: number) {
        return `${value}px`;
    }
}
