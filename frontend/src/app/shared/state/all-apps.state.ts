/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { EMPTY, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, getPagingInfo, ListState, shareSubscribed, State } from '@app/framework';
import { AppDto } from '../model';
import { AppsService } from '../services/apps.service';

interface Snapshot extends ListState<string> {
    // The current page of all apps.
    apps: ReadonlyArray<AppDto>;

    // True, if all apps of the installation are returned and not only the apps of the user.
    containsAllApps?: boolean;
}

@Service()
export class AllAppsState extends State<Snapshot> {
    private readonly appsService = inject(AppsService);
    private readonly dialogs = inject(DialogService);

    public readonly apps =
        this.project(x => x.apps);

    public readonly paging =
        this.project(x => getPagingInfo(x, x.apps.length));

    public readonly query =
        this.project(x => x.query);

    public readonly total =
        this.project(x => x.total);

    public readonly containsAllApps =
        this.project(x => x.containsAllApps === true);

    public readonly isLoaded =
        this.project(x => x.isLoaded === true);

    public readonly isLoading =
        this.project(x => x.isLoading === true);

    constructor() {
        super({ apps: [], page: 0, pageSize: 30, total: 0 });

        debug(this, 'allApps');
    }

    public load(): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        const { page, pageSize, query } = this.snapshot;

        return this.appsService.getAllApps(pageSize, pageSize * page, query).pipe(
            tap(({ items: apps, total, containsAllApps }) => {
                this.next({
                    apps,
                    containsAllApps,
                    isLoaded: true,
                    isLoading: false,
                    total,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public search(query: string): Observable<any> {
        if (!this.next({ query: query?.trim() || undefined, page: 0 }, 'Loading Search')) {
            return EMPTY;
        }

        return this.load();
    }

    public page(paging: { page: number; pageSize: number }): Observable<any> {
        if (!this.next(paging, 'Loading Paged')) {
            return EMPTY;
        }

        return this.load();
    }
}
