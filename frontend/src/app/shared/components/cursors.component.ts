/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { StringColorPipe } from '@app/framework';
import { CollaborationService, Profile } from '@app/shared/internal';

type CursorState = { user: Profile; cursor: { x: number; y: number } };

@Component({
    selector: 'sqx-cursors',
    styleUrls: ['./cursors.component.scss'],
    templateUrl: './cursors.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        StringColorPipe,
    ],
})
export class CursorsComponent {
    private readonly collaboration = inject(CollaborationService);

    protected readonly otherCursor = computed(() => {
        return this.collaboration.users().filter(s => !!s['cursor']) as CursorState[];
    });

    protected sizeInPx(value: number) {
        return `${value}px`;
    }
}
