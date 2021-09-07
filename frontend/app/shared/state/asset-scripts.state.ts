/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, Resource, shareSubscribed, State, Version } from '@app/framework';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { AppsService, AssetScripts, AssetScriptsPayload } from '../services/apps.service';
import { AppsState } from './apps.state';

interface Snapshot {
    // The current scripts.
    scripts: AssetScripts;

    // The app version.
    version: Version;

    // Indicates if the scripts are loaded.
    isLoaded?: boolean;

    // Indicates if the scripts are loading.
    isLoading?: boolean;

    // Indicates if the user can update the scripts.
    canUpdate?: boolean;

    // The current resource.
    resource: Resource;
}

@Injectable()
export class AssetScriptsState extends State<Snapshot> {
    public scripts =
        this.project(x => x.scripts);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canUpdate =
        this.project(x => x.canUpdate === true);

    constructor(
        private readonly appsState: AppsState,
        private readonly appsService: AppsService,
        private readonly dialogs: DialogService,
    ) {
        super({ scripts: {}, resource: { _links: {} }, version: Version.EMPTY });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.appsService.getAssetScripts(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:assetScripts.reloaded');
                }

                this.replaceAssetScripts(payload, version);
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(request: AssetScripts): Observable<any> {
        return this.appsService.putAssetScripts(this.appName, this.snapshot.resource, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceAssetScripts(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceAssetScripts(payload: AssetScriptsPayload, version: Version) {
        const { canUpdate, scripts } = payload;

        this.next({
            canUpdate,
            scripts,
            isLoaded: true,
            isLoading: false,
            resource: payload,
            version,
        }, 'Loading Success / Updated');
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}
