/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { DialogService, shareSubscribed, State, Types } from '@app/framework';
import { AppDto, AppSettingsDto, AppsService, CreateAppDto, UpdateAppDto, UpdateAppSettingsDto } from './../services/apps.service';

interface Snapshot {
    // All apps, loaded once.
    apps: ReadonlyArray<AppDto>;

    // The selected app.
    selectedApp: AppDto | null;

    // The selected app settings.
    selectedSettings: AppSettingsDto | null;
}

@Injectable()
export class AppsState extends State<Snapshot> {
    public apps =
        this.project(s => s.apps);

    public selectedApp =
        this.project(s => s.selectedApp);

    public selectedSettings =
        this.project(s => s.selectedSettings);

    public get appId() {
        return this.snapshot.selectedApp?.id || '';
    }

    public get appName() {
        return this.snapshot.selectedApp?.name || '';
    }

    constructor(
        private readonly appsService: AppsService,
        private readonly dialogs: DialogService,
    ) {
        super({
            apps: [],
            selectedApp: null,
            selectedSettings: null,
        }, 'Apps');
    }

    public reloadApps() {
        return this.loadApp(this.appName).pipe(
            shareSubscribed(this.dialogs));
    }

    public select(name: string | null): Observable<AppDto | null> {
        return this.loadApp(name, true).pipe(
            switchMap(selectedApp => {
                return this.loadSettingsCore(selectedApp).pipe(
                    map(selectedSettings => ({ selectedApp, selectedSettings })));
            }),
            tap(changes => {
                this.next(changes, 'Selected');
            }),
            map(changes => changes.selectedApp));
    }

    public loadApp(name: string | null, cached = false) {
        if (!name) {
            return of(null);
        }

        if (cached) {
            const found = this.snapshot.apps.find(x => x.name === name);

            if (found) {
                return of(found);
            }
        }

        return this.appsService.getApp(name).pipe(
            tap(app => {
                this.replaceApp(app);
            }),
            catchError(() => of(null)));
    }

    public load(): Observable<any> {
        return this.appsService.getApps().pipe(
            tap(apps => {
                this.next(s => {
                    let selectedApp = s.selectedApp;

                    if (selectedApp) {
                        selectedApp = apps.find(x => x.id === selectedApp!.id) || selectedApp;
                    }

                    return { ...s, apps, selectedApp };
                }, 'Loading Success');
            }),
            shareSubscribed(this.dialogs));
    }

    public loadSettings(isReload = false): Observable<any> {
        return this.loadSettingsCore(this.snapshot.selectedApp).pipe(
            tap(settings => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:appSettings.reloaded');
                }

                this.replaceAppSettings(settings);
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateAppDto): Observable<AppDto> {
        return this.appsService.postApp(request).pipe(
            tap(created => {
                this.next(s => {
                    const apps = [...s.apps, created].sortByString(x => x.displayName);

                    return { ...s, apps };
                }, 'Created');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public update(app: AppDto, request: UpdateAppDto): Observable<AppDto> {
        return this.appsService.putApp(app.name, app, request, app.version).pipe(
            tap(updated => {
                this.replaceApp(updated);
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public transfer(app: AppDto, teamId: string | null): Observable<AppDto> {
        return this.appsService.transferApp(app.name, app, { teamId }, app.version).pipe(
            tap(updated => {
                this.replaceApp(updated);
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public updateSettings(settings: AppSettingsDto, request: UpdateAppSettingsDto): Observable<AppSettingsDto> {
        return this.appsService.putSettings(this.appName, settings, request, settings.version).pipe(
            tap(updated => {
                this.replaceAppSettings(updated);
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public removeImage(app: AppDto): Observable<AppDto> {
        return this.appsService.deleteAppImage(app.name, app, app.version).pipe(
            tap(updated => {
                this.replaceApp(updated);
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public uploadImage(app: AppDto, file: File): Observable<number | AppDto> {
        return this.appsService.postAppImage(app.name, app, file, app.version).pipe(
            tap(updated => {
                if (Types.is(updated, AppDto)) {
                    this.replaceApp(updated);
                }
            }),
            shareSubscribed(this.dialogs));
    }

    public leave(app: AppDto): Observable<any> {
        return this.appsService.leaveApp(app.name, app).pipe(
            tap(() => {
                this.removeApp(app);
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(app: AppDto): Observable<any> {
        return this.appsService.deleteApp(app.name, app).pipe(
            tap(() => {
                this.removeApp(app);
            }),
            shareSubscribed(this.dialogs));
    }

    private loadSettingsCore(app?: AppDto | null): Observable<null | AppSettingsDto> {
        if (!app) {
            return of(null);
        } else {
            return this.appsService.getSettings(app.name);
        }
    }

    private replaceAppSettings(selectedSettings?: AppSettingsDto | null) {
        this.next({ selectedSettings }, 'UpdatedSettings');
    }

    private removeApp(app: AppDto) {
        this.next(s => {
            const apps = s.apps.filter(x => x.name !== app.name);

            const selectedApp =
                s.selectedApp?.id !== app.id ?
                s.selectedApp :
                null;

            return { ...s, apps, selectedApp };
        }, 'Deleted');
    }

    private replaceApp(app: AppDto) {
        this.next(s => {
            const apps = s.apps.replacedBy('id', app);

            const selectedApp =
                s.selectedApp?.id !== app.id ?
                s.selectedApp :
                app;

            return { ...s, apps, selectedApp };
        }, 'Updated');
    }
}
