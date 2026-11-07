/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ApiUrlConfig, ConfirmClickDirective, JobsState, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { JobComponent } from './job.component';

@Component({
    selector: 'sqx-jobs-page',
    styleUrls: ['./jobs-page.component.scss'],
    templateUrl: './jobs-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        JobComponent,
        LayoutComponent,
        ListViewComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class JobsPageComponent {
    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly jobsState: JobsState,
    ) {
        this.jobsState.load();

        effect(onCleanup => {
            const subscription =
                timer(3000, 3000).pipe(
                    switchMap(() => this.jobsState.load(false, true)))
                .subscribe();

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public reload() {
        this.jobsState.load(true, false);
    }

    public startBackup() {
        this.jobsState.startBackup();
    }

    public rebuildTextIndex() {
        this.jobsState.rebuildTextIndex();
    }
}
