/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
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
export class SchemasPageComponent extends AppComponentBase {
    public schemasFilter = new FormControl();
    public schemasFiltered =
        Observable.of(null)
            .merge(this.schemasFilter.valueChanges)
            .distinctUntilChanged()
            .debounceTime(300)
            .combineLatest(this.loadSchemas(),
                (query, schemas) => {
                    this.schemasFilter.setValue(query);

                    schemas = schemas.filter(t => t.isPublished);

                    if (query && query.length > 0) {
                        schemas = schemas.filter(t => t.name.indexOf(query) >= 0);
                    }

                    return schemas;
            }).map(schemas => {
                return schemas.sort((a, b) => {
                    if (a.name < b.name) {
                        return -1;
                    }
                    if (a.name > b.name) {
                        return 1;
                    }
                    return 0;
                });
            });

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly schemasService: SchemasService
    ) {
        super(apps, notifications, users);
    }

    private loadSchemas(): Observable<SchemaDto[]> {
        return this.appName()
            .switchMap(app => this.schemasService.getSchemas(app).retry(2))
            .catch(error => {
                this.notifyError(error);
                return [];
            });
    }
}

