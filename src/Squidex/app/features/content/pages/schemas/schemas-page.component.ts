/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
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
export class SchemasPageComponent extends AppComponentBase implements OnInit {
    public schemasFiltered =
        this.route.queryParams.map(q => q['schemaQuery'])
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

    public schemasFilter = new FormControl();

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasService: SchemasService
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.schemasFilter.valueChanges.distinctUntilChanged().debounceTime(100)
            .subscribe(f => {
                this.router.navigate([], { queryParams: { schemaQuery: f }});
            });
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

