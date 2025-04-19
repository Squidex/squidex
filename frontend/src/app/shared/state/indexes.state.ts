/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, LoadingState, shareSubscribed, State } from '@app/framework';
import { CreateIndexDto, IndexDto } from '../model';
import { IndexesService } from '../services/indexes.service';
import { AppsState } from './apps.state';
import { SchemasState } from './schemas.state';

interface Snapshot extends LoadingState {
    // The current indexes.
    indexes: ReadonlyArray<IndexDto>;

    // Indicates if the user can add an index.
    canCreate?: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class IndexesState extends State<Snapshot> {
    public indexes =
        this.project(x => x.indexes);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    public get schemaId() {
        return this.schemasState.schemaId;
    }

    public get schemaName() {
        return this.schemasState.schemaName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly schemasState: SchemasState,
        private readonly indexesService: IndexesService,
        private readonly dialogs: DialogService,
    ) {
        super({ indexes: [] });

        debug(this, 'indexes');
    }

    public load(isReload = false, silent = false): Observable<any> {
        if (isReload && !silent) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload, silent);
    }

    private loadInternal(isReload: boolean, silent: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Success');

        return this.indexesService.getIndexes(this.appName, this.schemasState.schemaName).pipe(
            tap(payload => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('i18n:schemas.indexes.reloaded');
                }

                const { canCreate, items: indexes } = payload;

                this.next({
                    canCreate,
                    isLoaded: true,
                    isLoading: false,
                    indexes,
                }, 'Loading Success / Updated');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public create(request: CreateIndexDto): Observable<any> {
        return this.indexesService.postIndex(this.appName, this.schemaName, request).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:schemas.indexes.created');
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(index: IndexDto): Observable<any> {
        return this.indexesService.deleteIndex(this.appName, index).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:schemas.indexes.deleted');
            }),
            shareSubscribed(this.dialogs));
    }
}
