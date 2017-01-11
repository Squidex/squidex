/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
    ImmutableArray,
    NotificationService,
    SchemaDto,
    SchemasService,
    UsersProviderService
} from 'shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html'
})
export class SchemasPageComponent extends AppComponentBase implements OnInit {
    public schemas = new BehaviorSubject(ImmutableArray.empty<SchemaDto>());
    public schemasFilter = new FormControl();
    public schemasFiltered =
        Observable.of(null)
            .merge(this.schemasFilter.valueChanges.debounceTime(100))
            .combineLatest(this.schemas,
                (query, schemas) => {

                schemas = schemas.filter(t => t.isPublished);

                if (query && query.length > 0) {
                    schemas = schemas.filter(t => t.name.indexOf(query) >= 0);
                }

                schemas =
                    schemas.sort((a, b) => {
                        if (a.name < b.name) {
                            return -1;
                        }
                        if (a.name > b.name) {
                            return 1;
                        }
                        return 0;
                    });

                return schemas;
            });

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly schemasService: SchemasService
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appName()
            .switchMap(app => this.schemasService.getSchemas(app).retry(2))
            .subscribe(dtos => {
                this.schemas.next(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }
}

