/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ApiUrlConfig, JobsState, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, Subscriptions, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { JobComponent } from './job.component';

@Component({
    standalone: true,
    selector: 'sqx-jobs-page',
    styleUrls: ['./jobs-page.component.scss'],
    templateUrl: './jobs-page.component.html',
    imports: [
        AsyncPipe,
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
export class JobsPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly jobsState: JobsState,
    ) {
    }

    public ngOnInit() {
        this.jobsState.load();

        this.subscriptions.add(
            timer(3000, 3000).pipe(
                switchMap(() => this.jobsState.load(false, true))));
    }

    public reload() {
        this.jobsState.load(true, false);
    }

    public startBackup() {
        this.jobsState.startBackup();
    }
}
