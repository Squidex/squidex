/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, signal, inject } from '@angular/core';
import { map, merge } from 'rxjs';
import { ScrollActiveDirective, TranslatePipe } from '@app/framework';
import { TaskSnapshot, TourService, TourState } from '@app/shared/internal';

@Component({
    selector: 'sqx-tour-guide',
    styleUrls: ['./tour-guide.component.scss'],
    templateUrl: './tour-guide.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ScrollActiveDirective,
        TranslatePipe,
    ],
})
export class TourGuideComponent {
    protected readonly tourState = inject(TourState);

    protected readonly isCollapsed = signal(false);

    constructor() {
        const tourService = inject(TourService);

        effect(onCleanup => {
            const subscription =
                merge(
                    tourService.stepShow$.pipe(map(() => true)),
                    tourService.end$.pipe(map(() => false)),
                ).subscribe(isCollapsed => {
                    this.isCollapsed.set(isCollapsed);
                });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected toggle() {
        this.isCollapsed.update(x => !x);
    }

    protected complete() {
        this.tourState.complete();
    }

    protected start(task: TaskSnapshot) {
        this.tourState.runTask(task);
    }
}
