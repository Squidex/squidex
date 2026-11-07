/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { ScrollActiveDirective, TranslatePipe } from '@app/framework';
import { Subscriptions, TaskSnapshot, TourService, TourState } from '@app/shared/internal';

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
    private readonly subscriptions = new Subscriptions();

    public readonly isCollapsed = signal(false);

    constructor(
        public readonly tourState: TourState,
        public readonly tourService: TourService,
    ) {
        this.subscriptions.add(
            tourService.stepShow$
                .subscribe(() => {
                    this.isCollapsed.set(true);
                }));

        this.subscriptions.add(
            tourService.end$
                .subscribe(() => {
                    this.isCollapsed.set(false);
                }));
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
