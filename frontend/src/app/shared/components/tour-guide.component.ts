/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ScrollActiveDirective, TranslatePipe } from '@app/framework';
import { fadeAnimation, StatefulComponent, Subscriptions, TaskSnapshot, TourService, TourState } from '@app/shared/internal';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    standalone: true,
    selector: 'sqx-tour-guide',
    styleUrls: ['./tour-guide.component.scss'],
    templateUrl: './tour-guide.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ScrollActiveDirective,
        TranslatePipe,
    ],
})
export class TourGuideComponent extends StatefulComponent<State> implements OnInit {
    private readonly subscriptions = new Subscriptions();

    constructor(
        public readonly tourState: TourState,
        public readonly tourService: TourService,
    ) {
        super({ isCollapsed: false });
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.tourService.stepShow$
                .subscribe(() => {
                    this.next({ isCollapsed: true });
                }));

        this.subscriptions.add(
            this.tourService.end$
                .subscribe(() => {
                    this.next({ isCollapsed: false });
                }));
    }

    public toggle() {
        this.next(s => ({ isCollapsed: !s.isCollapsed }));
    }

    public complete() {
        this.tourState.complete();
    }

    public start(task: TaskSnapshot) {
        if (!task.isActive) {
            return;
        }

        this.tourState.runTask(task);
    }
}
