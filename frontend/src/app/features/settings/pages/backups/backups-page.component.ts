/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ApiUrlConfig, BackupDto, BackupsState, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, Subscriptions, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { BackupComponent } from './backup.component';

@Component({
    selector: 'sqx-backups-page',
    styleUrls: ['./backups-page.component.scss'],
    templateUrl: './backups-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        TooltipDirective,
        ShortcutDirective,
        NgIf,
        ListViewComponent,
        NgFor,
        BackupComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class BackupsPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly backupsState: BackupsState,
    ) {
    }

    public ngOnInit() {
        this.backupsState.load(true);

        this.subscriptions.add(timer(3000, 3000).pipe(switchMap(() => this.backupsState.load(false, true))));
    }

    public reload() {
        this.backupsState.load(true, false);
    }

    public start() {
        this.backupsState.start();
    }

    public trackByBackup(_index: number, item: BackupDto) {
        return item.id;
    }
}
