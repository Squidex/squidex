/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, signal } from '@angular/core';
import { map, merge } from 'rxjs';
import { ScrollActiveDirective, TranslatePipe } from '@app/framework';
import { TaskSnapshot, TourService, TourState } from '@app/shared/internal';

@Component({
    selector: 'sqx-tour-guide',
    styleUrls: ['./tour-guide.component.scss'],
    templateUrl: './tour-guide.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ScrollActiveDirective,
        TranslatePipe,
    ],
})
export class TourGuideComponent {
    public readonly isCollapsed = signal(false);

    constructor(
        public readonly tourState: TourState,
        public readonly tourService: TourService,
    ) {
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

    public toggle() {
        this.isCollapsed.update(x => !x);
    }

    public complete() {
        this.tourState.complete();
    }

    public start(task: TaskSnapshot) {
        this.tourState.runTask(task);
    }
}
