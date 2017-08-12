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
    DialogService,
    SchemaDto,
    SchemasService
} from 'shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html'
})
export class SchemasPageComponent extends AppComponentBase {
    public schemasFilter = new FormControl();
    public schemasFiltered =
        this.schemasFilter.valueChanges
            .startWith(null)
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

    constructor(apps: AppsStoreService, dialogs: DialogService,
        private readonly schemasService: SchemasService
    ) {
        super(dialogs, apps);
    }

    private loadSchemas(): Observable<SchemaDto[]> {
        return this.appNameOnce()
            .switchMap(app => this.schemasService.getSchemas(app).retry(2))
            .catch(error => {
                this.notifyError(error);
                return [];
            });
    }
}

