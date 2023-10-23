
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { map } from 'rxjs';
import { CollaborationService, Profile } from '@app/shared/internal';

type CursorState = { user: Profile; cursor: { x: number; y: number } };

@Component({
    selector: 'sqx-cursors',
    styleUrls: ['./cursors.component.scss'],
    templateUrl: './cursors.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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

    public trackByUser(_: number, item: { user: Profile }) {
        return item.user.id;
    }
}
