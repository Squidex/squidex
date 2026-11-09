/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ConfirmClickDirective, JobsState, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { JobComponent } from './job.component';

@Component({
    selector: 'sqx-jobs-page',
    styleUrls: ['./jobs-page.component.scss'],
    templateUrl: './jobs-page.component.html',
    imports: [
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
    protected readonly jobsState = inject(JobsState);

    constructor() {
        this.jobsState.load();

        timer(3000, 3000).pipe(
            switchMap(() => this.jobsState.load(false, true)),
            takeUntilDestroyed())
            .subscribe();
    }

    protected reload() {
        this.jobsState.load(true, false);
    }

    protected startBackup() {
        this.jobsState.startBackup();
    }

    protected rebuildTextIndex() {
        this.jobsState.rebuildTextIndex();
    }
}
