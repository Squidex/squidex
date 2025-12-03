/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, LoadingState, Resource, shareSubscribed, State, VersionTag } from '@app/framework';
import { AssetScriptsDto, UpdateAssetScriptsDto } from '../model';
import { AppsService } from '../services/apps.service';
import { AppsState } from './apps.state';

type ClassPropertiesOnly<T> = {
    [K in keyof T as T[K] extends Function ? never : K]: T[K];
};

interface Snapshot extends LoadingState {
    // The current scripts.
    scripts: Omit<ClassPropertiesOnly<AssetScriptsDto>, 'canUpdate' | 'version' | '_links'>;

    // The app version.
    version: VersionTag;

    // Indicates if the user can update the scripts.
    canUpdate?: boolean;

    // The current resource.
    resource: Resource;
}

@Injectable({
    providedIn: 'root',
})
export class AssetScriptsState extends State<Snapshot> {
    public scripts =
        this.project(x => x.scripts);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canUpdate =
        this.project(x => x.canUpdate === true);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly appsService: AppsService,
        private readonly dialogs: DialogService,
    ) {
        super({ scripts: {}, resource: { _links: {} }, version: VersionTag.EMPTY });

        debug(this, 'assetScripts');
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

    public update(request: UpdateAssetScriptsDto): Observable<any> {
        return this.appsService.putAssetScripts(this.appName, this.snapshot.resource, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceAssetScripts(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceAssetScripts(payload: AssetScriptsDto, version: VersionTag) {
        const { _links: _, version: __, ...scripts } = payload.toJSON();

        this.next({
            canUpdate: payload.canUpdate,
            scripts,
            isLoaded: true,
            isLoading: false,
            resource: payload,
            version,
        }, 'Loading Success / Updated');
    }

    private get version() {
        return this.snapshot.version;
    }
}
