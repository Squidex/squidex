
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DateTime,
    DialogService,
    Form,
    ImmutableArray,
    State,
    ValidatorsEx
} from '@app/framework';

import {
    AppDto,
    AppsService,
    CreateAppDto
} from './../services/apps.service';

const FALLBACK_NAME = 'my-app';

export class CreateAppForm extends Form<FormGroup> {
    public appName =
        this.form.controls['name'].valueChanges.map(n => n || FALLBACK_NAME)
            .startWith(FALLBACK_NAME);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]
            ]
        }));
    }
}

interface Snapshot {
    apps: ImmutableArray<AppDto>;

    selectedApp: AppDto | null;
}

@Injectable()
export class AppsState extends State<Snapshot> {
    public get appName() {
        return this.snapshot.selectedApp!.name;
    }

    public selectedApp =
        this.changes.map(s => s.selectedApp)
            .distinctUntilChanged();

    public apps =
        this.changes.map(s => s.apps)
            .distinctUntilChanged();

    constructor(
        private readonly appsService: AppsService,
        private readonly dialogs: DialogService
    ) {
        super({ apps: ImmutableArray.empty(), selectedApp: null });
    }

    public select(name: string | null): Observable<AppDto | null> {
        const observable =
            !name ?
                Observable.of(null) :
                Observable.of(this.snapshot.apps.find(x => x.name === name) || null);

        return observable
            .do(selectedApp => {
                this.next(s => ({ ...s, selectedApp }));
            });
    }

    public load(): Observable<any> {
        return this.appsService.getApps()
            .do(dtos => {
                this.next(s => {
                    const apps = ImmutableArray.of(dtos);

                    return { ...s, apps };
                });
            });
    }

    public create(request: CreateAppDto, now?: DateTime): Observable<AppDto> {
        return this.appsService.postApp(request)
            .do(dto => {
                this.next(s => {
                    const apps = s.apps.push(dto).sortByStringAsc(x => x.name);

                    return { ...s, apps };
                });
            });
    }

    public delete(name: string): Observable<any> {
        return this.appsService.deleteApp(name)
            .do(app => {
                this.next(s => {
                    const apps = s.apps.filter(x => x.name !== name);

                    const selectedApp = s.selectedApp && s.selectedApp.name === name ? null : s.selectedApp;

                    return { ...s, apps, selectedApp };
                });
            })
            .notify(this.dialogs);
    }
}