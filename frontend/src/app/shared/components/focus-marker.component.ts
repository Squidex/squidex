/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, input, Renderer2, inject } from '@angular/core';
import { StringColorPipe } from '@app/framework';
import { CollaborationService } from '@app/shared/internal';

@Component({
    selector: 'sqx-focus-marker',
    styleUrls: ['./focus-marker.component.scss'],
    templateUrl: './focus-marker.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        StringColorPipe,
    ],
})
export class FocusMarkerComponent {
    private readonly collaboration = inject(CollaborationService, { optional: true });

    private readonly users = computed(() => {
        return this.collaboration?.users() ?? [];
    });

    /** The ID of the control, to show which other user has focused it. */
    public readonly controlId = input.required<string>();

    protected readonly focusedUser = computed(() => {
        const controlId = this.controlId();

        return this.users().find(x => x['focusedControl'] === controlId)?.user;
    });

    constructor() {
        const renderer = inject(Renderer2);
        const collaboration = this.collaboration;

        effect(onCleanup => {
            if (!collaboration) {
                return;
            }

            const unlisten = renderer.listen('window', 'click', (event: MouseEvent) => {
                this.updateFocusedControl(event, null);
            });

            onCleanup(unlisten);
        });
    }

    protected onClick(event: MouseEvent) {
        this.updateFocusedControl(event, this.controlId());
    }

    private updateFocusedControl(event: MouseEvent, controlId: string | null) {
        if (!this.collaboration) {
            return;
        }

        if ((event as any)['handled'] === true) {
            return;
        }

        this.collaboration.updateAwareness('focusedControl', controlId);

        (event as any)['handled'] = true;
    }
}
