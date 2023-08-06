/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { fadeAnimation, StatefulComponent, TaskSnapshot, TourState } from '@app/shared/internal';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    selector: 'sqx-tour-guide',
    styleUrls: ['./tour-guide.component.scss'],
    templateUrl: './tour-guide.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TourGuideComponent extends StatefulComponent<State> {
    constructor(
        public readonly tourState: TourState,
    ) {
        super({ isCollapsed: false });
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
