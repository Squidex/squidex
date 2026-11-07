/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, linkedSignal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
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
    private readonly tourService = inject(TourService);

    protected readonly tourState = inject(TourState);

    protected readonly isCollapsed = linkedSignal(() => this.tourCollapsed());

    protected readonly tourCollapsed = toSignal(
        merge(
            this.tourService.stepShow$.pipe(map(() => true)),
            this.tourService.end$.pipe(map(() => false)),
        ),
        { initialValue: false, equal: () => false });

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
